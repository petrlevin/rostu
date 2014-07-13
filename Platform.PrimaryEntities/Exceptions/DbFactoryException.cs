using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Exceptions
{
    public class DbFactoryException :Exception
    {
        private Object _dbBaseFactory;

        public DbFactoryException(string message, Exception innerException, Object dbBaseFactory)
            : base(message, innerException)
        {
            _dbBaseFactory = dbBaseFactory;
        }

        public DbFactoryException(string message, Exception innerException, Object dbBaseFactory, params object[] massageParams)
            : base(String.Format(message, massageParams), innerException)
        {
            _dbBaseFactory = dbBaseFactory;
        }


        public DbFactoryException(string message, Object dbBaseFactory)
            : base(message)
        {
            _dbBaseFactory = dbBaseFactory;
        }

        public DbFactoryException(Object dbBaseFactory)
        {
            _dbBaseFactory = dbBaseFactory;
        }

        public Object Factory
        {
            get { return _dbBaseFactory; }
        }
    }
}
