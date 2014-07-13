using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.Interfaces
{
    /// <summary>
    /// интерфейс для объектов у которых есть строка подключения
    /// </summary>
    public interface IDbConnectioned
    {
        SqlConnection DbConnection { get; }
    }
    
}
