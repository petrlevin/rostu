using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.ReportingServices;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Sbor.Reference;
using Sbor.Tablepart;

namespace Sbor.Reports.PrintForms.DocumentsOfSED
{
    /// <summary>
    /// Для просмотра: http://localhost/platform3/Services/PrintForm.aspx?entityName=DocumentsOfSED&printFormClassName=PrimaryPf&docId=-1811939300
    /// </summary>
    [PrintForm(Caption = "Печатная форма")]
    public class PrimaryPf : PrintFormBase
    {
        public PrimaryPf(int docId) : base(docId) { }

        public List<DataSetDoc> DataSetDoc()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.DocumentsOfSED.Single(s => s.Id == DocId);
            var ppo = context.PublicLegalFormation.Single(s => s.Id == doc.IdPublicLegalFormation);

            string header = (
                doc.IdDocType == DocType.StrategySED 
                ? "СТРАТЕГИЯ СОЦИАЛЬНО-ЭКОНОМИЧЕСКОГО РАЗВИТИЯ"
                : "ПРОГРАММА СОЦИАЛЬНО-ЭКОНОМИЧЕСКОГО РАЗВИТИЯ" 
            );
            
            return new List<DataSetDoc>() { 
                new DataSetDoc()
                {
                    Id = DocId, 
                    Caption = doc.Caption, 
                    Date  = doc.Date, 
                    CurrentDate = DateTime.Now,
                    IdPublicLegalFormation = ppo.Id,
                    CapPublicLegalFormation = ppo.Caption,
                    Header = header
                } 
            };
        }

        private void numHier(List<DataSetMain> list, int? IdParent = null, string prefix = "")
        {
            var data = list.Where(w => w.IdParent == IdParent);
            if (!data.Any()) return;

            var gdata =
                data.GroupBy(s => new {s.Id, s.IdParent, s.SortKeyHierarhy})
                    .Distinct()
                    .OrderBy(o => o.Key.SortKeyHierarhy);

            int cnt = 1;
            foreach (var g in gdata)
            {
                var NN = prefix + cnt.ToString(CultureInfo.InvariantCulture) + ".";
                foreach (var d in g)
                {
                    d.NN = NN;
                }
                numHier(list, g.Key.Id, NN);
                cnt++;
            }
        }

        public List<DataSetMain> DataSetMain()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();

            var doc = context.DocumentsOfSED.Single(s => s.Id == DocId);

            List<DataSetMain> res = (
                    from sg in context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == DocId)
                    join b  in context.DocumentsOfSED_GoalIndicator.Where(w => w.IdOwner == DocId) on sg.Id equals b.IdMaster into tmpb
                    from gi in tmpb.DefaultIfEmpty()
                    join c in context.DocumentsOfSED_GoalIndicatorValue.Where(w => w.IdOwner == DocId) on gi.Id equals c.IdMaster into tmpc
                    from giv in tmpc.DefaultIfEmpty()
                    select new DataSetMain()
                    {
                        Id                = sg.Id,
                        IdParent          = sg.IdParent,
                        IdTpGoalIndicator = gi == null ? (int?)null : gi.Id,
                        CapSystemGoal            = sg.SystemGoal.Caption,
                        CapElementTypeSystemGoal = sg.ElementTypeSystemGoal.Caption,
                        CapGoalIndicator         = gi == null ? null : gi.GoalIndicator.Caption,
                        CapUnitDimension         = gi == null ? null : gi.GoalIndicator.UnitDimension.Symbol,
                        DateStart = sg.DateStart,
                        DateEnd   = sg.DateEnd,
                        Year  = giv == null ? doc.DateStart.Year : giv.HierarchyPeriod.DateStart.Year,
                        Value = giv == null ? (decimal?)null : giv.Value,
                        SortKeyHierarhy = sg.SystemGoal.Caption,
                        NN = ""
                    }
            ).ToList();

            if (res.Any())
            {
                numHier(res);

                var existsYears = res.Select(s => s.Year).Distinct().ToArray();
                var firstRes = res.First();
                for (int y = doc.DateStart.Year; y <= doc.DateEnd.Year; y++)
                {
                    if (!existsYears.Contains(y))
                    {
                        res.Add(new DataSetMain(firstRes, y, null));
                    }
                }
            }

            //http://msdn.microsoft.com/ru-ru/library/bb311040%28v=vs.90%29.aspx
            //http://stackoverflow.com/questions/525194/linq-inner-join-vs-left-join
            // требуется проверить левое соедиение в ситуации 1:m
            //var qq = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == DocId).GroupJoin(
            //    context.DocumentsOfSED_GoalIndicator.Where(w => w.IdOwner == DocId).DefaultIfEmpty(),
            //    a => a.Id, b => b.IdMaster,
            //    (a,b) => new { a, b = b.DefaultIfEmpty() }
            //);

            //int cnt = 0;
            //var all = context.DocumentsOfSED_ItemsSystemGoal.Where(w => w.IdOwner == DocId);
            //var child = all.Where(w => !w.IdParent.HasValue).ToList();
            //foreach (var s in child.OrderBy(o => o.SystemGoal.Caption))
            //{
                
            //}

            return res;
        }

    }
}
