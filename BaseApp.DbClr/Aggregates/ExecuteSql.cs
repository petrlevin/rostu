using System.Data.SqlClient;
using Microsoft.SqlServer.Server;


namespace BaseApp.DbClr.Aggregates
{
    /// <summary>
    /// Concatenates the strings with a given delimiter
    /// </summary>
    [System.Serializable]
    [Microsoft.SqlServer.Server.SqlUserDefinedAggregate(
       Microsoft.SqlServer.Server.Format.UserDefined,
       IsInvariantToDuplicates = false, // Receiving the same value again 
        // changes the result
       IsInvariantToNulls = true,      // Receiving a NULL value changes the result
       IsInvariantToOrder = false,      // The order of the values affects the result
       IsNullIfEmpty = true,            // If no values are given the result is null
       MaxByteSize = -1,
       Name = "ExecuteSql"             // Name of the aggregate
    )]

    public struct ExecuteSql : IBinarySerialize
    {
        private SqlConnection _connection;
        private SqlCommand _command;

        /// <summary>
        /// Инициализация
        /// </summary>
        public void Init()
        {
            _connection = new SqlConnection("context connection = true");
            _connection.Open();
            _command = _connection.CreateCommand();
        }

        /// <summary>
        /// Inserts a new string into the existing already concatenated string
        /// </summary>
        /// <param name="stringval">Value to include</param>
        /// <param name="delimiter">Delimiter to use</param>
        public void Accumulate(System.Data.SqlTypes.SqlString stringval)
        {
                _command.CommandText = stringval.Value;
                _command.ExecuteNonQuery();
        }


        /// <summary>
        /// Merges this group to another group instantiated for the concatenation
        /// </summary>
        /// <param name="group"></param>
        public void Merge(ExecuteSql group)
        {
            // Merge only if the group has a value
        }

        /// <summary>
        /// Ends the operation and returns the result
        /// </summary>
        /// <returns></returns>
        public System.Data.SqlTypes.SqlString Terminate()
        {
            _command.Dispose();
            _connection.Close();
            _connection.Dispose();
            return "";
        }


        #region IBinarySerialize
        /// <summary>
        /// Writes the values to the stream in order to be stored
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        public void Write(System.IO.BinaryWriter writer)
        {
        }

        /// <summary>
        /// Reads the values from the stream
        /// </summary>
        /// <param name="reader">The BinaryReader stream</param>
        public void Read(System.IO.BinaryReader reader)
        {
        }
        #endregion IBinarySerialize


    }
}
