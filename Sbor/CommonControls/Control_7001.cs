using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Interfaces;

namespace Sbor.CommonControls
{
    /// <summary>
    /// Проверка заполнения обязательных полей при прекращении документа
    /// </summary>
    [ControlInitial(ExcludeFromSetup = false, InitialSkippable = false, InitialManaged = false, InitialUNK = "7001", InitialCaption = "Проверка заполнения обязательных полей при прекращении документа")]
    public class Control_7001: IFreeCommonControl<IDocStatusTerminate, DataContext>
    {
        public void Execute(DataContext dataContext, IDocStatusTerminate doc)
        {
            List<string> list = new List<string>();
            if (!doc.DateTerminate.HasValue) list.Add("'Дата прекращения'");
            if (string.IsNullOrEmpty(doc.ReasonTerminate)) list.Add("'Причина прекращения'");
            if (list.Any())
            {
                string sMsg = list.Count() > 1 ? "Не заполнены обязательные поля " : "Не заполнено поле ";
                foreach (string l in list)
                {
                    sMsg += "<br> - " + l;
                }
                Controls.Throw(sMsg);
            }
        }
    }
}
