using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Dal;
using Platform.Dal.Serialization;
using Platform.Log;

namespace Platform.BusinessLogic.Activity.Operations.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public class RestoreCommands :SerializationCommandFactory
    {
        private readonly List<KeyValuePair<String, String>> _inserts = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandText"></param>
        public RestoreCommands(string commandText) : base(commandText)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="connection"></param>
        public void Execute(int docId , SqlConnection connection)
        {
            var main = CreateCommand(docId);
            main.Connection = connection;
            main.ExecuteNonQueryLog();
            foreach (var insert in _inserts)
            {
                CreateIdentityInsert(insert.Key, connection, IdentityInsert.On).ExecuteNonQueryLog();
                var com = CreateCommand(docId, insert.Value);
                com.Connection = connection;
                try
                {
                    com.ExecuteNonQueryLog();
                }
                finally 
                {
                    CreateIdentityInsert(insert.Key, connection, IdentityInsert.Off).ExecuteNonQueryLog();
                }

            }
        }


        private SqlCommand CreateIdentityInsert(string fullTableName,SqlConnection connection ,IdentityInsert identityInsert)
        {
            return new SqlCommandFactory(String.Format(" SET IDENTITY_INSERT {0} {1} ", fullTableName, identityInsert), connection).CreateCommand();
        }

        private enum IdentityInsert
        {
            On =1,
            Off = 2
        }




        internal void AddInsert(string fullTableName,string commandText)
        {
            _inserts.Add(new KeyValuePair<string, string>(fullTableName, commandText));
        }
        
    }
}
