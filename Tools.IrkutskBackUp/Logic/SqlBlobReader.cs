using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace Tools.IrkutskBackUp.Logic
{
    /// <summary>
    /// Потоковое чтение бинарных данных с Sql-сервера
    /// There must be a SqlConnection that works inside the SqlCommand. Remember to dispose of the object after usage.
    /// </summary>
    public class SqlBlobReader : Stream
    {
        private readonly SqlCommand command;
        private readonly SqlDataReader dataReader;
        private bool disposed = false;
        private long currentPosition = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="command">The supplied <para>sqlCommand</para> must only have one field in select statement, or else the stream won't work. Select just one row, all others will be ignored.</param>
        public SqlBlobReader(SqlCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (command.Connection == null)
                throw new ArgumentException("The internal Connection cannot be null", "command");
            if (command.Connection.State != ConnectionState.Open)
                throw new ArgumentException("The internal Connection must be opened", "command");
            dataReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            dataReader.Read();
            this.command = command; // only stored for disposal later
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            long returned = dataReader.GetBytes(0, currentPosition, buffer, 0, buffer.Length);
            currentPosition += returned;
            return Convert.ToInt32(returned);
        }

        /// <summary>
        /// Not supported
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (dataReader != null)
                        dataReader.Dispose();
                    disposed = true;
                }
            }
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

    }
}
