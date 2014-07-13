using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Platform.BusinessLogic.Auditing
{
    /// <summary>
    /// Настройка аудита
    /// </summary>
    public partial class AuditConfiguration : ConfigurationSection
    {
        #region Свойства секции <audit> в web.config

        /// <summary>
        /// Разрешить логирование
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Строка подключения
        /// </summary>
        [ConfigurationProperty("connectionstringName", IsRequired = false)]
        public string ConnectionstringName
        {
            get { return (String)this["connectionstringName"]; }
            set { this["connectionstringName"] = value; }
        }

        /// <summary>
        /// Логировать время выполненеия действий над элементами (операции/контроли/etc)
        /// </summary>
        [ConfigurationProperty("operationsEnabled", IsRequired = false)]
        public bool OperationsEnabled
        {
            get { return (bool)this["operationsEnabled"]; }
            set { this["operationsEnabled"] = value; }
        }

        /// <summary>
        /// Логировать клиентские запросы
        /// </summary>
        [ConfigurationProperty("requestsEnabled", IsRequired = false)]
        public bool RequestsEnabled
        {
            get { return (bool)this["requestsEnabled"]; }
            set { this["requestsEnabled"] = value; }
        }

        #endregion

        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString { get; private set; }
        
        /// <summary>
        /// Строка подключения в объектном виде
        /// </summary>
        public SqlConnectionStringBuilder ConnectionStringBuilder
        {
            get { return new SqlConnectionStringBuilder(ConnectionString); }
        }

    }
}
