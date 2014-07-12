using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Reference;
using Platform.Common;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.Utils;
using Tests.Tablepart;

namespace Tests.Reference
{
    public partial class testDtp : IColumnFactoryForDenormalizedTablepart
    {
        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == "testDtp_Parent")
                return new ColumnsInfo()
                    {
                        Periods = GetColumnsFor_testDtp_Parent(),
                        Resources = GetResourcesFor_testDtp_Parent()
                    };
            return null;
        }

        private IEnumerable<PeriodIdCaption> GetColumnsFor_testDtp_Parent()
        {
            DataContext db = IoC.Resolve<DbContext>().Cast<DataContext>();

            var periods = db.HierarchyPeriod.Where(a => 
                a.DateStart.Year >= 2013 
                && a.DateStart.Year <= 2015 
                && a.DateStart.Month == 1 
                && a.DateEnd.Month == 12);

            foreach (HierarchyPeriod period in periods)
            {
                yield return new PeriodIdCaption() {PeriodId = period.Id, Caption = period.Caption };
            }
        }

        private IEnumerable<string> GetResourcesFor_testDtp_Parent()
        {
            var result = new List<string>();
            if (V1) result.Add(Reflection<testDtp_Child>.Property(ent => ent.Value1).Name);
            if (V2) result.Add(Reflection<testDtp_Child>.Property(ent => ent.Value2).Name);
            if (V3) result.Add(Reflection<testDtp_Child>.Property(ent => ent.Value3).Name);
            return result;
        }
    }
}
