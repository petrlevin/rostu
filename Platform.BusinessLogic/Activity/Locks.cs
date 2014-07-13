using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.BusinessLogic.Activity
{
    /// <summary>
    /// Блокировки документа
    /// </summary>
    public class Locks
    {
        private readonly DbConnection _dbConnection;
        private Dictionary<IBaseEntity, bool> _locked = new Dictionary<IBaseEntity, bool>(new Comaparer());

        /// <summary>
        /// Создает блокировщик 
        /// </summary>
        /// <param name="dbConnection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Locks([Dependency("DbConnection")]SqlConnection dbConnection)
        {
            if (dbConnection == null) throw new ArgumentNullException("dbConnection");
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// Блокирует экземпляр сущности
        /// </summary>
        /// <param name="document"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="LockEntityException"></exception>
        public void Lock(IBaseEntity document)
        {
            if (!(document is IIdentitied))
                throw new ArgumentException(String.Format("Сущность {0} не потдерживает иентерфейс 'IIdentitied' . Блокировка не возможна. ", document),"document");
            if (_locked.ContainsKey(document))
                return;
            _locked.Add(document,true);
            ExecuteLock(document);
        }

        private void ExecuteLock(IBaseEntity document)
        {
            try
            {
				using (var command = _dbConnection.CreateCommand())
				{
					command.CommandText = String.Format(
						@"SET LOCK_TIMEOUT 0;
                SELECT COUNT(1) FROM [reg].[StartedOperation] WITH (XLOCK, ROWLOCK) WHERE [idRegistratorEntity]={1} AND [idRegistrator]={0};
                SET LOCK_TIMEOUT -1;",
						((IIdentitied) document).Id, document.EntityId);
					command.ExecuteNonQuery();
				}

            }
            catch (Exception)
            {
	            throw new LockEntityException(document, String.Format("Над объектом '{0}'  уже выполняется операция другим пользователем", document));
            }

        }


        class Comaparer : IEqualityComparer<IBaseEntity>
        {
            public bool Equals(IBaseEntity x, IBaseEntity y)
            {
                if (x == y)
                    return true;
                if ((x == null) || (y == null))
                    return false;
                if (ObjectContext.GetObjectType(x.GetType()) != ObjectContext.GetObjectType(y.GetType()))
                    return false;
                if (x is IIdentitied)
                    return ((IIdentitied)x).Id == ((IIdentitied)y).Id;
                return x.Equals(y);
            }

            public int GetHashCode(IBaseEntity obj)
            {
                if (obj is IIdentitied)
                    return ((IIdentitied) obj).Id.GetHashCode();
                else
                    return obj.GetHashCode();

            }
        }

    }
}
