using System;
using System.Data;

namespace Platform.Utils.Extensions
{
    public static class DataRecordExtension
    {
        public static object GetValue(this IDataRecord dbReader, string columnName)
        {
            return dbReader.GetValue(dbReader.GetOrdinal(columnName));

        }

        public static String GetString(this IDataRecord dbReader, string columnName)
        {
            var result = dbReader.GetValue(dbReader.GetOrdinal(columnName));
            if (result is DBNull)
                return null;
            else
            {
                return (String)result;
            }


        }

        public static Boolean GetBoolean(this IDataRecord dbReader, string columnName)
        {
            return dbReader.GetBoolean(dbReader.GetOrdinal(columnName));

        }

        public static Byte GetByte(this IDataRecord dbReader, string columnName)
        {
            return dbReader.GetByte(dbReader.GetOrdinal(columnName));

        }

        public static Char GetChar(this IDataRecord dbReader, string columnName)
        {
            return dbReader.GetChar(dbReader.GetOrdinal(columnName));

        }

        public static Int32 GetInt32(this IDataRecord dbReader, string columnName)
        {
            return dbReader.GetInt32(dbReader.GetOrdinal(columnName));

        }

        public static Int64 GetInt64(this IDataRecord dbReader, string columnName)
        {
            return dbReader.GetInt64(dbReader.GetOrdinal(columnName));

        }
    }
}