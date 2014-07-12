using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Ionic.Zip;
using Platform.Common;
using Tools.IrkutskBackUp.Logic;

namespace Tools.IrkutskBackUp
{
    public class SqlBackuper
    {
        private readonly SqlConnection _connection;
        private readonly string _localPath;

        private string _localFileName;
        private string _tempTableName;

        private bool _isArchivedBackup = false;

        private readonly Action<bool, string> _onBackupCreate;


        public SqlBackuper(SqlConnection connection, string localPath, Action<bool, string> onCreate)
        {
            _connection = connection;
            _localPath = localPath.TrimEnd('/');
            _onBackupCreate = onCreate;
        }

        public SqlBackuper(string localPath, Action<bool, string> onCreate):this(IoC.Resolve<SqlConnection>("DbConnection"), localPath, onCreate)
        {
            
        }

        public void CopyBacup()
        {
            var backupLocalPath = GetPathToBackup();
            if (String.IsNullOrEmpty(backupLocalPath))
            {
                _onBackupCreate(false, String.Format("Текущая база ('{0}') не имеет бэкапов", _connection.Database));
                return;
            }
            
            _localFileName = Path.GetFileName(backupLocalPath);
            _tempTableName = FindUniqueTemporaryTableName();

            if (!CreateTempTable())
                _onBackupCreate(false, "Не удалось создать временную таблицу");

            if (!WriteBackupInDb(backupLocalPath))
                _onBackupCreate(false, "Не удалось записать бэкап в БД");

            
            var sql = String.Format("SELECT bck FROM ##{0}", _tempTableName);

            using (var readStream = new SqlBlobReader(GetCommand(sql)))
            {
                const int bufferLength = 100000000;
                var buffer = new byte[bufferLength];
                var position = 0;

                using (var fs = new FileStream(String.Format("{0}\\{1}",
                                                             _localPath, _localFileName), FileMode.OpenOrCreate,
                                               FileAccess.Write))
                {
                    int readed;
                    while ((readed = readStream.Read(buffer, position, bufferLength)) > 0)
                    {
                        fs.Write(buffer, 0, readed);
                        position += bufferLength;
                    }
                }
            }

            DropTempTable();

            if (!_isArchivedBackup)
                ArchiveFile();    
            else
                _onBackupCreate(true, _localPath + "\\" + _localFileName + ".7z");
        }

        
        #region private

        private void ArchiveFile()
        {
            var archiveName = String.Format("{0}\\{1}.zip", _localPath, _localFileName);
            var fileName = String.Format("{0}\\{1}", _localPath, _localFileName);
            using (var zip = new ZipFile())
            {
                //zip.
                zip.AddFile(fileName);
                zip.Save(archiveName);
                _onBackupCreate(true, archiveName);
            }
        }

        private void DropTempTable()
        {
            var sql = String.Format("DROP TABLE ##{0}", _tempTableName);
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                GetCommand(sql).ExecuteNonQuery();
            }
            catch{}

        }

        private bool CreateTempTable()
        {
            string _sql;
            _sql = String.Format(@"IF OBJECT_ID('tempdb..##{0}') IS 
                                    NOT NULL DROP TABLE ##{0}", _tempTableName);

            try
            {
                GetCommand(_sql).ExecuteNonQuery();
            }
            catch
            {
                return false;
            }


            _sql = String.Format("CREATE TABLE ##{0} (bck VARBINARY(MAX))", _tempTableName);

            try
            {
                GetCommand(_sql).ExecuteNonQuery();
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool WriteBackupInDb(string filePath)
        {
            _isArchivedBackup = false;
            var sql = String.Format("INSERT INTO ##{0} SELECT bck.* FROM " +
                  @"OPENROWSET(BULK '{1}',SINGLE_BLOB) bck",
                  _tempTableName, filePath + ".7z");

            try
            {
                GetCommand(sql).ExecuteNonQuery();
                _isArchivedBackup = true;
            }
            catch{}

            if (!_isArchivedBackup)
            {
                sql = String.Format("INSERT INTO ##{0} SELECT bck.* FROM " +
                  @"OPENROWSET(BULK '{1}',SINGLE_BLOB) bck",
                  _tempTableName, filePath);

                try
                {
                    GetCommand(sql).ExecuteNonQuery();
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private string GetPathToBackup()
        {
            return GetCommand(@"
                declare @dbName varchar(128)
                set @dbName = DB_NAME();

                SELECT 
                    Top 1
	                bfam.physical_device_name
                FROM sys.sysdatabases sdb
	                LEFT OUTER JOIN msdb.dbo.backupset bus ON bus.database_name = sdb.name
	                LEFT OUTER JOIN msdb.dbo.backupmediafamily bfam on bfam.media_set_id = bus.media_set_id
                Where 
	                sdb.name = @dbName
                Order By 
                    bus.backup_finish_date DESC").ExecuteScalar().ToString();
        }

        private SqlCommand GetCommand(string cmd)
        {
            return new SqlCommand(cmd, _connection)
                {
                    CommandTimeout = 60*60*1000
                };
        }

        private SqlCommand GetCommand()
        {
            return GetCommand(null);
        }
        
        /// <summary>
        /// /// This function checks for temporary tables since we don't want to interfere with other programs/functions
        /// </summary>
        /// <returns></returns>
        private string FindUniqueTemporaryTableName()
        {
            const string name = "afpTempBackup";
            int counter = 0;
            var  mycommand = GetCommand();
            while (true)
            {
                ++counter;
                string sql = String.Format("SELECT OBJECT_ID('tempdb..##{0}') as id", name + counter.ToString());
                mycommand.CommandText = sql;
                if (mycommand.ExecuteScalar().ToString() == "")
                {
                    return name + counter.ToString();
                }
            }
        }
        #endregion
    }
}
