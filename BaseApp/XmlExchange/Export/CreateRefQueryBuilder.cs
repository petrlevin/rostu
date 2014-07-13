using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.XmlExchange.Export
{
    public class CreateRefQueryBuilder
    {
        public CreateTableStatement BuildCreate(SchemaObjectName destination)
        {
            var result = new CreateTableStatement();
            result.ColumnDefinitions.Add(new ColumnDefinition()
            {
                ColumnIdentifier = "id".ToIdentifier(),
                IsIdentity = true,
                DataType = SqlDataTypeOption.Int.ToSqlDataType()
            });

            result.ColumnDefinitions.Add(new ColumnDefinition()
            {
                ColumnIdentifier = "idEntity".ToIdentifier(),
                DataType = SqlDataTypeOption.Int.ToSqlDataType()
            });
            result.ColumnDefinitions.Add(new ColumnDefinition()
            {
                ColumnIdentifier = "idElement".ToIdentifier(),
                DataType  = SqlDataTypeOption.Int.ToSqlDataType()
            });

            result.ColumnDefinitions.Add(new ColumnDefinition()
            {
                ColumnIdentifier = "originalIdElement".ToIdentifier(),
                DataType = SqlDataTypeOption.Int.ToSqlDataType()
            });

            result.ColumnDefinitions.Add(new ColumnDefinition()
            {
                ColumnIdentifier = "force".ToIdentifier(),
                DataType = SqlDataTypeOption.Bit.ToSqlDataType() ,

            });

            
            result.SchemaObjectName = destination;
            return result;


        }


    }
}
