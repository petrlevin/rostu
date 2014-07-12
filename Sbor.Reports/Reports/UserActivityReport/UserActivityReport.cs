using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Auditing.EfModel;
using Platform.Common;
using Platform.BusinessLogic.ReportingServices.Reports;
using Sbor.Reports.UserActivityReport;

namespace Sbor.Reports.Report
{
    [Report]
    public partial class UserActivityReport
    {
        public string UserCaption
        {
            get { return User.Caption; }
        }

        public List<UserAction> GetResult()
        {
            List<UserAction> result;
            using (var dc = new AuditDataContext())
            {
                var cmd = new StringBuilder("Select * from [dbo].[FullReportView] Where 1 = 1 ");

                if (IdUser.HasValue)
                    cmd.Append(" And [idUser] = " + IdUser.Value );
                
                if (DateFrom.HasValue)
                    cmd.Append(" And [Date] >= '" + DateFrom.Value.Date.ToString("s") + "'");

                if (DateTo.HasValue)
                    cmd.Append(" And [Date] < '" + DateTo.Value.Date.AddDays(1).ToString("s") + "'");

                if (IdEntity.HasValue)
                    cmd.Append(" And [EntityId] = " + IdEntity.Value );
                
                if (IdElement.HasValue)
                    cmd.Append(" And [ElementId] = " + IdElement.Value);

                if (IdAuditEntityEntity.HasValue)
                    cmd.Append(" And [EntityId] = " + IdAuditEntityEntity.Value);

                if (IdAuditEntity.HasValue)
                    cmd.Append(" And [ElementId] = " + IdAuditEntity.Value);

                result = dc.Database.SqlQuery<UserAction>( cmd.ToString() ).ToList();
            }

            return result;
        }

        public List<CommonInfo> GetCommonPart()
        {
            DataContext context = IoC.Resolve<DbContext>().Cast<DataContext>();
            User = context.User.FirstOrDefault(u => u.Id == IdUser);

            string elementCaption = null;

            if (IdEntity.HasValue && IdElement.HasValue)
                elementCaption = "Элемент: " + context.Database.SqlQuery<string>("Select [dbo].[GetCaption](" + IdEntity.Value + ", " + IdElement.Value + ")").FirstOrDefault();
            else if (IdEntity.HasValue)
                elementCaption = "Элементы типа: " + context.Entity.Where(e => e.Id == IdEntity.Value).Select(e => e.Caption).FirstOrDefault();

            return new List<CommonInfo>
                {
                    new CommonInfo
                        {
                            UserCaption = IdUser.HasValue ? "Пользователь: " + UserCaption : "",
                            ReportElementCaption = elementCaption ?? "",
                            DateFrom = DateFrom.HasValue ? "Начало периода: " + DateFrom.Value.ToString("d", new CultureInfo("ru-RU")) : "",
                            DateTo = DateTo.HasValue  ? "Конец периода: " + DateTo.Value.ToString("d", new CultureInfo("ru-RU")) : ""
                        }
                };
        }
    }
}
