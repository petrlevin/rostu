using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Caching;
using Platform.Caching.Common;

namespace Platform.BusinessLogic.Activity.Values
{
    /// <summary>
    /// Устанавливает значения для объекта
    /// </summary>
    public class Setter
    {


        private readonly bool _ignoreAbsentProperty;
        private bool _emptyStringAsNullOnConvertError;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valuesContainer"></param>
        /// <param name="values"></param>
        /// <exception cref="ValueExecutionException"></exception>
        /// <exception cref="ValueResolutionException"></exception>

        public void Set(object valuesContainer, IDictionary<string, object> values)
        {
            foreach (KeyValuePair<string, object> keyValuePair in values)
            {
                Set(valuesContainer, keyValuePair.Key, keyValuePair.Value);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valuesContainer"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <param name="valueType"></param>
        /// <exception cref="ValueExecutionException"></exception>
        /// <exception cref="ValueResolutionException"></exception>
        public void Set(object valuesContainer, string valueName, object value, Type valueType = null)
        {
            if (value is DBNull)
                value = null;
            //var actualObject = value==null?null:(value.GetType()==typeof(Int64))?Convert.ChangeType(value,decimal)

            if (valueType == null)
                valueType = (value != null) ? value.GetType() : null;

            if (value != null && (valueType == typeof (DateTime) || valueType.UnderlyingSystemType == typeof (DateTime)))
            {
                DateTime temp;

                if (DateTime.TryParseExact(value.ToString(), "ddd, d MMM yyyy HH:mm:ss UTC", CultureInfo.InvariantCulture, new DateTimeStyles(), out temp) )
                {
                    value = temp.ToUniversalTime().ToString();
                }
                        
            }

            var containerType = ObjectContext.GetObjectType(valuesContainer.GetType());
            var inv = GetInvocation(containerType, valueName, (value != null) ? value.GetType() : null);
            try
            {
                inv.Action(valuesContainer, value);
            }
            catch (ValueConvertException ex)
            {
                if ((_emptyStringAsNullOnConvertError) && (value is string) && ((string)value == String.Empty))
                    Set(valuesContainer, valueName, null);
                else
                    throw new ValueConvertException((value != null) ?
                        String.Format(
                                "Невозможно установить  значение свойству '{0}' объекта типа '{1}' .Переданное значение  -  '{2}'. Тип переданного значения - '{3}' . Тип свойства - '{4}'",
                                inv.MemberInfo.Name, containerType, value, value.GetType(), inv.MemberInfo.PropertyType)
                                : String.Format(
                                "Невозможно установить  значение свойству '{0}' объекта типа '{1}' .Переданное значение  -  null.  Тип свойства - '{2}'",
                                inv.MemberInfo.Name, containerType, inv.MemberInfo.PropertyType),

                        ex.InnerException,
                        inv.MemberInfo,
                        containerType,
                        valueName,
                        value
                        );
            }
            catch (Exception ex)
            {

                throw new ValueExecutionException(String.Format("Ошибка при установлении значения в контейнер значений ('{0}') по имени '{1}'. Установливаемое значение - {2}", containerType, valueName, value), ex, inv.MemberInfo, containerType, valueName);
            }

        }

        #region private

        private Invocation GetInvocation(Type containerType, string valueName, Type valueType)
        {
            var inv = Cache.Get<Invocation>(containerType, valueName, valueType, _ignoreAbsentProperty);
            if (inv == null)
            {
                inv = BuildInvocation(containerType, valueName, valueType);
                Cache.Put(inv, containerType, valueName, valueType, _ignoreAbsentProperty);
            }
            return inv;

        }


        private Invocation BuildInvocation(Type containerType, string valueName, Type valueType)
        {

            ParameterExpression vcExpression = Expression.Parameter(typeof(Object), "vc");
            ParameterExpression vExpression = Expression.Parameter(typeof(Object), "c");


            var memberInfo =
                containerType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                             .FirstOrDefault(
                                 p =>
                                 (p.CanWrite && p.Name.ToLower() == valueName.ToLower() && ((valueType != null)
                                      ? p.PropertyType.IsAssignableFrom(valueType) || CanConvert(valueType, p.PropertyType)
                                      : p.PropertyType.IsClass || p.PropertyType.IsInterface || (Nullable.GetUnderlyingType(p.PropertyType) != null))));
            if (memberInfo == null)
            {
                if (!_ignoreAbsentProperty)
                {
                    var badMember =
                        containerType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     .FirstOrDefault(m => m.Name.ToLower() == valueName.ToLower());


                    throw new ValueResolutionException(
                        badMember == null
                            ? String.Format(
                                "Значение не может быть установлено по имени '{0}' . Контейнер значений типа ('{1}') не содержит соответствующего свойства .",
                                valueName, containerType)
                            : (valueType != null)
                                  ? String.Format(
                                      "Значение не может быть установлено по имени '{0}' . Контейнер значений типа ('{1}') не содержит соответствующего свойства c подходящей сигнатурой. Тип переданного значения - '{2}'. Тип соответствующего свойства - '{3}'",
                                      valueName, containerType, valueType, badMember.PropertyType)
                                  : String.Format(
                                      "Значение не может быть установлено по имени '{0}' . Контейнер значений типа ('{1}') не содержит соответствующего свойства c подходящей сигнатурой.Переданное значение - null. Тип соответствующего свойства - '{2}' ",
                                      valueName, containerType, badMember.PropertyType)
                        , badMember, containerType, valueName);
                }
                else
                {
                    return new Invocation()
                               {
                                   Action = (c, v) => { },
                                   MemberInfo = null
                               };
                }
            }

            Expression valueParam = vExpression;
            if ((valueType != null) && (!memberInfo.PropertyType.IsAssignableFrom(valueType)))
            {
                var catchParam = Expression.Parameter(typeof(Exception), "ex");
                valueParam = Expression.TryCatch(
                    Expression.Call(
                        typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(Object), typeof(Type) }),
                        vExpression,
                        Nullable.GetUnderlyingType(memberInfo.PropertyType) == null
                            ? Expression.Constant(memberInfo.PropertyType)
                            : Expression.Constant(Nullable.GetUnderlyingType(memberInfo.PropertyType))),
                    Expression.Catch(catchParam, Expression.Throw(
                        Expression.New(typeof(ValueConvertException).GetConstructor(new Type[] { typeof(Exception) }), catchParam), typeof(Object)
                                                             )));
            }
            var call = Expression.Call(Expression.Convert(vcExpression, containerType), memberInfo.GetSetMethod(), Expression.Convert(valueParam, memberInfo.PropertyType));

            return new Invocation()
            {
                Action = Expression.Lambda<Action<Object, Object>>(call, vcExpression, vExpression).Compile(),
                MemberInfo = memberInfo

            };
        }


        private bool CanConvert(Type sourceType, Type destinationType)
        {
            return ((typeof(IConvertible).IsAssignableFrom(sourceType)) && (Helper.IsScalar(destinationType)));


        }


        private class Invocation
        {
            public Action<Object, Object> Action { get; set; }
            public PropertyInfo MemberInfo { get; set; }


        }

        private static readonly ISimpleCache Cache = new SimpleCache();


        public Setter(bool ignoreAbsentProperty = false, bool emptyStringAsNullOnConvertError = false)
        {
            _ignoreAbsentProperty = ignoreAbsentProperty;
            _emptyStringAsNullOnConvertError = emptyStringAsNullOnConvertError;
        }

        #endregion
    }
}
