using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Platform.BusinessLogic.Queries
{
    internal class InterfaceQueryable<TInterface> : IQueryable<TInterface>  
    {
        private IQueryable _inner;

        public InterfaceQueryable(IQueryable inner)
        {
            _inner = inner;
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return (IEnumerator<TInterface>) _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public Expression Expression
        {
            get
            {
                return _inner.Expression;
            }
        }

        public Type ElementType
        {
            get { return typeof (TInterface); }
        }

        public IQueryProvider Provider
        {
            get
            {
                return _inner.Provider;
            }
        }
    }
}
