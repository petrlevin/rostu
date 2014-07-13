using System;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;

namespace BaseApp.Reference
{
    public partial class Organization : ReferenceEntity 
	{
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_500101(DataContext context)
        {
            if (INN.Length != 10 && INN.Length != 12)
                Controls.Throw("Некорректный ИНН. ИНН должен состоять из 10 или 12 цифр.");
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 20)]
        public void Control_500102(DataContext context)
        {
            if (
                (INN.Length == 10 && (KPP ?? "").Length != 9)
                || (INN.Length == 12 && !string.IsNullOrEmpty(KPP))
            )
                Controls.Throw("КПП необходимо указывать при 10-значном ИНН.<br>КПП должен состоять из 9 цифр.");
        }

        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 30)]
        public void Control_500103(DataContext context)
        {
            bool fail = context.Organization.Any(a =>
                a.IdPublicLegalFormation == IdPublicLegalFormation
                && a.INN == INN
                && (a.KPP ?? "") == (KPP ?? "")
                && a.Caption == Caption
                && a.Id != Id
            );
            if (fail)
                Controls.Throw("В справочнике уже имеется организация с таким же кратким наименованием, ИНН и КПП.");
        }
    }
}

