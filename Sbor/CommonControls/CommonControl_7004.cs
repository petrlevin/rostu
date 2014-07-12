using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.BusinessLogic;
using Sbor.Interfaces;
using Sbor.Logic;

namespace Sbor.CommonControls
{
    [ControlInitial(ExcludeFromSetup = false, InitialSkippable = false, InitialManaged = false, InitialUNK = "7004", InitialCaption = "Проверка наличия элементов СЦ из ТЧ Элементы СЦ в других документах СЦ")]
    public class CommonControl_7004 : IFreeCommonControl<IPpoVerDoc, DataContext>
    {
        public void Execute(DataContext context, IPpoVerDoc doc)
        {
            var reg = (from sge in context.SystemGoalElement.Where(r => r.IdTerminator == doc.Id)
                       join sgeN in context.SystemGoalElement.Where(w =>
                                                                    w.IdPublicLegalFormation ==
                                                                    doc.IdPublicLegalFormation &&
                                                                    w.IdVersion == doc.IdVersion &&
                                                                    !w.IdTerminator.HasValue)
                           on sge.IdSystemGoal equals sgeN.IdSystemGoal
                       select new {goal = sge.SystemGoal.Caption, sgeN.IdRegistrator, sgeN.IdRegistratorEntity})
                .ToList()
                .OrderBy(o => o.goal);

            if (reg.Any())
            {
                var errstr = new StringBuilder();

                foreach (var docent in reg.Select(s => s.IdRegistratorEntity).Distinct())
                {
                    var table = context.Set<IHierarhy>(docent);

                    foreach (var d in reg.Where(r => r.IdRegistratorEntity == docent))
                    {
                        var doc0 = table.FirstOrDefault(r => r.Id == d.IdRegistrator);

                        errstr.AppendFormat("{0} <br> - {1}<br>", d.goal, doc0.ToString());
                    }
                }

                Controls.Throw("Следующие элементы СЦ уже добавлены в другие документы системы целеполагания:<br>"+ errstr);
            }
            
        }
    }
}