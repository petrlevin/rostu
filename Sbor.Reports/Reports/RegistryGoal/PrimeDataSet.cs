using System;

namespace Sbor.Reports.RegistryGoal
{
    public class PrimeDataSet
    {
        public int? IdSubjectBP { get; set; }
        public int? Id { get; set; }
        public int? IdParent { get; set; }
        public string TypeGoal { get; set; }//
        public string PPO { get; set; }//
        public string CaptionGoal { get; set; }//
        public string SubjectBP { get; set; }//
        public DateTime DateStart { get; set; }//«Срок реализации с год»
        public DateTime DateEnd { get; set; }//«Срок реализации по год»
        public string CapGoalIndicator { get; set; }//
        public string CapUnitDimension { get; set; }//Единица измерения
        public int? Year { get; set; }//Год фиксации показателя
        public decimal? GIValue { get; set; }//Значение показателя
        public string SortKeyHierarhy { get; set; }
        public string NN { get; set; }
        public int? code { get; set; }
        public string GoalCode { get; set; }//код цели строка
        public int? GoalCodeInt { get; set; }//код цели число
        public int? IdElementTypeSystemGoal { get; set; } //тип елемента
        public string CaptionSBP { get; set; } //Имя СБП
        public string numb { get; set; }
        public decimal? VolumeOfExpenses { get; set; } //Объем расходов на реализацию за ОФГ
        public decimal? VolumeOfExpenses1 { get; set; } //Объем расходов на реализацию за ПФГ-1
        public decimal? VolumeOfExpenses2 { get; set; } //Объем расходов на реализацию за ПФГ-2
        public decimal? AmountOfCash { get; set; } //
        public int? IdSystemGoal { get; set; }//айди цели ил справочника
    }
}
