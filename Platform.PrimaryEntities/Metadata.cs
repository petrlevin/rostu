using System;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities
{
	/// <summary>
	/// Базовый класс для всех первичных сущностных классов. 
	/// Предполягается, что любой первичных сущностной класс может (но не обязан) реализовывать интерфейс <see cref="ITriggerAction"/>. 
	/// При реализации можно обращаться к БД через свойство sqlCmd.
	/// </summary>
	public abstract class Metadata: BaseEntity
	{
		private SqlCmd _sqlCmd;

        protected internal Metadata()
        {
            
        }

	    protected SqlCmd sqlCmd
		{
			get
			{
				if (_sqlCmd == null)
					_sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);
				return _sqlCmd;
			}
		}

        public static Func<IFactory> GetObjects { get; set; }

	    protected IFactory Objects
	    {
	        get
	        {
	            if (GetObjects == null)
	                throw new  InvalidOperationException(
	                    "Метаданные не настроены для манипулирования объектами. Значение функтора 'GetObjects' не установлено (null)");
	            return GetObjects();
	        }
	    }



	}
}
