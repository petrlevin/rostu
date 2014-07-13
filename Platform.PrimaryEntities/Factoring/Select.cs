using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring
{
    /// <summary>
    /// Информация необходимая для формрования выборки данных
    /// </summary>
    public class Select : SelectBase, ISelect<DataRow>
    

{
    private SqlCommand _selectCommand;

    protected virtual string BuildSelect()
    {
        return string.Format("SELECT {0} FROM [ref].[{1}] WHERE [{2}]=@{2}", GetColumns(MetadataName), MetadataName,
                             ParameterName);
    }

    protected virtual string GetColumns(string metadataName)
    {
        return "*";
    }

    public SqlConnection DbConnection { get; set; }


    public SqlCommand SqlCommand
    {
        get
        {
            if (_selectCommand == null)
            {
                BuildCommand();
            }
            return _selectCommand;
        }
    }

    private void BuildCommand()
    {
        if (MetadataName == null)
            throw new InvalidOperationException("Не определено имя таблицы");
        if (ParameterName == null)
            throw new InvalidOperationException("Не определено имя параметра");
        if (Parameter == null)
            throw new InvalidOperationException("Не определен параметр");
        if (DbConnection == null)
            throw new InvalidOperationException("Не определен соединение с базой данных");

        _selectCommand = DbConnection.CreateCommand();
        _selectCommand.CommandText = BuildSelect();
        _selectCommand.Parameters.AddWithValue("@" + ParameterName, Parameter);

    }

        public virtual IEnumerable<DataRow> Execute()
        {
            var dt = SqlHelper.GetTable(this.SqlCommand);
            var result = new DataRow[dt.Rows.Count];
            dt.Rows.CopyTo(result,0);
            return result;
        }
}


}
