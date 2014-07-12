using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sbor.Reports.Report
{
    partial class BudgetExpenseStructure
    {
        private string BuildOuterSelectPart()
        {
            var sb = new StringBuilder();

            var selectQuery = new StringBuilder();
            var fromQuery = new StringBuilder();
            var whereQuery = new StringBuilder();
            var orderQuery = new StringBuilder();

            //Мероприятие
            selectQuery.Append(_orderedColumns.Any(c => c.Entity.Name == "Activity")
                                   ? " [Activity].[Id] "
                                   : " Null ");
            selectQuery.Append(" as [IdActivity], ");

            //Программа
            selectQuery.Append(_orderedColumns.Any(c => c.Entity.Name == "Program")
                                   ? " [Program].[Id] "
                                   : " Null ");
            selectQuery.Append(" as [IdProgram], ");

            //Цель
            selectQuery.Append(_orderedColumns.Any(c => c.Entity.Name == "SystemGoalElement")
                                   ? " [SystemGoalElement].[Id] "
                                   : " Null ");
            selectQuery.Append(" as [IdSystemGoalElement], ");


            #region жЫрность по груповым колонкам KCSR и RZPR
                if (_orderedColumns.Any(c => c.Entity.Name.ToUpper() == "KCSR") ||
                    _orderedColumns.Any(c => c.Entity.Name.ToUpper() == "RZPR"))
                {
                    selectQuery.Append(@" Case When ");

                    if (_orderedColumns.Any(c => c.Entity.Name.ToUpper() == "KCSR"))
                        selectQuery.Append(" ( Exists (Select Null From [ref].[KCSR] [KCSRChild] Where [KCSRChild].[IdParent] = [KCSR].[id] ))");

                    if (_orderedColumns.Any(c => c.Entity.Name.ToUpper() == "KCSR") && _orderedColumns.Any(c => c.Entity.Name.ToUpper() == "RZPR"))
                        selectQuery.Append(" Or ");

                    if (_orderedColumns.Any(c => c.Entity.Name.ToUpper() == "RZPR"))
                        selectQuery.Append("( Exists (Select Null From [ref].[RZPR] [RZPRChild] Where [RZPRChild].[IdParent] = [RZPR].[id] ))");

                    selectQuery.Append(" Then 0 Else 1 End as [isBold], ");
                }
                else
                    selectQuery.Append(" 1 as [isBold], ");
            #endregion

            //Строка 'Итого'. Поле IsResult -- для того, чтобы опустить вниз итоги.
            selectQuery.AppendFormat(@"{1} Case When ( {0} ) 
                                                Then 0
                                                Else 1						
							                End as IsResult, ", String.Join(" OR ", _orderedColumns.ToList().Select(c => String.Format(" Result.Id{0} Is Not Null ", c.Entity.Name))), Environment.NewLine);

            //Тип цели
            if (ShowGoals ?? false)
            {
                selectQuery.AppendFormat(@"Case
    When
       (Result.IdSystemGoalElement Is Not Null {2} And {0} {1} )
    Then
        [ElementTypeSystemGoal].Caption + ' ' + [SystemGoalElementTree].number
    Else
        Null
    End as SystemGoalType,", string.Join(" And ", _allColumns.Select(c => " Result.Id" + c.Name + " Is Null ").ToList()),
                                         (ShowActivities ?? false) ? "And Result.IdActivity Is Null " : String.Empty,
                                         (ShowProgram ?? false) ? "And Result.IdProgram Is Null" : String.Empty);

                selectQuery.AppendFormat(@"
 Case 
    When 
        {0}
        ( [ElementTypeSystemGoal].Caption = 'Стратегическая цель' Or  [ElementTypeSystemGoal].Caption = 'Стратегическая задача' Or  [ElementTypeSystemGoal].Caption = 'Тактическая цель' )
    Then 
        1
    Else
        0
End as IsBoldSystemGoal, ", ShowProgram ?? false ? " Result.IdProgram Is Null And  " : string.Empty);
            }
            else
            {
                selectQuery.Append(@"
         0 as IsBoldSystemGoal, ");
            
            }
            // Уроверь программы
            if (ShowProgram ?? false) 
                selectQuery.Append(@"Case when Result.isProgram = 1 and [DocType].id = -1543503843 then 1
                                        when Result.isProgram = 1 and [DocType].id = -1543503842 then 2
                                        when Result.isProgram = 1 and ([DocType].id = -1543503841 or [DocType].id = -1543503840 or [DocType].id = -1543503839) then 3
                                        end as LevelProgram, ");
            


            #region Поле наименование
            selectQuery.Append(" Case ");
            foreach (var column in _orderedColumns.Reverse().ToList())
            {
                if (column.Entity.Name == "SystemGoalElement")
                    selectQuery.AppendFormat(@"When 
								Result.Id{0} Is Not Null 
							Then 
								[SystemGoal].Caption {1}", column.Entity.Name, Environment.NewLine);
                else if (column.Entity.Name == "Activity")
                    selectQuery.AppendFormat(@"When 
								Result.Id{0} Is Not Null 
							Then 
								'Мероприятие «' + {0}.FullCaption + '»' {1}", column.Entity.Name, Environment.NewLine);
                else if (column.Entity.Name == "Program")
                    selectQuery.AppendFormat(@"When 
								Result.Id{0} Is Not Null 
							Then 
								[ProgramCaptionKCSR].Caption {1}", column.Entity.Name, Environment.NewLine);
                else
                    selectQuery.AppendFormat(@"When 
								Result.Id{0} Is Not Null 
							Then 
								{0}.{1} {2}", column.Entity.Name, column.DescriptionField, Environment.NewLine);
            }
            selectQuery.Append(@"Else
						'ИТОГО:'
							End as Caption," + Environment.NewLine);
            #endregion

            #region Вывод КБК в столбцы S1 - S13 в правильном порядке. "Пустые" колонки должны быть заполнены нулами.

            int i = 1;
            foreach (var column in _allColumns)
            {
                //Если выводим программы -- в поле KCSR выводим аналитический код
                if ((ShowProgram ?? false) && column.Name == "KCSR")
                {
                    selectQuery.AppendFormat(@" Case
    When
       (Result.IdProgram Is Not Null And {4} {5})
    Then
        [ProgramCaptionKCSR].Code 
    Else
        {0}.{1} 
    End as S{2}, {3}", column.Name, column.CaptionField.Name, i++,
                                    Environment.NewLine, string.Join(" And ", _allColumns.Select(c => " Result.Id" + c.Name + " Is Null ").ToList()),
                                    (ShowActivities ?? false) ? "And Result.IdActivity Is Null " : String.Empty);
                }
                else
                    selectQuery.AppendFormat(" {0}.{1} as S{2}, {3}", column.Name, column.CaptionField.Name, i++,
                                    Environment.NewLine);
            }

            for (int c = i; c <= 13; c++)
                selectQuery.AppendFormat(" NULL as S{0}, {1}", c, Environment.NewLine);

            #endregion
            
            //Суммы
            selectQuery.Append(
                "Result.ValueCaption + Cast( Year(HP.DateStart) as nvarchar ) as [Year], Year(HP.DateStart) as Sort, " +
                Environment.NewLine + GetValueString());

            #region Соединения для получения наименований
            fromQuery.Append(@" (" + BuildUnionParts() + @") Result 
						  Inner Join ref.HierarchyPeriod HP on Result.idHierarchyPeriod = HP.Id ");


            foreach (var column in _orderedColumns)
                fromQuery.AppendFormat("{2}Left Join [{0}].[{1}] [{1}] on Result.Id{1} = {1}.Id ", column.Entity.Schema,
                                                                                        column.Entity.Name,
                                                                                        Environment.NewLine);


            if (_orderedColumns.Any(c => c.Entity.Name == "Program"))
                fromQuery.Append(Environment.NewLine + @"Left Join [ProgramTree] [ProgramTree] on [ProgramTree].Id = Program.Id 
                                Left Join [reg].[AttributeOfProgram] [AttributeOfProgram] on ([AttributeOfProgram].idProgram = [Program].[Id] And [AttributeOfProgram].[idTerminator] is null) 
                                Left Join [ref].[DocType] [DocType] on [DocType].id = [Program].idDocType
                                Left Join [ref].[AnalyticalCodeStateProgram] [AnalyticalCodeStateProgram] on [AnalyticalCodeStateProgram].[id] = [AttributeOfProgram].[idAnalyticalCodeStateProgram]
                                Left Join [ref].[KCSR] [ProgramCaptionKCSR] on REPLACE(AnalyticalCodeStateProgram.AnalyticalCode, '.', '') = REPLACE(ProgramCaptionKCSR.Code, '.', '')");

            if (_orderedColumns.Any(c => c.Entity.Name.ToUpper() == "KCSR" && c.IsGrouped))
                fromQuery.Append(Environment.NewLine + @"Left Join [KCSRTree] [KCSRTree] on [KCSRTree].Id = Result.IdKCSR");


            if (_orderedColumns.Any(c => c.Entity.Name == "SystemGoalElement"))
                fromQuery.Append(Environment.NewLine + @"
                    Left Join [SystemGoalElementTree] [SystemGoalElementTree] on [SystemGoalElementTree].id = [SystemGoalElement].Id
                    Left Join [ref].[SystemGoal] [SystemGoal] on [SystemGoal].id = [SystemGoalElement].idSystemGoal
                    Left Join [reg].[AttributeOfSystemGoalElement] [AttributeOfSystemGoalElement] on ( [AttributeOfSystemGoalElement].[idSystemGoalElement] = [SystemGoalElement].Id And [AttributeOfSystemGoalElement].[idTerminator] Is Null)
					Left Join [ref].[ElementTypeSystemGoal] [ElementTypeSystemGoal] on [ElementTypeSystemGoal].id = [AttributeOfSystemGoalElement].[idElementTypeSystemGoal]");
            #endregion

            #region Уровни программ
            if (ShowProgram ?? false)
            {
                whereQuery.AppendFormat(@"{3} ( {0} {1} Or [Result].[idProgram] Is Null Or [Program].[idDocType] = {2}",
                        string.Join(" Or ", _allColumns.Select(c => " Result.Id" + c.Name + " Is Not Null ")),
                        (ShowActivities ?? false) ? " Or Result.IdActivity Is Not Null " : String.Empty, Sbor.Reference.DocType.StateProgram, Environment.NewLine);

                if (!IdDocType.HasValue || (IdDocType.Value != Sbor.Reference.DocType.StateProgram))
                {
                    whereQuery.AppendFormat(" Or [Program].[idDocType] = {0} ", Sbor.Reference.DocType.SubProgramSP);

                    if (!IdDocType.HasValue || (IdDocType.Value == Sbor.Reference.DocType.ProgramOfSBP))
                        whereQuery.AppendFormat(" Or [Program].[idDocType] = {0} Or [Program].[idDocType] = {1} ", Sbor.Reference.DocType.ProgramOfSBP, Sbor.Reference.DocType.MainActivity);
                }

                whereQuery.Append(")");


                //скрываем сметные строки у ВЦП
                whereQuery.AppendFormat(@"{2} And ( {0} Or [Program].[idDocType] = {1} )",
                        string.Join(" And ", _allColumns.Select(c => " Result.Id" + c.Name + " Is Null ")), Sbor.Reference.DocType.NonProgramActivity, Environment.NewLine);
            }
            #endregion
            
            if (_orderedColumns.Any())
            {
                orderQuery.Append(Environment.NewLine + " IsResult,");

                var orderColumnNames = new List<String>();

                foreach (var c in _orderedColumns)
                {
                    if (c.Entity.Name == "Program")
                        orderColumnNames.Add("ProgramTree.Position");
                    else if (c.Entity.Name == "SystemGoalElement")
                        orderColumnNames.Add("SystemGoalElementTree.Position");
                    else if (c.Entity.Name.ToUpper() == "KCSR" && c.IsGrouped)
                        orderColumnNames.Add("KCSRTree.position");
                    else
                        orderColumnNames.Add(c.Entity.Name + "." + c.Entity.CaptionField.Name);
                }

                orderQuery.AppendFormat(String.Join("," + Environment.NewLine, orderColumnNames));
            }

            sb.Append("Select " 
                        + Environment.NewLine + selectQuery 
                        + Environment.NewLine + "From" + fromQuery );

            if (whereQuery.Length > 0)
                sb.Append(Environment.NewLine + "Where"
                          + Environment.NewLine + whereQuery);

            if (orderQuery.Length > 0)
                sb.Append(Environment.NewLine + "Order By"
                          + Environment.NewLine + orderQuery);

            return sb.ToString();
        }
    }
}
