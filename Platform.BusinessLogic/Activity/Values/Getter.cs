using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Caching;
using Platform.Caching.Common;

namespace Platform.BusinessLogic.Activity.Values
{
    /// <summary>
    ///  Получение значений свойств
    /// </summary>
    public class Getter
    {
        /// <summary>
        /// Получить  значенния всех свойств объекта тип которых удовлетворят фильтру <paramref name="propertyTypeMatch"/>
        /// </summary>
        /// <param name="valuesContainer"></param>
        /// <param name="propertyTypeMatch">тип свойства (не значения)</param>
        /// <param name="notDefault"></param>
        /// <returns></returns>
        public Dictionary<String, Object> GetAll(object valuesContainer,Func<Type,bool> propertyTypeMatch , bool notDefault )
        {
            var containerType = ObjectContext.GetObjectType(valuesContainer.GetType());
            var inv = GetAllInvocation(containerType, notDefault);
            return inv(valuesContainer, propertyTypeMatch);

        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="valuesContainer"></param>
        /// <param name="valueName"></param>
        /// <param name="options"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public TResult Get<TResult>(object valuesContainer, string valueName, Options options = Options.Default)
        {
            if (((options & Options.UseMethod) != Options.UseMethod) && ((options & Options.UseProperty) != Options.UseProperty))
                throw new ArgumentException("options");

            var containerType = ObjectContext.GetObjectType(valuesContainer.GetType());
            var inv = GetInvocation<TResult>(containerType, valueName, options);
            try
            {
                return inv.Func(valuesContainer);
            }
            catch (Exception ex)
            {

                throw new ValueExecutionException(String.Format("Ошибка при получениии значения из контейнера значений ('{0}') по имени '{1}'" ,containerType,valueName),ex,inv.MemberInfo,containerType,valueName);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="valuesContainer"></param>
        /// <param name="valueName"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ValueExecutionException"></exception>
        public Object Get(object valuesContainer, string valueName, Options options = Options.Default)
        {
            if (((options & Options.UseMethod) != Options.UseMethod) && ((options & Options.UseProperty) != Options.UseProperty))
                throw new ArgumentException("options");

            var containerType = ObjectContext.GetObjectType(valuesContainer.GetType());
            var inv = GetInvocation(containerType, valueName, options);
            try
            {
                return inv.Func(valuesContainer);
            }
            catch (Exception ex)
            {

                throw new ValueExecutionException(String.Format("Ошибка при получениии значения из контейнера значений ('{0}') по имени '{1}'", containerType, valueName), ex, inv.MemberInfo, containerType, valueName);
            }
        }


        #region private


        private Func<Object, Func<Type, bool>, Dictionary<String, Object>> GetAllInvocation(Type containerType, bool notDefault)
        {
            var inv = Cache.Get<Func<Object, Func<Type, bool>, Dictionary<String, Object>>>("All", containerType, notDefault);
            if (inv == null)
            {
                inv = BuildAllInvocation(containerType, notDefault);
                Cache.Put(inv, "All", containerType, notDefault);
            }
            return inv;

        }


        private BindingFlags GetBindingFlags(Options options)
        {
            if ((options & Options.IgnoreCase) == Options.IgnoreCase)
                return BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            else
                return BindingFlags.Instance | BindingFlags.Public;

        }

        private Invocation<TResult> GetInvocation<TResult>(Type containerType, string valueName , Options options)
        {
            var inv = Cache.Get<Invocation<TResult>>(containerType, valueName, options);
            if (inv == null)
            {
                inv = BuildInvocation<TResult>(containerType, valueName, options);
                Cache.Put(inv, containerType, valueName, options);
            }
            return inv;

        }


        private Invocation<Object> GetInvocation(Type containerType, string valueName, Options options)
        {
            var inv = Cache.Get<Invocation<Object>>(containerType, valueName, options,"ObjectResult");
            if (inv == null)
            {
                inv = BuildInvocation(containerType, valueName, options);
                Cache.Put(inv, containerType, valueName, options, "ObjectResult");
            }
            return inv;

        }


        private Invocation<TResult> BuildInvocation<TResult>(Type containerType, string valueName ,Options options)
        {
            
            ParameterExpression vcExpression = Expression.Parameter(typeof(Object), "vc");
            MemberInfo memberInfo = null;
            if ((options & Options.UseMethod) == Options.UseMethod)
                memberInfo = containerType.GetMethods(GetBindingFlags(options)).FirstOrDefault(m => (typeof(TResult).IsAssignableFrom(m.ReturnType))&&!m.GetParameters().Any()&&m.Name==valueName);
            Expression call;
            if (memberInfo != null)
                call = Expression.Call(Expression.Convert(vcExpression, containerType), (MethodInfo)memberInfo);
            else
            {
                if ((options & Options.UseProperty) == Options.UseProperty)
                    memberInfo =
                        containerType.GetProperties(GetBindingFlags(options))
                                     .FirstOrDefault(p => (typeof (TResult).IsAssignableFrom(p.PropertyType)) && p.CanRead &&p.Name==valueName);
                if (memberInfo != null)
                {
                    call = Expression.Call(Expression.Convert(vcExpression,containerType), ((PropertyInfo)memberInfo).GetGetMethod());

                }
                else
                {
                    MemberInfo badMember = null;
                    if ((options & Options.UseMethod) == Options.UseMethod)
                    {
                        badMember =
                            containerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.NonPublic).FirstOrDefault(m => m.Name == valueName);
                    }
                    if ((options & Options.UseProperty) == Options.UseProperty)
                    {
                        badMember = badMember ??
                                    containerType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.NonPublic)
                                                 .FirstOrDefault(m => m.Name == valueName);
                    }

                    throw new ValueResolutionException(
                        String.Format(
                            badMember == null ?
                            "Значение по умолчанию типа '{0}' по имени '{1}' не может быть определено. Контейнер значений типа ('{2}') не содержит соответствующего {3} ."
                            :"Значение по умолчанию типа '{0}' по имени '{1}' не может быть определено. Контейнер значений типа ('{2}') не содержит соответствующего {3} c подходящей сигнатурой",
                            typeof(TResult), valueName, containerType,ExceptionSuffix(options)), badMember, containerType,
                        valueName);

                }
            }
            return new Invocation<TResult>()
                       {
                           Func = Expression.Lambda<Func<Object,TResult>>(call, vcExpression).Compile(),
                           
                           MemberInfo = memberInfo

                       };

        }


        private Func<object, Func<Type, bool>, Dictionary<string, object>> BuildAllInvocation(Type containerType, bool notDefault)
        {
            var expressions = new List<Expression>();
            var resultExpression = Expression.Variable(typeof (Dictionary<string, object>));
            
            expressions.Add(
                Expression.Assign(
                    resultExpression,
                    Expression.New(typeof (Dictionary<string, object>))));
            foreach (PropertyInfo propertyInfo in containerType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
            {
                Expression getProperty = Expression.Call(Expression.Convert(_vcExpression, containerType), propertyInfo.GetGetMethod());
                getProperty = Expression.TryCatch(getProperty,Expression.Catch(typeof(Exception),
                                Expression.Throw(Expression.Constant(new ValueExecutionException(
                                        String.Format(
                                        "Ошибка  при получении значения свойства {0} для {1} . ",
                                        propertyInfo.Name, containerType), propertyInfo,containerType,propertyInfo.Name)), propertyInfo.PropertyType
                                )));

                Expression addToDict = Expression.Call(resultExpression, "Add", null, Expression.Constant(propertyInfo.Name),
                                              Expression.Convert(getProperty, typeof(Object)));
                Expression invoke = addToDict;  
                if (notDefault)
                {
                    var blockExpessions = new List<Expression>();
                    var valueExpression = Expression.Variable(propertyInfo.PropertyType);
                    var assign = Expression.Assign(valueExpression, getProperty);
                    blockExpessions.Add(assign);

                    var ifNotDefault =
                        Expression.IfThen(
                            Expression.NotEqual(valueExpression, Expression.Default(propertyInfo.PropertyType)),
                            addToDict);
                    blockExpessions.Add(ifNotDefault);
                    invoke = Expression.Block(new[] {valueExpression}, blockExpessions);
                }


                var ifMatchTypeThen =
                    Expression.IfThen(
                        Expression.Invoke(_ptmExpression, Expression.Constant(propertyInfo.PropertyType)), invoke);
                expressions.Add(ifMatchTypeThen);

            }
            expressions.Add(resultExpression);
            Expression finalExpression = Expression.Block(new[] { resultExpression },
                                              expressions);
            return
                Expression.Lambda<Func<object, Func<Type, bool>, Dictionary<string, object>>>(finalExpression,
                                                                                              _vcExpression,
                                                                                              _ptmExpression).Compile();
        }


        private Invocation<Object> BuildInvocation(Type containerType, string valueName, Options options)
        {

            
            MemberInfo memberInfo = null;
            if ((options & Options.UseMethod) == Options.UseMethod)
                memberInfo = containerType.GetMethods(GetBindingFlags(options)).FirstOrDefault(m => (m.ReturnType!=typeof(void)) && !m.GetParameters().Any() && m.Name == valueName);
            Expression call;
            if (memberInfo != null)
                call = Expression.Call(Expression.Convert(_vcExpression, containerType), (MethodInfo)memberInfo);
            else
            {
                if ((options & Options.UseProperty) == Options.UseProperty)
                    memberInfo =
                        containerType.GetProperties(GetBindingFlags(options))
                                     .FirstOrDefault(p => (p.CanRead && p.Name == valueName));
                if (memberInfo != null)
                {
                    call = Expression.Call(Expression.Convert(_vcExpression, containerType), ((PropertyInfo)memberInfo).GetGetMethod());

                }
                else
                {
                    MemberInfo badMember = null;
                    if ((options & Options.UseMethod) == Options.UseMethod)
                    {
                        badMember =
                            containerType.GetMethods(BindingFlags.Instance).FirstOrDefault(m => m.Name == valueName);
                    }
                    if ((options & Options.UseProperty) == Options.UseProperty)
                    {
                        badMember = badMember ??
                                    containerType.GetProperties(BindingFlags.Instance)
                                                 .FirstOrDefault(m => m.Name == valueName);
                    }

                    throw new ValueResolutionException(
                        String.Format(
                            badMember == null ?
                            "Значение  не может быть определено по имени '{0}'. Контейнер значений типа ('{1}') не содержит соответствующего {2} ."
                            : "Значение  не может быть определено по имени '{0}'. Контейнер значений типа ('{1}') не содержит соответствующего {2} c подходящей сигнатурой",
                             valueName, containerType, ExceptionSuffix(options)), badMember, containerType,
                        valueName);

                }
            }
            call = Expression.Convert(call, typeof (Object));
            return new Invocation<Object>()
            {
                Func = Expression.Lambda<Func<Object, Object>>(call, _vcExpression).Compile(),

                MemberInfo = memberInfo

            };
        }







        private string ExceptionSuffix(Options options)
        {
            if (((options & Options.UseProperty) == Options.UseProperty) && ((options & Options.UseMethod) == Options.UseMethod))
                return "метода или свойства";
            if ((options & Options.UseProperty) == Options.UseProperty)
                return "свойства";
            return "метода";

        }
        

        private static readonly ISimpleCache Cache = new SimpleCache();

        private class Invocation<TResult>
        {
            public Func<Object,TResult> Func { get; set; }
            public MemberInfo MemberInfo { get; set; }


        }


        readonly ParameterExpression _vcExpression = Expression.Parameter(typeof(Object), "vc");
        readonly ParameterExpression _ptmExpression = Expression.Parameter(typeof(Func<Type, bool>), "ptm");


#endregion

    }
}
