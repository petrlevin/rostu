using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using BaseApp.Reference;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic.NavigationPanel;
using Platform.Common;
using BaseApp.Reference;

namespace Sbor.NavigationPanel
{
    public class StateProgramCaption : INavigationPanelItemCaptionSelector
    {
        /// <summary>
        /// Муниципальная программа
        /// </summary>
        private const int municipalProgram = -1811939294;

        public int? GetItemName()
        {
            IPublicLegalFormation ppo = IoC.Resolve<SysDimensionsState>("CurentDimensions").PublicLegalFormation;
            if (ppo.IdBudgetLevel != BudgetLevel.SubjectRF && ppo.IdBudgetLevel != BudgetLevel.Federal)
            {
                return municipalProgram; 
            }
            return null;
        }
    }
}
