using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Exceptions
{
    public class ObjectException : DbFactoryException
    {
        private Type _objectType;

        public ObjectException(string message, Exception innerException, Type objectType, object dbFactoryBase)
            : base(message, innerException, dbFactoryBase)
        {
            _objectType = objectType;
            
            
            
        }



        public ObjectException(string message, Type objectType, object dbFactoryBase)
            : this(message, null, objectType, dbFactoryBase)
        {
            _objectType = objectType;
        }

        public ObjectException(Type creationObjectType,object dbFactoryBase)
            : this(null, creationObjectType, dbFactoryBase)
        {
            _objectType = creationObjectType;
        }

        public Type ObjectType
        {
            get { return _objectType; }
        }



    }
}
