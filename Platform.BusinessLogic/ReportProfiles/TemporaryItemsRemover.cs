using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;

namespace Platform.BusinessLogic.ReportProfiles
{
    public class TemporaryItemsRemover
    {
        /// <summary>
        /// Удалить все временные экземпляры всех сущностей типа "Отчет", созданные пользователем <paramref name="userId"/>
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <returns>значение - количество удаленных элементов</returns>
        public static int RemoveAllTemporaryProfiles(int userId)
        {
            int result = 0;
            using (var db = new DataContext()) // через IoC на этапе Session_End контекст недоступен
            {
                var instance = new TemporaryItemsRemover();
                instance.db = db;
                result = instance.removeAllTemporaryProfiles(userId);
            }
            return result;
        }

        private DataContext db;

        private int removeAllTemporaryProfiles(int userId)
        {
            string sql = getDeleteStatements(userId).Aggregate((a, b) => string.Format("{0}{1}{2}", a, Environment.NewLine, b));
            return db.Database.ExecuteSqlCommand(sql);
        }

        private List<string> getDeleteStatements(int userId)
        {
            List<string> result = new List<string>();
            List<string> reportEntities = getReportEntities();
            foreach (string name in reportEntities)
            {
                result.Add(getDeleteStatement(name, userId));
            }
            return result;
        }

        private List<string> getReportEntities()
        {
            return db.Database.SqlQuery<string>(string.Format("SELECT Name FROM ref.Entity WHERE idEntityType = {0}", (int) EntityType.Report)).ToList();
        }

        private string getDeleteStatement(string tableName, int userId)
        {
            return string.Format("DELETE FROM rep.{0} WHERE isTemporary = 1 and idReportProfileUser = {1}", tableName, userId);
        }
    }
}
