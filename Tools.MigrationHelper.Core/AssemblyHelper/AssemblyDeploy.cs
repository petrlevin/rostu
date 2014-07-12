using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.Core.Context;
using Tools.MigrationHelper.Core.Helpers;

namespace Tools.MigrationHelper.Core
{
    public class AssemblyDeploy
    {
        private readonly SqlConnection _connection;
        /// <summary>
        /// Контекст со справочником программируемые объекты
        /// </summary>
        private readonly ProgrammabilityContext _context;
        /// <summary>
        /// Список сборок
        /// </summary>
        private readonly List<string> _listAssemblies;
        /// <summary>
        /// Коллекция со сборками и набором зависящих объектов
        /// </summary>
        private readonly List<AssemblyObjects> _listObjects;
        /// <summary>
        /// Папка в которой происходит поиск сборок
        /// </summary>
        private readonly string _sourcePath;
        /// <summary>
        /// Путь к папке со списком сборок которые необходимо загрузить/обновить
        /// </summary>
        private readonly string _pathPlatformDbScripts = AppDomain.CurrentDomain.BaseDirectory + @"\PlatformDb\Scripts";
        /// <summary>
        /// Флаг того что объекты создались через справочник программируемые объекты
        /// </summary>
        private bool _createFromProgrammability;
        /// <summary>
        /// Какая таска выполняется
        /// На основе нее выполняем скрипты по созданию объектов или нет
        /// </summary>
        private readonly TasksEnum _task;
        /// <summary>
        /// Список полей которые не восстановились в исходное состояние после обновления сборок
        /// </summary>
        private readonly List<int> _notCreatedFields;

        /// <summary>
        /// сообщения о объектах которые не смог создать
        /// </summary>
        public string Message { get; protected set; }

        public AssemblyDeploy(string connectionString, List<string> listAssemblies, string sourcePath, int? task)
        {
            _listAssemblies = listAssemblies;
            _sourcePath = sourcePath;
            _task = task == null ? TasksEnum.DeployPlatformDb : (TasksEnum) task;
            _listObjects = new List<AssemblyObjects>();
            _context = new ProgrammabilityContext(connectionString);
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _createFromProgrammability = false;
            _notCreatedFields = new List<int>();
        }

        public void UpdateAssemblies()
        {
            try
            {
                AlterAssemblies();
            }
            catch (Exception)
            {
                DropAssemblies();
                CreateAssemblies();
            }
        }

        private void AlterAssemblies()
        {

            var directoryInfo2 = new DirectoryInfo(_pathPlatformDbScripts);
            FileInfo fileInfo2 = directoryInfo2.GetFiles("Assemblies.list")[0];
            List<string> listAssemblies = fileInfo2.OpenText()
                                                   .ReadToEnd()
                                                   .Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)
                                                   .ToList();
            const string textCommand =
                "ALTER ASSEMBLY [{0}] FROM {1} WITH PERMISSION_SET = UNSAFE";

            int count;
            do
            {
                count = listAssemblies.Count();
                var toDelete = new List<string>();
                foreach (string assembly in listAssemblies)
                {
                    var directoryInfo = new DirectoryInfo(_sourcePath + @"\bin");
                    if (!directoryInfo.Exists)
                    {
                        directoryInfo = new DirectoryInfo(_sourcePath + @"\" + assembly.Replace(".dll", @"\bin\debug"));
                        if (!directoryInfo.Exists)
                            throw new Exception("Не найден путь: '" + _sourcePath + @"\" +
                                                assembly.Replace(".dll", @"\bin\debug") + "'");
                    }
                    FileInfo file;
                    if (directoryInfo.GetFiles(assembly).Length == 1)
                    {
                        file = directoryInfo.GetFiles(assembly)[0];
                    }
                    else
                    {
                        throw new Exception("Не найден файл: '" + directoryInfo.FullName + @"\" + assembly);
                    }

                    var commandText = string.Format(textCommand,
                                                    file.Name.Substring(0, file.Name.Length - 4),
                                                    GetAssemblyBits(file.FullName));
                    try
                    {
                        TaskHelper.ExecuteSQlCommand(_connection, commandText);
                        toDelete.Add(assembly);
                    }
                    catch (SqlException e)
                    {
                        if (e.Number == 6285)
                            toDelete.Add(assembly);
                    }
                }

                foreach (var assembly in toDelete)
                {
                    listAssemblies.Remove(assembly);
                }

            } while (listAssemblies.Any() && listAssemblies.Count() != count);

            if (listAssemblies.Any())
            {
                throw new Exception("Ошибка публикации сборок :" + string.Join(",", listAssemblies));
            }
        }

        public void DropAssemblies()
        {
            for (var i = _listAssemblies.Count() - 1; i >= 0; i--)
            {
                var exists = false;
                using (var command = new SqlCommand("", _connection))
                {
                    command.CommandText = string.Format("SELECT * FROM sys.assemblies asms WHERE asms.name = N'{0}'", _listAssemblies[i].Replace(".dll",""));
                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                exists = true;
                            }
                            reader.Close();
                        }
                    }
                    catch (SqlException e)
                    {
                        throw new Exception("Ошибка проверки существования сборки на базе; " + e.Message);
                    }
                }

                if(exists)
                    DropAssembly(_listAssemblies[i]);
            }
        }

        public void CreateAssemblies()
        {
            foreach (var assembly in _listAssemblies)
            {
                CreateAssembly(assembly);

                CreateObjects(_listObjects.FirstOrDefault(f=>f.AssemblyName == assembly));
            }

            if(_notCreatedFields.Any())
                throw new Exception("Не восстановлены поля с id :" + string.Join(",", _notCreatedFields));

            if (!_createFromProgrammability && (_task == TasksEnum.DeployPlatformDb || _task == TasksEnum.Update))
            {
                if (_task == TasksEnum.Update)
                {
                    CreateTriggers();
                    CreateClrFunctions();
                }
                else
                {
                    foreach (var programmability in _context.Programmabilities.Where(w => w.IdProject == (int)SolutionProject.Tools_MigrationHelper_Core).ToList())
                    {
                        CreateObject(programmability);
                    }
                }
            }
        }

        #region private methods

        private void CreateObjects(AssemblyObjects assemblyObjects)
        {
            if (assemblyObjects != null)
            {
                _createFromProgrammability = true;
                // сначало триггеры, потом все остальное
                foreach (var programmability in assemblyObjects.ListObjects.Where(w=> !w.IsDisabled).OrderByDescending(o=>o.IdProgrammabilityType))
                {
                    CreateObject(programmability);
                }

                //говно код, надо отключить триггер EntityLogic и EntityFieldLogic
                if (_task == TasksEnum.Update)
                    TaskHelper.DisableEntityLogicTrigger(_connection);

                foreach (var fields in assemblyObjects.ListFields.Values)
                {
                    foreach (var field in fields)
                    {
                        var command = string.Format(CreateCalculatedType, field["id"],
                                                    field["idcalculatedfieldtype"]);
                        try
                        {
                            TaskHelper.ExecuteSQlCommand(_connection, command);
                            if (_notCreatedFields.Contains((int) field["id"]))
                                _notCreatedFields.Remove((int) field["id"]);
                        }
                        catch (Exception) //пропускаем
                        {
                            _notCreatedFields.Add((int) field["id"]);
                        }
                    }
                }
            }
        }

        private void CreateObject(Programmability programmability)
        {
            try
            {
                TaskHelper.ExecuteSQlCommand(_connection, programmability.CreateCommand);
            }
            catch (SqlException e)
            {
                Message += ("Ошибка создания объекта " + programmability.Name + "; " + e.Message + "; ");
            }
        }

        private void CreateAssembly(string assembly)
        {
            const string textCommand =
                    "CREATE ASSEMBLY [{0}] AUTHORIZATION [dbo] FROM {1} WITH PERMISSION_SET = UNSAFE";

            var directoryInfo = new DirectoryInfo(_sourcePath + @"\bin");
            if (!directoryInfo.Exists)
            {
                directoryInfo = new DirectoryInfo(_sourcePath + @"\" + assembly.Replace(".dll", @"\bin\debug"));
                if (!directoryInfo.Exists)
                    throw new Exception("Не найден путь: '" + _sourcePath + @"\" +
                                        assembly.Replace(".dll", @"\bin\debug") + "'");
            }

            FileInfo file;
            if (directoryInfo.GetFiles(assembly).Length == 1)
            {
                file = directoryInfo.GetFiles(assembly)[0];
            }
            else
            {
                throw new Exception("Не найден файл: '" + directoryInfo.FullName + @"\" + assembly);
            }

            try
            {
                var commandText = string.Format(textCommand,
                                            file.Name.Substring(0, file.Name.Length - 4),
                                            GetAssemblyBits(file.FullName));
                TaskHelper.ExecuteSQlCommand(_connection, commandText);
            }
            catch (SqlException e)
            {
                throw new Exception("Ошибка создания сборки " + assembly + "; " + e.Message);
            }
        }

        /// <summary>
        /// Удаление сборки
        /// </summary>
        /// <remarks>
        /// Находим зависимые объекты записываем их в список и удаляем
        /// </remarks>
        private void DropAssembly(string assembly)
        {
            var assemblyName = assembly.Replace(".dll", "");
            var items = GetAssemblyDepends(assemblyName);
            List<Programmability> list;
            try
            {
                list = _context.Programmabilities.ToList();
            }
            catch (Exception)
            {
                list = null;
            }

            if (list != null)
            {
                var ao = new AssemblyObjects
                    {
                        AssemblyName = assembly,
                        ListObjects = list.Where(w => items.Contains(w.Name)).ToList()
                    };

                //сортировка для того чтобы триггеры оказались позже всех
                foreach (var programmability in ao.ListObjects.Where(w=> !w.IsDisabled).OrderBy(o=>o.IdProgrammabilityType))
                {
                    var listDependFields = new List<Dictionary<string, object>>();
                    var depends = Select(string.Format(SelectDepends, programmability.Id, Programmability.EntityIdStatic));

                    foreach (var depend in depends)
                    {
                        listDependFields.AddRange(Select(string.Format(SelectField, depend["idobject"])));
                    }

                    foreach (var field in listDependFields)
                    {
                        var command = string.Format(DeleteCalculatedType, field["id"]);
                        TaskHelper.ExecuteSQlCommand(_connection,command);
                    }

                    ao.ListFields.Add(programmability, listDependFields);

                    DropObject(programmability);
                }
                _listObjects.Add(ao);


                var dropCommand = string.Format("DROP ASSEMBLY [{0}]", assemblyName);

                try
                {
                    TaskHelper.ExecuteSQlCommand(_connection, dropCommand);
                }
                catch (Exception e)
                {
                    throw new Exception("Ошибка удаления сборки " + assembly + "; " + e.Message);
                }
            }
            else
            {
                var directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                var fileInfo = directoryInfo.GetFiles("DropAssembly.sql")[0];
                if (!fileInfo.Exists)
                    throw new Exception("Не найден файл DropAssembly.sql");
                string textCommand = fileInfo.OpenText().ReadToEnd();

                TaskHelper.ExecuteSQlCommand(_connection, string.Format(textCommand, assemblyName));
            }

        }

        private List<string> GetAssemblyDepends(string assembly)
        {
            var items = new List<string>();
            var command = string.Format("SELECT t.object_id, t.name, t.type, t.parent_class as class " +
                                        "FROM sys.triggers t " +
                                        "INNER JOIN sys.assembly_modules m ON t.object_id = m.object_id " +
                                        "INNER JOIN sys.assemblies a ON m.assembly_id = a.assembly_id " +
                                        "WHERE a.name = N'{0}' " +
                                        "UNION " +
                                        "SELECT o.object_id, o.name, o.type, NULL as class " +
                                        "FROM sys.objects o " +
                                        "INNER JOIN sys.assembly_modules m ON o.object_id = m.object_id " +
                                        "INNER JOIN sys.assemblies a ON m.assembly_id = a.assembly_id " +
                                        "WHERE a.name = N'{0}'", assembly);
            var result = Select(command);

            foreach (var dic in result)
            {
                if (dic.ContainsKey("name"))
                {
                    var name = dic["name"].ToString();
                    if (!items.Contains(name))
                        items.Add(name);
                }
            }
            return items;
        }

        private void DropObject(Programmability programmability)
        {
            string command;
            switch (programmability.IdProgrammabilityType)
            {
                case (int)ProgrammabilityType.Function:
                    command = string.Format(DropFunction, programmability.Schema, programmability.Name);
                    break;
                case (int)ProgrammabilityType.StoredProcedure:
                    command = string.Format(DropStoredProcedure, programmability.Schema, programmability.Name);
                    break;
                case (int)ProgrammabilityType.Trigger:
                    command = string.Format(DropTrigger, programmability.Schema, programmability.Name);
                    break;
                case (int)ProgrammabilityType.Aggreagate:
					command = string.Format(DropAggregate, programmability.Schema, programmability.Name);
                    break;
                default:
                    throw new NotImplementedException();
            }

            TaskHelper.ExecuteSQlCommand(_connection,command);
        }

        /// <summary>
        /// Создание триггеров для таблиц ref.Entity и ref.EntityField
        /// </summary>
        private void CreateTriggers()
        {
            try
            {
                var directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                var fileInfo = directoryInfo.GetFiles("CreateTriggers.list")[0];
                if (!fileInfo.Exists)
                    throw new Exception("Не найден файл CreateTriggers.list");
                ExecListCommandFromFile(fileInfo);
            }
            catch (Exception ex)
            {
                throw new Exception("Фатальная ошибка при cоздании триггеров для таблиц ref.Entity и ref.EntityField",
                                    ex);
            }
        }

        /// <summary>
        /// Создание CLR функций
        /// </summary>
        private void CreateClrFunctions()
        {
            try
            {
                var directoryInfo = new DirectoryInfo(_pathPlatformDbScripts);
                FileInfo fileInfo = directoryInfo.GetFiles("CreateCLRFunctions.list")[0];
                if (!fileInfo.Exists)
                    throw new Exception(
                        "Фатальная ошибка при создании CLR функций. Не найден файл CreateCLRFunctions.list");
                ExecListCommandFromFile(fileInfo);
            }
            catch (Exception ex)
            {
                throw new Exception("Фатальная ошибка при создании CLR функций; " + ex.Message);
            }

        }

        /// <summary>
        /// Выполнение списка команд перечисленного в файле
        /// </summary>
        /// <param name="file">Файл</param>
        private void ExecListCommandFromFile(FileInfo file)
        {
            string[] textCommands = file.OpenText().ReadToEnd().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string textCommand in textCommands)
            {
                TaskHelper.ExecuteSQlCommand(_connection, textCommand);
            }
        }

        internal const string DropFunction =
            "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')) DROP FUNCTION [{0}].[{1}]";

        internal const string DropStoredProcedure =
            "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'P', N'PC')) DROP PROCEDURE [{0}].[{1}]";

        internal const string DropTrigger =
            "IF EXISTS (SELECT * FROM sys.triggers WHERE name = '{1}') DROP TRIGGER [{0}].[{1}]";

        public const string SelectDepends =
            "SELECT * FROM reg.ItemsDependencies WHERE idDependsOn = '{0}' AND idDependsOnEntity = {1}";

        public const string DeleteItemDep =
            "DELETE FROM reg.ItemsDependencies WHERE idObject = '{0}' AND idObjectEntity = {1}";

        public const string SelectField =
            "SELECT * FROM ref.EntityField WHERE [idEntity] = '{0}' AND [IdCalculatedFieldType] = 3";

        public const string DeleteCalculatedType =
            "UPDATE ref.EntityField SET [idCalculatedFieldType] = NULL WHERE id = '{0}'";

        public const string CreateCalculatedType =
            "UPDATE ref.EntityField SET [idCalculatedFieldType] = {1} WHERE id = '{0}'";

        public const string DropAggregate =
            "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'AF')) DROP AGGREGATE  [{0}].[{1}]";

        public List<Dictionary<string, object>> Select(string command)
        {
            var result = new List<Dictionary<string, object>>();
            var cmd = new SqlCommand(command,_connection);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int col = 0; col <= reader.FieldCount - 1; col++)
                    {
                        row.Add(reader.GetName(col).ToLowerInvariant(), reader[col]);
                    }

                    result.Add(row);
                }
                reader.Close();
            }
            return result;
        }

        /// <summary>
        /// Класс описывающий сборку и зависимые от нее объекты
        /// </summary>
        private class AssemblyObjects
        {
            public string AssemblyName;
            public List<Programmability> ListObjects;
            public readonly Dictionary<Programmability, List<Dictionary<string, object>>> ListFields;

            public AssemblyObjects()
            {
                ListObjects = new List<Programmability>();
                ListFields = new Dictionary<Programmability, List<Dictionary<string, object>>>();
            }
        }

        private string GetAssemblyBits(string assemblyPath)
        {
            var builder = new StringBuilder();
            builder.Append("0x");

            using (var stream = new FileStream(assemblyPath,
                                                      FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int currentByte = stream.ReadByte();
                while (currentByte > -1)
                {
                    builder.Append(currentByte.ToString("X2", CultureInfo.InvariantCulture));
                    currentByte = stream.ReadByte();
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
