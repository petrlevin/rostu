using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoClient.FixLimitVolumeAppropriations
{
    partial class Main: IDisposable
    {
        public Main(string connectionString)
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        #region Public Properties

        public string ConnectionString { get; private set; }

        [Obsolete("Use ResultDir")]
        public TextWriter Out { get; set; }

        public string ResultDir { get; set; }

        public List<string> TargetDocuments = new List<string>()
            {
                "LimitBudgetAllocations",
                "ActivityOfSBP",
                "PlanActivity",
                "PublicInstitutionEstimate",
                "FinancialAndBusinessActivities"
            };

        private Dictionary<string, int> _targetEntities;
        public Dictionary<string, int> TargetEntities
        {
            get
            {
                if (_targetEntities == null)
                    _targetEntities = _getEntityIds();
                return _targetEntities;
            }
        }

        #endregion

        #region Private properties

        private SqlConnection connection { get; set; }

        #endregion

        #region Public

        public void FindDrafts()
        {
            forEach(TargetDocuments, SqlTpl.FindDrafts, "FindDrafts");
        }

        public Dictionary<string, string> LeafStatuses()
        {
            return forEach(TargetDocuments, SqlTpl.LeafStatuses)
                .ToDictionary(kvp => kvp.Key, kvp => ToString(kvp.Value));
        }

        public Dictionary<string, List<int>> LeafDocuments()
        {
            return forEach(TargetDocuments, SqlTpl.LeafDocuments)
                .ToDictionary(kvp => kvp.Key, kvp => ToList<int>(kvp.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>ключ - id сущности, значение - список идентификаторов документов на статусе Черновик</returns>
        public Dictionary<string, List<int>> GetDrafts()
        {
            return forEach(TargetDocuments, SqlTpl.GetDrafts)
                .ToDictionary(kvp => kvp.Key, kvp => ToList<int>(kvp.Value));
        }

        public Dictionary<string, List<int>> GetDraftsInEditMode()
        {
            return forEach(TargetDocuments, SqlTpl.GetDraftsInEditMode)
                .ToDictionary(kvp => kvp.Key, kvp => ToList<int>(kvp.Value));
        }

        public Dictionary<string, int> GetUndoChangeOparations()
        {
            DataTable table = executeCommand(SqlTpl.GetUndoChangeOperations(TargetDocuments));
            return table.Rows.Cast<DataRow>().ToDictionary(row => row.Field<string>("Name"), row => row.Field<int>("id"));
        }

        public Dictionary<string, int> GetChangeOparations()
        {
            DataTable table = executeCommand(SqlTpl.GetChangeOperations(TargetDocuments));
            return table.Rows.Cast<DataRow>().ToDictionary(row => row.Field<string>("Name"), row => row.Field<int>("id"));
        }

        public Dictionary<string, int> GetProcessOparations()
        {
            DataTable table = executeCommand(SqlTpl.GetProcessOperations(TargetDocuments));
            return table.Rows.Cast<DataRow>().ToDictionary(row => row.Field<string>("Name"), row => row.Field<int>("id"));
        }

        public Dictionary<string, List<int>> GetDraftsParents()
        {
            return forEach(TargetDocuments, SqlTpl.GetDraftsParents)
                .ToDictionary(kvp => kvp.Key, kvp => ToList<int>(kvp.Value));
        }

        public void Dispose()
        {
            connection.Close();
        }

        #endregion

        #region Private Methods

        private Dictionary<string, int> _getEntityIds()
        {
            DataTable table = executeCommand(SqlTpl.GetEntityIdsByName(TargetDocuments));
            return table.Rows.Cast<DataRow>().ToDictionary(row => row.Field<string>("Name"), row => row.Field<int>("id"));
        }

        //ToDo: похож на другой метод forEach. Следует объединить в один или вообще удалить.
        private void forEach(IEnumerable<string> array, Func<string, string> sqlGetter, string subfolder)
        {
            Directory.CreateDirectory(string.Format(@"{0}\{1}", ResultDir, subfolder));
            TextWriter output;
            foreach (string item in array)
            {
                output = File.CreateText(string.Format(@"{0}\{1}\{2}.txt", ResultDir, subfolder, item));
                DataTable table = executeCommand(sqlGetter(item));
                Write(output, table);
                output.Close();
            }
        }

        private Dictionary<string, DataTable> forEach(IEnumerable<string> array, Func<string, string> sqlGetter)
        {
            var result = new Dictionary<string, DataTable>();
            foreach (string item in array)
            {
                result.Add(item, executeCommand(sqlGetter(item)));
            }
            return result;
        }

        private DataTable executeCommand(string sql)
        {
            var sqlCmd = new SqlCommand(sql, connection);
            var table = new DataTable();
            var adapter = new SqlDataAdapter(sqlCmd);
            adapter.Fill(table);
            return table;
        }

        private static void Write(TextWriter output, DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                string rowString = table.Columns.Cast<DataColumn>()
                     .Select(c => row[c.ColumnName].ToString())
                     .Aggregate((a, b) => string.Format("{0}\t{1}", a, b));

                output.WriteLine(rowString);
            }
        }

        private static string ToString(DataTable table)
        {
            var sb = new StringBuilder();
            foreach (DataRow row in table.Rows)
            {
                string rowString = table.Columns.Cast<DataColumn>()
                     .Select(c => row[c.ColumnName].ToString())
                     .Aggregate((a, b) => string.Format("{0}\t{1}", a, b));

                sb.AppendLine(rowString);
            }
            return sb.ToString();
        }

        private static List<T> ToList<T>(DataTable table, string fieldName = "id")
        {
            return table.Rows.Cast<DataRow>().Select(row => row.Field<T>(fieldName)).ToList();
        }

        #endregion
    }
}
