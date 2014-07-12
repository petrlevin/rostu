using Platform.BusinessLogic.Common.Attributes;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Activity.Controls;

namespace Sbor.Reference
{
    [SelectedWithNoChilds]
    public partial class AnalyticalCodeStateProgram : ReferenceEntity
    {
        #region Control_502201 заменён индексом
        ///// <summary>
        ///// Контроль "Проверка уникальности аналитического кода "
        ///// </summary>
        //[Control(ControlType.Update | ControlType.Insert, Sequence.Before , ExecutionOrder = 10)]
        //public void Control_502201(DataContext context)
        //{

        //    bool fail = context.AnalyticalCodeStateProgram.Any( a=>
        //                        a.IdPublicLegalFormation == IdPublicLegalFormation &&
        //                        a.AnalyticalCode  == AnalyticalCode &&
        //                        a.IdRefStatus != (byte) RefStatus.Archive  &&
        //                        a.Id != Id );
            
        //    if (fail)
        //    {
        //        Controls.Throw(string.Format(
        //            "В справочнике уже имеется элемент с кодом «{0}».", AnalyticalCode));
        //    }
        //}
        #endregion

        /// <summary>
        /// Контроль "Проверка подчинения типов аналитических кодов"
        /// </summary>
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 20)]
        public void Control_502202(DataContext context)
        {
            var err =
                //1.	Если поле «Тип» = «Государственная программа», то поле «Вышестоящий» должно быть пусто.
                (this.IdTypeOfAnalyticalCodeStateProgram == (int) DbEnums.TypeOfAnalyticalCodeStateProgram.StateProgram) && this.IdParent.HasValue ||
                //2.	Если поле «Тип» = «Подпрограмма ГП», то элемент в поле «Вышестоящий» должен иметь тип «Государственная программа». 
                (this.IdTypeOfAnalyticalCodeStateProgram == (int) DbEnums.TypeOfAnalyticalCodeStateProgram.StateSubprogram) && (!this.IdParent.HasValue || this.Parent.IdTypeOfAnalyticalCodeStateProgram != (int) DbEnums.TypeOfAnalyticalCodeStateProgram.StateProgram) ||
                //3.	Если поле «Тип» = «Основное мероприятие», то элемент в поле «Вышестоящий» должен иметь тип «Государственная программа» или «Подпрограмма ГП».
                (this.IdTypeOfAnalyticalCodeStateProgram == (int) DbEnums.TypeOfAnalyticalCodeStateProgram.MainAction) && (!this.IdParent.HasValue || this.Parent.IdTypeOfAnalyticalCodeStateProgram == (int) DbEnums.TypeOfAnalyticalCodeStateProgram.MainAction);

            if (err)
            {
                var msg = "Некорректный тип аналитического кода, или тип вышестоящего аналитического кода.<br>" +
                          "Допустимые значения (тип кода – тип вышестоящего кода): <br>" +
                          "«Государственная программа» - вышестоящий отсутствует;<br>" +
                          "«Подпрограмма ГП» - «Государственная программа»;<br>" +
                          "«Основное мероприятие» - «Государственная программа» или «Подпрограмма ГП».";

                Controls.Throw(msg);
            }
        }
    }
}
