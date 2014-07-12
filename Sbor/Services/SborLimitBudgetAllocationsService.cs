using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.AppServices;
using Platform.Common;
using Sbor.DbEnums;
using Sbor.Logic;
using Sbor.Tablepart;

namespace Sbor.Services
{
    /// <summary>
    /// Сервисы ЭД Предельные объемы бюджетных ассигнований
    /// </summary>
    [AppService]
    public class SborLimitBudgetAllocationsService
    {
        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_Sbpblank(int idSbp, int idBudget)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var sbpBlank = new SBP_Blank();

            var sbp = context.SBP.FirstOrDefault(s => s.Id == idSbp);
            if (sbp == null)
                return null;

            if (sbp.SBPType == SBPType.GeneralManager)
                sbpBlank = sbp.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingGRBS && b.IdBudget == idBudget);

            if (sbp.SBPType == SBPType.Manager)
            {
                var parent = sbp.Parent;

                if (parent != null)
                {
                    sbpBlank = parent.SBP_Blank.FirstOrDefault(b => b.BlankType == BlankType.BringingRBS && b.IdBudget == idBudget);
                }
            }

            return sbpBlank.GetBlankCostProperties();
        }

        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_SbpblankByDocument(int idDocument)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LimitBudgetAllocations.FirstOrDefault(l => l.Id == idDocument);
            if (doc == null)
                return null;

            var sbpBlank = context.SBP_BlankHistory.FirstOrDefault(r => r.Id == doc.IdSBP_BlankActual);
            if (sbpBlank == null)
            {
                return null;
            }

            return sbpBlank.GetBlankCostProperties();
        }

        /// <summary>
        /// Получить по бланку СБП для текущего документа список скрываемых полей в гриде и форме сметной строки
        /// </summary>
        public Dictionary<string, IEnumerable<string>> get_HideColumn_tpControlRelation(int idDocument)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LimitBudgetAllocations.FirstOrDefault(l => l.Id == idDocument);
            if (doc == null)
                return null;
            if (doc.SBP.SBPType == SBPType.GeneralManager)
            {
                return new Dictionary<string, IEnumerable<string>>(){
                    {"hidden", new List<string>(){"withcompanyallocations","diffallocations"}},
                    {"required", new List<string>()}
                };
            }
            else
            {
                    return new Dictionary<string, IEnumerable<string>>(){
                    {"hidden", new List<string>()},
                    {"required", new List<string>(){"withcompanyallocations","diffallocations"}}
                };
                
            }

        }

        

        /// <summary>
        /// Проверяет есть ли разрешение для СБП на ввод дополнительной потребности и выставлен ли флажек в документе 
        /// </summary>
        public bool getPermissionsInputAdditionalRequirementsForLBA(int? IdDoc)
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.LimitBudgetAllocations.Where(r => r.Id == IdDoc).FirstOrDefault();

            // находим вышестоящий ГРБС
            var SBP = context.SBP.FirstOrDefault(s => s.Id == doc.IdSBP);
            var curSBP = SBP;
            while (curSBP != null)
            {

                curSBP = curSBP.IdParent.HasValue ? context.SBP.SingleOrDefault(w => w.Id == curSBP.IdParent) : null;
                if (curSBP != null) SBP = curSBP;
            }

            // находим для него разрешения

            var Permissions = context.PermissionsInputAdditionalRequirements.Any(w => w.IdSBP == SBP.Id && w.EnterAdditionalRequirements);

            return Permissions && (doc.IsAdditionalNeed ?? false);

        }

    }
}
