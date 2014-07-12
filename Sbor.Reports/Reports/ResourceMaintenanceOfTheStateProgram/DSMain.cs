using System;
using Sbor.Registry;

namespace Sbor.Reports.ResourceMaintenanceOfTheStateProgram
{
    public class DSMain: ICloneable
    {
        public int? ProgramId { get; set; }
        public int? ParentProgramId { get; set; }
        public int? ActivityId { get; set; }

        public string StrName { get; set; }
        public string SortKey { get; set; }

        public bool IsContractorGrp { get; set; }
        public int? ContractorOrd { get; set; }
        public string Contractor { get; set; }

        public bool IsSourceGrp { get; set; }
        public int? SourceOrd { get; set; }
        public string SourceName { get; set; }

        public int? Year { get; set; }
        public decimal? Value { get; set; }

        public DSMain Clone(int? year, int? value)
        {
            DSMain clone = (DSMain)this.Clone();
            clone.Year = year;
            clone.Value = value;
            return clone;
        }

        public DSMain Clone(AttributeOfProgram prg, string org, bool isClearActivityId = true)
        {
            DSMain clone = (DSMain)this.Clone();
            clone.ProgramId = prg.IdProgram;
            clone.ParentProgramId = prg.IdParent;
            clone.Contractor = org;
            if (isClearActivityId) clone.ActivityId = null;
            return clone;
        }

        public object Clone()
        {
            object clone = new DSMain
            {
                ProgramId = ProgramId,
                ParentProgramId = ParentProgramId,
                ActivityId = ActivityId,

                StrName = StrName,
                SortKey = SortKey,

                IsContractorGrp = IsContractorGrp,
                Contractor = Contractor,

                IsSourceGrp = IsSourceGrp,
                SourceName = SourceName,

                Year = Year,
                Value = Value
            };
            return clone;
        }
    }
}
