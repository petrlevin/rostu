using System;
using System.Data;
using System.Data.SqlClient;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Factoring.Strategies
{
    public  class Default<TData, TSelect> : BaseFactoryStrategy<TData>  where TSelect : SelectBase, ISelect<TData>, new()
    {
        public override ISelect<TData> CreateSelect(object parameter, string parameterName, string metadataName)
        {
            return new TSelect
                       {
                           Parameter = parameter,
                           ParameterName = parameterName,
                           MetadataName = metadataName
                       };

        }

        public override IFactory GetFactory()
        {
            return new Factory<TData>() { FactoryStrategy = this };
        }
    }

    public  class Default<TSelect> : Default<DataRow, TSelect>,IDbConnectioned, IFactoryStrategy<DataRow> where TSelect : Select, ISelect<DataRow>, new()
    {

    public virtual SqlConnection DbConnection { get; set; }
        public Default(SqlConnection dbConnection)
        {
            if (dbConnection == null) throw new ArgumentNullException("dbConnection");
            DbConnection = dbConnection;

        }

        protected Default()
        {

        }


        public override ISelect<DataRow> CreateSelect(object parameter, string parameterName, string metadataName)
        {
            Select result = base.CreateSelect(parameter, parameterName, metadataName) as Select;
            result.DbConnection = DbConnection;
            return result;
        }

    }

    public  class Default: Default<Select>
    {
        public Default(SqlConnection dbConnection)
            : base(dbConnection)
        {

        }

        protected Default()
        {
            
        }
    }
}
