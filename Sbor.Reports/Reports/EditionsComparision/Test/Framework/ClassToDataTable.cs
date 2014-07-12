using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;

namespace Sbor.Reports.EditionsComparision.Test
{
    public class ClassToDataTable<T>
    {
        private Type type;

        public static DataTable Get(IEnumerable<T> data)
        {
            var instance = new ClassToDataTable<T>();
            return instance.get(data);
        }

        private ClassToDataTable()
        {
        }

        private DataTable get(IEnumerable<T> data)
        {
            type = typeof (T);
            var table = CreateEmptyDataTable();
            fillTable(table, data);
            return table;
        }

        private DataTable CreateEmptyDataTable()
        {
            DataTable dt = new DataTable();

            foreach (PropertyInfo info in type.GetProperties())
            {
                dt.Columns.Add(new DataColumn(info.Name, info.PropertyType));
            }

            return dt;
        }

        private void fillTable(DataTable table, IEnumerable<T> data)
        {
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (string propName in type.GetProperties().Select(p => p.Name))
                {
                    row[propName] = item.GetValue(propName);
                }
                table.Rows.Add(row);
            }
        }
    }
}
