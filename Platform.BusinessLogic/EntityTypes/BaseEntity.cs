using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.EntityTypes
{
    public class BaseEntity : Platform.PrimaryEntities.BaseEntity
    {


        public override bool Equals(object obj)
        {
            if ((EqualityComparer == null) || (!(obj is IBaseEntity)))
                return base.Equals(obj);
            
            var result = EqualityComparer.Equals(this, (IBaseEntity)obj);
            try
            {
                if ((this.ToString() == obj.ToString()) && (!result))
                {
                    _logger.Debug("Сравнимаем {0}{1} и {2}{3} ", this.GetType(), this, obj.GetType(), obj);
                    _logger.Debug("Результат - {0}", result);
                }

            }
            catch (Exception)
            {
                

            }
            return result;

        }

        public override int GetHashCode()
        {
            if (EqualityComparer == null)
                return base.GetHashCode();
            return EqualityComparer.GetHashCode();

        }

        [ThreadStatic]
        private  static IEqualityComparer<IBaseEntity> EqualityComparer;

        private class UsingComparer :IDisposable
        {
            public UsingComparer(IEqualityComparer<IBaseEntity> equalityComparer)
            {
                EqualityComparer = equalityComparer;
            }

            public void Dispose()
            {
                EqualityComparer = null;
            }
        }


        static public IDisposable UseComparer(IEqualityComparer<IBaseEntity> equalityComparer)
        {
            return new UsingComparer(equalityComparer);
        }


        private static Logger _logger = LogManager.GetCurrentClassLogger();

    }
}
