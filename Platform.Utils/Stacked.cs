using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Platform.Utils
{

	public class Stacked<TThis> : IDisposable where TThis : Stacked<TThis>

    {
        [ThreadStatic] private static List<Stacked<TThis>> _listStack;

        public virtual void Dispose()
        {
            BeforeDispose();
            Previous = null;
            //Debug.WriteLine("Stacked.Dispose ");
            _listStack.RemoveAt(_listStack.Count - 1);
            
        }

        protected virtual void BeforeDispose()
        {
            
        }

        public Stacked()
        {
            if (_listStack==null)
                _listStack = new List<Stacked<TThis>>();
            Previous = Current;
            System.Diagnostics.Debug.WriteLineIf(Current != null, "Создание вложенной транзакции: ");
            _listStack.Add(this);

         }

        static public TThis Current
        {
            get
            {
                if (_listStack == null)
                    return null;
                if (_listStack.Count==0)
                    return null;
                return (TThis)_listStack.Last();
            }
        }

        static public int Count
        {
            get
            {
                if (_listStack == null)
                    return 0;
                return _listStack.Count;
            }
        }

        static public TThis Root
        {
            get
            {
                if (_listStack == null)
                    return null;
                if (_listStack.Count == 0)
                    return null;
                return (TThis)_listStack.First();
            }
            
        }

        public TThis Previous { get; private set; }
    }
}
