using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbor.Reports.PrintForms.DocumentsOfSED
{
    public class DataSetMain
    {
        public int Id { get; set; }
        public int? IdParent { get; set; }
        public string NN { get; set; }
        public string SortKeyHierarhy { get; set; }
        public string CapElementTypeSystemGoal { get; set; }
        public string CapSystemGoal { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? IdTpGoalIndicator { get; set; }
        public string CapGoalIndicator { get; set; }
        public string CapUnitDimension { get; set; }
        public int? Year { get; set; }
        public decimal? Value { get; set; }

        public DataSetMain()
        {
        }

        public DataSetMain(DataSetMain old, int? y, decimal? val)
        {
            Id = old.Id;
            IdParent = old.IdParent;
            NN = old.NN;
            SortKeyHierarhy = old.SortKeyHierarhy;
            CapElementTypeSystemGoal = old.CapElementTypeSystemGoal;
            CapSystemGoal = old.CapSystemGoal;
            DateStart = old.DateStart;
            DateEnd = old.DateEnd;
            IdTpGoalIndicator = old.IdTpGoalIndicator;
            CapGoalIndicator = old.CapGoalIndicator;
            CapUnitDimension = old.CapUnitDimension;
            Year = y;
            Value = val;
        }
    }
}
