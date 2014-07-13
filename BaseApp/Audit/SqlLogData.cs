using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platform.BusinessLogic.Auditing.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.Dal;


namespace BaseApp.Audit
{
    /// <summary>
    /// Sql-логгер изменений в данных
    /// </summary>
    public class SqlLogData :ISqlLogData
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        public int ElementId { get; set; }
        
        /// <summary>
        /// Тип операции (CUD)
        /// </summary>
        public Operations Operation { get; set; }
        
        /// <summary>
        /// Слепок элемента на момент начала операции
        /// </summary>
        public string XmlBefore { get; set; }
        
        /// <summary>
        /// Слепок элемента после операции
        /// </summary>
        public string XmlAfter { get; set; }

        /// <summary>
        /// Команда для записи лога в базу
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public SqlCommand CreateCommand(SqlConnection connection, int userId)
        {
            var comm = new SqlCommandFactory(connection).CreateCommand();

            comm.CommandText = @"
                        INSERT INTO [dbo].[data]
                        (" + ((XmlBefore == null) ? "" : @"
                         [Before] ,")
                               + ((XmlAfter == null) ? "" : @"
                        [After] ,") + @"
                        [EntityId],
                        [Operation],
                        [ElementId] ,
                        [IdUser] ,
                        [Date])
                    VALUES
                        (
                         " + ((XmlBefore == null) ? "" : @"
                         @Before ,")
                               + ((XmlAfter == null) ? "" : @"
                        @After ,") + @"
                        @EntityId,
                        @Operation ,
                        @ElementId ,
                        @IdUser ,
                        @Date)";

            if (XmlBefore != null)
                comm.Parameters.Add("@Before", SqlDbType.Xml);
            if (XmlAfter != null)
                comm.Parameters.Add("@After", SqlDbType.Xml);
            comm.Parameters.Add("@EntityId", SqlDbType.Int);
            comm.Parameters.Add("@Operation", SqlDbType.TinyInt);
            comm.Parameters.Add("@ElementId", SqlDbType.Int);
            comm.Parameters.Add("@IdUser", SqlDbType.Int);
            comm.Parameters.Add("@Date", SqlDbType.DateTime);

            if (XmlBefore != null)
                comm.Parameters["@Before"].Value = XmlBefore;
            if (XmlAfter != null)
                comm.Parameters["@After"].Value = XmlAfter;
            comm.Parameters["@EntityId"].Value = EntityId;
            comm.Parameters["@Operation"].Value = (Int16) Operation;
            comm.Parameters["@ElementId"].Value = ElementId;
            comm.Parameters["@IdUser"].Value = userId;
            comm.Parameters["@Date"].Value = DateTime.Now;
            return comm;


        }

        /// <summary>
        /// Возвращает строку, которая представляет текущий объект.
        /// </summary>
        /// <returns>
        /// Строка, представляющая текущий объект.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
