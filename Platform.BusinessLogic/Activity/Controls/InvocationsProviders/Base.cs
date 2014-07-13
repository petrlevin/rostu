using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity.Controls.InvocationsProviders
{
    /// <summary>
    /// 
    /// </summary>
    public class Base
    {
        
        protected readonly ParameterExpression _controlTypeExpression = Expression.Parameter(typeof(ControlType), "ct");
        protected readonly ParameterExpression _sequenceExpression = Expression.Parameter(typeof(Sequence), "s");
        protected readonly ParameterExpression _oldEntityExpression = Expression.Parameter(typeof(IBaseEntity), "oe");
        protected readonly ParameterExpression _dbContextExpression = Expression.Parameter(typeof(DbContext), "dbc");

        protected Expression BuildDataContextParameter(Type parameterType , string parameterName , MemberInfo control)
        {
                            return            Expression.TryCatch(

                                Expression.Convert(_dbContextExpression, parameterType),
                                Expression.Catch(typeof(Exception),
                                Expression.Throw(Expression.Constant(new ControlDefinitionException(
                                        String.Format(
                                        "Контрол не правильно определен. Параметр {0}  типа '{1}' не может быть инициализирован ",
                                        parameterName, parameterType ), control)), parameterType )
                                ));



        }

        protected static IEnumerable<MemberInfo> GetControls(IEnumerable<MemberInfo> methods, ControlType controlType, Sequence sequence)
        {
            methods = methods
                .Select(
                    mi =>
                    new KeyValuePair<MemberInfo, ControlAttribute>(mi,
                                                                   (ControlAttribute)
                                                                   mi.GetCustomAttribute(
                                                                       typeof(ControlAttribute),
                                                                       true)))
                .Where(kv => (kv.Value.Sequence & sequence) != 0 )
                .Where(kv => (kv.Value.ControlType & controlType) == controlType)
                .Select(kv => kv.Key);
            return methods;
        }

        protected virtual int GetExecutionOrder(MemberInfo mi)
        {
            return ((ControlAttribute)mi.GetCustomAttribute(typeof(ControlAttribute), true)).ExecutionOrder;
        }


    }
}
