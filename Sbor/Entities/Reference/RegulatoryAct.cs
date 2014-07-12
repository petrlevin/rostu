using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using System.Text.RegularExpressions;

namespace Sbor.Reference
{
    public partial class RegulatoryAct : ReferenceEntity
    {
        private const string CharsForRemove = @"[- '""`.,;:!/\<>~@#$%^&*()\[\]{}]";

        private string GetCaptionKey(string cap)
        {
            return Regex.Replace(cap.ToLower(), CharsForRemove, string.Empty);
        }

        private bool EqualsCaption(string cap1, string cap2)
        {
            return GetCaptionKey(cap1) == GetCaptionKey(cap2);
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500301(DataContext context)
        {
            List<RegulatoryAct> list = context.RegulatoryAct.Where(a =>
				a.IdPublicLegalFormation == IdPublicLegalFormation
				&& a.IdTypeRegulatoryAct == IdTypeRegulatoryAct
                && a.Number  == Number
                && a.Date    == Date
                && a.Id != Id
            ).ToList();

            if (list.Any(a => EqualsCaption(a.Caption, Caption)))
                Controls.Throw("В справочнике уже имеется НПА с таким же номером, наименованием, датой и видом.");
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_500302(DataContext context)
        {
            if (DateStart < Date)
                Controls.Throw("Дата вступления в силу не может быть меньше даты НПА.");
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 30)]
        public void Control_500303(DataContext context)
        {
            if (DateEnd.HasValue && DateEnd.Value < DateStart)
                Controls.Throw("Дата конца срока действия НПА должна быть больше даты вступления в силу.");
        }
    }
}

