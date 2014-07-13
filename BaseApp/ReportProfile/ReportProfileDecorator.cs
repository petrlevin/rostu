using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using BaseApp.DbEnums;
using BaseApp.Environment;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;

namespace BaseApp.ReportProfile
{
    /// <summary>
    /// 
    /// </summary>
    public class ReportProfileDecorator : AddWhere
    {
        /// <summary>
        /// 
        /// </summary>
        public ReportProfileDecorator()
        {
            var currentUser =  BaseAppEnvironment.Instance.SessionStorage.CurrentUser;
            _filter = new FilterConditions()
                {
                    Type = LogicOperator.And,
                    Operands = new List<IFilterConditions>()
                        {
                            new FilterConditions()
                                {
                                    Field = "isTemporary",
                                    Operator = ComparisionOperator.Equal,
                                    Value = false
                                },
                            new FilterConditions()
                                {
                                    Type = LogicOperator.Or,
                                    Operands = new List<IFilterConditions>()
                                        {
                                            new FilterConditions()
                                                {
                                                    Field = "idReportProfileType",
                                                    Operator = ComparisionOperator.Equal,
                                                    Value = (int) ReportProfileType.Public
                                                },
                                            new FilterConditions()
                                                {
                                                    Field = "idReportProfileUser",
                                                    Operator = ComparisionOperator.Equal,
                                                    Value = currentUser.Id
                                                }
                                        }
                                }
                        }
                };
        }

        
    }
}
