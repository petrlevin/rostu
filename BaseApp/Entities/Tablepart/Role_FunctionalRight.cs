using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Collections.Generic;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data.Objects.DataClasses;
using Platform.PrimaryEntities.Reference;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Common.DbEnums;
namespace BaseApp.Tablepart
{
    /// <summary>
    /// ТЧ Функциональные права
    /// </summary>
    public partial class Role_FunctionalRight
    {

        [Control(ControlType.Insert, Sequence.After, ExecutionOrder = 200)]
        [ControlInitial(ExcludeFromSetup = true)]
        public void CreateStatusesAndOperation(DataContext dataContext)
        {
            if (Entity.IsRefernceWithStatus())
            {
                foreach (RefStatus refStatus in Enum.GetValues(typeof(RefStatus)))
                {
                    dataContext.Role_RefStatus.Add(new Role_RefStatus()
                        {
                            Master = this,
                            Owner = Owner,
                            RefStatus = refStatus,
                            SwitchOn = true
                        }
                        );
                }
            }
            else if (Entity.IdEntityType == (int)EntityType.Document || Entity.IdEntityType == (int)EntityType.Tool)
            {
                var operation = dataContext.EntityOperation.Where(r => r.IdEntity == Entity.Id).ToList()
                                   .Join(dataContext.Operation.ToList(), eo => eo.IdOperation, op => op.Id,
                                         (eo, op) => op).Distinct();

                foreach (var op in operation)
                {
                    dataContext.Role_DocumentOperation.Add(new Role_DocumentOperation()
                        {
                            Master = this,
                            Owner = Owner,
                            Operation = op,
                            SwitchOn = true
                        }
                        );
                }
            }

            dataContext.SaveChanges();

        }
    }
}