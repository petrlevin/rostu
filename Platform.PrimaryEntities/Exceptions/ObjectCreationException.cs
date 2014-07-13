using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Exceptions
{
    public class ObjectCreationException : ObjectException
    {


        public ObjectCreationException(string message, Exception innerException, Type creationObjectType, object dbFactoryBase)
            : base(message, innerException,creationObjectType, dbFactoryBase)
        {

        }



        public ObjectCreationException(string message, Type creationObjectType, object dbFactoryBase)
            : base(message, creationObjectType, dbFactoryBase)
        {

        }

        public ObjectCreationException(Type creationObjectType,object dbFactoryBase)
            : base(creationObjectType,dbFactoryBase)
        {

        }

    }
}
