using System;
using System.Linq;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;

namespace BaseApp.Reference
{
    public partial class Budget : ReferenceEntity, IBudget
    {
//        public string Caption {
//            get { return Year.ToString() + " - " + (Year + 2).ToString() + " гг."; }
//        }
        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        //public void Control_501601(DataContext context)
        //{
        //    var exists = context.Budget.Where(a =>
        //                                      a.Year == Year &&
        //                                      (a.IdPublicLegalFormation ?? 0) == (IdPublicLegalFormation ?? 0));
        //    if (exists.Any())
        //    {
        //        var e = exists.FirstOrDefault();
        //        Controls.Throw
        //        ("Для ППО " + e.PublicLegalFormation.Caption + "  уже создан бюджет с годом " + e.Year);
        //    }
        //}
    }
}

