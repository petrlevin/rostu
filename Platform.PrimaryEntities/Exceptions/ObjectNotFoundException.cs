using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Exceptions
{
    public class ObjectNotFoundException : ObjectException
    {
        private object _foundBy ;
        public ObjectNotFoundException(string message, Exception innerException, Type creationObjectType, object dbFactoryBase, object foundBy)
            : base(message, innerException,creationObjectType, dbFactoryBase)
        {
            _foundBy = foundBy;
        }


        public ObjectNotFoundException(string message, Type creationObjectType, object dbFactoryBase, object foundBy)
            : this(message,null, creationObjectType, dbFactoryBase,foundBy)
        {
            _foundBy = foundBy;
        }

        public ObjectNotFoundException(Type creationObjectType, object dbFactoryBase, object foundBy)
            : this(null, creationObjectType, dbFactoryBase, foundBy)
        {
            _foundBy = foundBy;
        }

        public object FoundBy
        {
            get { return _foundBy; }
        }
    }
}
