using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Tablepart;
using ValueType = Sbor.DbEnums.ValueType;
using SourcesData = Sbor.DbEnums.SourcesDataTools;


namespace Sbor.Tool
{
	/// <summary>
	/// Утверждение балансировки расходов, доходов и ИФДБ
	/// </summary>
	public partial class ApprovalBalancingIFDB
	{
        [ControlInitial(ExcludeFromSetup = true)]
        public void AllToolsInclude(DataContext context)
        {
            var idt = BalancingIFDBs.Select(s => s.Id).ToArray();

            List<string> list = context.BalancingIFDB.Where(a =>
                a.IdSourcesDataTools == IdSourcesDataTools && a.IdDocStatus == DocStatus.Project && !idt.Contains(a.Id)
            ).Select(s => new { 
                s.Caption, s.Number, s.Date 
            }).ToList().Select(s => "«" + s.Caption + "» №" + s.Number + " от " + s.Date.ToString("dd.MM.yyyy")).ToList();

            Controls.Check(list, "Не все инструменты балансировки на статусе «Проект» включены в свод:<br>{0}");
        }

        [ControlInitial(ExcludeFromSetup = true)]
        public void ExistsBlanks(DataContext context)
        {
            if (!Blanks.Any(w => w.IdBlankType == (byte)BlankType.BringingGRBS))
                Controls.Throw("В инструменте не заполнен бланк «Доведение ГРБС».");

            bool fail = BalancingIFDBs.Any(a => a.IdBalancingIFDBType == (byte) BalancingIFDBType.LimitBudgetAllocationsAndActivityOfSBP)
                        && !Blanks.Any(w => w.IdBlankType == (byte)BlankType.FormationGRBS);

            if (fail)
                Controls.Throw("В инструменте не заполнен бланк «Формирование ГРБС».");
        }
    }
}