using Microsoft.Practices.Unity;

namespace Platform.PrimaryEntities.Factoring.Strategies
{
    public class Base<TSelect>: Default<TSelect> where TSelect : Select, new()
    {
        [Dependency("DbConnection")]
        public override System.Data.SqlClient.SqlConnection DbConnection
        {
            get
            {
                return base.DbConnection;
            }
            set
            {
                base.DbConnection = value;
            }
        }
    }

    public class Base :Base<Select>
    {}
}
