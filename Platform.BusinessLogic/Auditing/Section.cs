using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Auditing
{
    /// <summary>
    /// Секция параметров логирования действий пользователя
    /// </summary>
    public class Section : ConfigurationSection
    {
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
        /// Логировать время выполненеия действий над элементами (операции/контроли/etc)
        /// </summary>
        [ConfigurationProperty("operationsEnabled", IsRequired = false)]
        public bool OperationsEnabled
        {
            get { return (bool)this["operationsEnabled"]; }
            set { this["operationsEnabled"] = value; }
        }

        /// <summary>
        /// Логировать время выполненеия действий над элементами (операции/контроли/etc)
        /// </summary>
        [ConfigurationProperty("requestsEnabled", IsRequired = false)]
        public bool RequestsEnabled
        {
            get { return (bool)this["requestsEnabled"]; }
            set { this["requestsEnabled"] = value; }
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

    }
}
