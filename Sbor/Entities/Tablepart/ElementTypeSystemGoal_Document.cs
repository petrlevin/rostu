using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.DbEnums;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;
using Platform.PrimaryEntities.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Tablepart
{
	public partial class ElementTypeSystemGoal_Document : TablePartEntity 
	{
        private void InitMaps(DataContext context)
        {
            if (Owner == null)
                Owner = context.ElementTypeSystemGoal.SingleOrDefault(a => a.Id == IdOwner);

            if (DocType == null)
                DocType = context.DocType.SingleOrDefault(a => a.Id == IdDocType);
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            if (IsDefault)
            {
                InitMaps(context);

                var q = context.ElementTypeSystemGoal_Document.Where(a => a.IdOwner == IdOwner && a.Id != Id && a.IsDefault);
                if (q.Any())
                {
                    foreach (ElementTypeSystemGoal_Document d in q)
                    {
                        d.IsDefault = false;
                    }
                    context.SaveChanges();
                }
            }
        }

        //TODO: Старый костыль для проверки удаления строк при нахождении в статусе в работе. Сейчас (3.0.2784) проверка не нужна.
        //[Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 30)]
        //public void Control_500802(DataContext context)
        //{
        //    if (Owner.IdRefStatus == (byte)RefStats.Work)
        //    {
        //        if (!context.ElementTypeSystemGoal_Document.Any(a => a.IdOwner == IdOwner && a.Id != Id))
        //            Controls.Throw("Необходимо указать хотя бы один документ.");
        //    }
        //}

        [Control(ControlType.Delete, Sequence.Before, ExecutionOrder = 40)]
        public void Control_500803(DataContext context)
        {
            SystemGoal obj = context.SystemGoal.FirstOrDefault(a => 
                a.IdElementTypeSystemGoal == IdOwner
				&& a.IdDocType_CommitDoc == IdDocType
            );

            if (obj != null)
            {
                InitMaps(context);

                Controls.Throw(string.Format(
                    "В справочнике «Система целеполагания» обнаружены элементы с типом «{0}», у которых указан удаляемый документ:<br>{1}",
                    obj.Caption,
                    DocType.Caption
                ));
            }
        }
    }
}

