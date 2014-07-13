using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ControlInvoker<T> where T : class ,IBaseEntity
    {

        private readonly IControlLauncher _controlLauncher;
        private readonly T _entity;
        private readonly bool _alwaysCompile;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="controlLauncher"></param>
        /// <param name="alwaysCompile"></param>
        public ControlInvoker(T entity, IControlLauncher controlLauncher = null, bool alwaysCompile = false)
        {
            _entity = entity;
            _controlLauncher = controlLauncher ?? IoC.Resolve<IControlLauncher>(ControlLauncher.NameForFreeControlRegistration);
            _alwaysCompile = alwaysCompile;
        }

        internal void InvokeControl<TCommonControl>(T element)
        {
            InvokeControl(typeof (TCommonControl) ,element);
        }

        internal void InvokeControl(Type commonControlType, T element )
        {
            var inv = GetInvocationCommon(commonControlType);

            _controlLauncher.InvokeControl(e => inv((T)e, IoC.Resolve<DbContext>()), element, commonControlType, CommonControlInfo.GetInitial(typeof(T), commonControlType));
        }

        

        /// <summary>
        /// 
        /// </summary>

        /// <param name="controlLambda"></param>
        /// <exception cref="Exception"></exception>
        internal void InvokeControl(Expression<Action<T>> controlLambda)
        {
            if ((controlLambda.NodeType != ExpressionType.Lambda))
                throw Exception(controlLambda);

            var body = controlLambda.Body;
            var call = body as MethodCallExpression;
            if (call == null)
                throw Exception(controlLambda);
            if ((call.Object == null) || (call.Object.NodeType != ExpressionType.Parameter) || (call.Object.Type != typeof(T)))
                throw Exception(controlLambda);
            var args = TryGetArguments(call.Arguments);
            if (args != null)
            {
                var inv = GetInvocation(call.Method, args.Select(kvp => kvp.Key));
                var arguments = args.Select(kvp => kvp.Value).ToArray();
                _controlLauncher.InvokeControl(e => inv((T)e, arguments), _entity, call.Method, ControlInfo.GetInitial(typeof(T), call.Method));

            }
            else
                _controlLauncher.InvokeControl(e => controlLambda.Compile()((T)e), _entity, call.Method, ControlInfo.GetInitial(typeof(T), call.Method));


        }

        private List<KeyValuePair<Type, Object>> TryGetArguments(IEnumerable<Expression> args)
        {
            var result = new List<KeyValuePair<Type, Object>>();
            if (_alwaysCompile)
                return null;

            foreach (var expression in args)
            {
                var ce = expression as ConstantExpression;
                if (ce != null)
                {
                    result.Add(new KeyValuePair<Type, Object>(ce.Type, ce.Value));

                    continue;
                }
                var me = expression as MemberExpression;
                if (me != null)
                {
                    var target = me.Expression as ConstantExpression;
                    if (target == null)
                    {
                        return null;
                    }
                    if (me.Member is FieldInfo)
                        result.Add(new KeyValuePair<Type, Object>(((FieldInfo)me.Member).FieldType,
                                                                  ((FieldInfo)me.Member).GetValue(target.Value)));
                    else if (me.Member is PropertyInfo)
                        result.Add(new KeyValuePair<Type, Object>(((PropertyInfo)me.Member).PropertyType,
                                                                  ((PropertyInfo)me.Member).GetValue(target.Value)));
                    else
                    {
                        return null;
                    }
                    continue;
                }
                return null;
            }
            return result;
        }

        private ControlAction GetInvocation(MethodInfo method, IEnumerable<Type> argsTypes)
        {
            var keys = new List<object>() { method.Name };
            keys.AddRange(argsTypes);

            var result = (ControlAction)Cache.GetByKeys(keys.ToArray());
            if (result == null)
            {
                result = BuildInvocation(method, argsTypes);
                Cache.PutByKeys(result, keys.ToArray());
            }
            return result;
        }

        private ControlAction BuildInvocation(MethodInfo method, IEnumerable<Type> argsTypes)
        {
            var instance = Expression.Parameter(typeof(T), "ent");
            var array = Expression.Parameter(typeof(Object[]));
            var call = Expression.Call(instance, method, argsTypes.Select((t, i) => Expression.Convert(Expression.ArrayIndex(array, Expression.Constant(i)), t)));
            var lparams = new List<ParameterExpression>() { instance, array };
            return Expression.Lambda<ControlAction>(call, lparams).Compile();
        }


        private CommonControlAction GetInvocationCommon(Type control)
        {

            var result = (CommonControlAction)Cache.Get<CommonControlAction>(control);
            if (result == null)
            {
                result = BuildInvocationCommon(control);
                Cache.Put(result,control);
            }
            return result;
        }


        private CommonControlAction BuildInvocationCommon(Type control)
        {
            
            Type controlTargetType;
            Type dbContextType;
            GetControlGenericArgumentTypes(control,out controlTargetType,out dbContextType);
            var instance = Expression.Parameter(typeof(T), "ent");
            var dbContext = Expression.Parameter(typeof(DbContext), "ent");
            var controlInterface = typeof (IFreeCommonControl<,>).MakeGenericType(controlTargetType, dbContextType);
            var call = Expression.Call(
                           Expression.Convert(
                               Expression.New(control),controlInterface),
                           controlInterface.GetMethods()[0],
                           Expression.Convert(dbContext,dbContextType),
                           Expression.Convert(instance, controlTargetType)
                           );

            return Expression.Lambda<CommonControlAction>(call, instance,dbContext).Compile();
        }

        private static void GetControlGenericArgumentTypes(Type control, out Type controlTargetType, out Type dbContextType)
        {
            try
            {
                var @interface = control.GetInterfaces()
                                               .SingleOrDefault(
                                                   i =>
                                                   i.IsGenericType &&
                                                   (i.GetGenericTypeDefinition() == typeof(IFreeCommonControl<,>)) &&
                                                   (typeof(T).InheritsFrom(i.GetGenericArguments()[0])));
                if (@interface == null)
                {
                    if (!control.GetInterfaces()
                                               .Any(
                                                   i =>
                                                   i.IsGenericType &&
                                                   (i.GetGenericTypeDefinition() == typeof(IFreeCommonControl<,>))))
                        throw new ArgumentException(String.Format("Тип {0} не является свободным контролем ('{1}')", control, typeof(IFreeCommonControl<,>)), "commonControlType");
                    else
                    {
                        throw new ArgumentException(String.Format("Тип {0} не может служить свободным контролем для сущности {1}. Сущность {1} не реализует интерфес цели контрола (TTarget) ", control, typeof(T)), "commonControlType");
                    }
                }
                controlTargetType = @interface.GetGenericArguments()[0];
                dbContextType = @interface.GetGenericArguments()[1];

            }
            catch (InvalidOperationException)
            {
                throw new ControlDefinitionException(String.Format("Контроль '{0}' неправильно определен. Класс реализует интерфейс {1}  более чем один раз с обобщенным аргументом который может быть интерпритрован как интерфейс контроля для типа  {2} . Неоднозначность. " ,control,typeof(IFreeCommonControl<,>),typeof(T)),control);
            }
            
        }


        private Exception Exception(Expression<Action<T>> controlLambda)
        {
            return new ArgumentException(String.Format("Для вызова контроля нужно использовать ламбда выражение (entity=>entity.SomeControlMethod(...)). Передано выражение - {0} ", controlLambda), "controlLambda");
        }


        private delegate void ControlAction(T entity, Object[] args);

        private delegate void CommonControlAction(T entity, DbContext dbcontext);


        static private readonly ISimpleCache Cache = new SimpleCache();

    }



}


