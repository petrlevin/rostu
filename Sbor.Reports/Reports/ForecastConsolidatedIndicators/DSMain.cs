using System;

namespace Sbor.Reports.ForecastConsolidatedIndicators
{
    public class DSMain:ICloneable
    {
        // 1
        public int? TopGroupBy { get; set; } // id
        public string TopNumber { get; set; } // Порядковый номер
        public string TopName { get; set; } // Подпрограммы
        public string TopAnalyticalCode { get; set; }

        //2
        //public int MiddleId { get; set; } // id
        public int? MiddleGroupBy { get; set; }
        public string MiddleNumber { get; set; } // Порядковый номер
        public bool MiddleHide { get; set; } // скрытие, если нет дочерних
        public string MiddleName { get; set; } // Наименование под-подпрограммы
        public string MiddleAnalyticalCode { get; set; }

        // 3
        //public int ActivityId { get; set; } // id
        public int? ActivityGroupByPart1 { get; set; } //Id мероприятия в sn7
        public int? ActivityGroupByPart2 { get; set; } //Id показателя
        public string ActivityNumber { get; set; } // Порядковый номер
        public string ActivityName { get; set; } // Наименование мероприятия  ...
        public string InditorActivity { get; set; } // Наименование показателя объема услуги

        public bool VolumeOrExpense { get; set; }

        public int? Year { get; set; }

        public bool IsAdditionalNeed { get; set; }

        //public int AttributeOfProgram_Id { get; set; }
        //public int AttributeOfProgram_IdSBP { get; set; }
        //public string AttributeOfProgram_SBPCaption { get; set; }
        public decimal? ValueD { get; set; }
        public string Value { get; set; }
        public string ValueWhithAdditionalNeed { get; set; }

        public DSMain Clone(int? year, string value, string valueWhithAdditionalNeed, decimal? valueD)
        {
            DSMain clone = (DSMain)this.Clone();
            clone.Year = year;
            clone.Value = value;
            clone.ValueD = valueD;
            clone.ValueWhithAdditionalNeed = valueWhithAdditionalNeed;
            return clone;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }
}
