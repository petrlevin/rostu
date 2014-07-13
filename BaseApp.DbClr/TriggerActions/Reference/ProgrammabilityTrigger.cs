using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Platform.DbClr.Interfaces;
using Platform.DbCmd;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.TriggerActions.Reference
{
	/// <summary>
	/// Триггеры для сущности Programmability
	/// </summary>
	public class ProgrammabilityTrigger : ITriggerAction<Programmability>
	{

		private void _dropObject(Programmability programmability)
		{
			switch (programmability.ProgrammabilityType)
			{
				case ProgrammabilityType.Function:
					_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropFunction, programmability.Schema, programmability.Name));
					break;
				case ProgrammabilityType.StoredProcedure:
					_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropStoredProcedure, programmability.Schema, programmability.Name));
					break;
				case ProgrammabilityType.Trigger:
					_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropTrigger, programmability.Schema, programmability.Name));
					break;
                case ProgrammabilityType.Aggreagate:
					_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropAggregate, programmability.Schema, programmability.Name));
					break;
				case ProgrammabilityType.View:
					_sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DropView, programmability.Schema, programmability.Name));
					break;
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Класс для формирования SQL команд
		/// </summary>
		private static class SqlTpl
		{
			internal const string DropFunction =
                "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')) DROP FUNCTION [{0}].[{1}]";

			internal const string DropStoredProcedure =
				"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'P', N'PC')) DROP PROCEDURE [{0}].[{1}]";

			internal const string DropTrigger =
                "IF EXISTS (SELECT * FROM sys.triggers WHERE name = '{1}') DROP TRIGGER [{0}].[{1}]";
            
			internal const string DropAggregate =
                "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'AF')) DROP AGGREGATE  [{0}].[{1}]";

			internal const string DropView =
				"IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[{0}].[{1}]')) DROP VIEW [{0}].[{1}]";

		    private const string _checkExistFunction =
                "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}.{1}') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT')";

		    private const string _checkExistProcedure =
                "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}.{1}') AND type in (N'P', N'PC')";

		    private const string _checkExistTrigger =
                "SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'{0}.{1}')";

		    private const string _checkExistAggregate =
                "SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}.{1}') AND type = N'AF'";

			private const string _checkExistView =
				"SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[{0}].[{1}]')";

            public const string SelectDepends =
                "SELECT * FROM reg.ItemsDependencies WHERE idDependsOn = '{0}' AND idDependsOnEntity = {1}";

            public const string DeleteItemDep =
                "DELETE FROM reg.ItemsDependencies WHERE (idObject = '{0}' AND idObjectEntity = {1}) OR (idDependsOn = '{0}' AND idDependsOnEntity = {1})";

            public const string SelectField =
                "SELECT * FROM ref.EntityField WHERE [idEntity] = '{0}' AND [IdCalculatedFieldType] = 3";

            public const string DeleteCalculatedType =
                "UPDATE ref.EntityField SET [idCalculatedFieldType] = NULL, [AllowNull] = 1  WHERE id = '{0}'";

            public const string CreateCalculatedType =
                "UPDATE ref.EntityField SET [idCalculatedFieldType] = {1}, [AllowNull] = {2} WHERE id = '{0}'";

		    public const string ReturnAllowNullToField =
		        "UPDATE ref.EntityField SET [AllowNull] = {1} WHERE id = '{0}'";

			public static string AlterObject(ProgrammabilityType programmabilityType, string createCommand)
			{
				switch (programmabilityType)
				{
					case ProgrammabilityType.Function:
						return createCommand.Replace("CREATE FUNCTION", "ALTER FUNCTION");
					case ProgrammabilityType.StoredProcedure:
						return createCommand.Replace("CREATE PROCEDURE", "ALTER PROCEDURE");
                    case ProgrammabilityType.Trigger:
                        return createCommand.Replace("CREATE TRIGGER", "ALTER TRIGGER");
                    case ProgrammabilityType.Aggreagate:
                        return createCommand.Replace("CREATE AGGREGATE", "ALTER AGGREGATE");
					case ProgrammabilityType.View:
						return createCommand.Replace("CREATE VIEW", "ALTER VIEW");

					default:
						throw new NotImplementedException();
				}
			}

		    public static string GetTriggerOrderCommand(string createCommand, string order)
		    {
		        var sBuilder = new StringBuilder();
                var commandsString = Regex.Match(createCommand, "(AFTER|INSTEAD OF)(?<command>.*?)AS", RegexOptions.Singleline).Groups["command"].Value;
		        var commands = commandsString.Replace("\n","").Replace("\r","").Split(',');
		        foreach (var command in commands)
		        {
                    var name = Regex.Match(createCommand, "TRIGGER(?<name>.*?)ON", RegexOptions.Singleline).Groups["name"].Value.Trim();
		            sBuilder.AppendFormat("exec sp_settriggerorder @triggername='{0}', @order='{1}', @stmttype='{2}';",
                                          name, order, command.Trim());
		        }
                return sBuilder.ToString();
		    }

            public static List<Dictionary<string, object>> CheckExist(Programmability item)
		    {
                switch (item.ProgrammabilityType)
                {
                    case ProgrammabilityType.Function:
                        return _sqlCmd.Select2(string.Format(_checkExistFunction, item.Schema, item.Name));
                    case ProgrammabilityType.StoredProcedure:
                        return _sqlCmd.Select2(string.Format(_checkExistProcedure, item.Schema, item.Name));
                    case ProgrammabilityType.Trigger:
                        return _sqlCmd.Select2(string.Format(_checkExistTrigger, item.Schema, item.Name));
                    case ProgrammabilityType.Aggreagate:
                        return _sqlCmd.Select2(string.Format(_checkExistAggregate, item.Schema, item.Name));
					case ProgrammabilityType.View:
						return _sqlCmd.Select2(string.Format(_checkExistView, item.Schema, item.Name));

                    default:
                        throw new NotImplementedException();
                }
		    }
		}

		/// <summary>
		/// Экземпляр SqlCmd для выполненения команд
		/// </summary>
		private static readonly SqlCmd _sqlCmd = new SqlCmd(SqlCmd.ContextConnection, ConnectionType.ConnectionPerCommand);

		#region Implementation of ITriggerAction<Programmability>

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
		public void ExecInsertCmd(List<Programmability> inserted)
		{
			foreach (Programmability insert in inserted)
			{
				_dropObject(insert);
			    if (insert.IsDisabled) continue;
			    _sqlCmd.ExecuteNonQuery(insert.CreateCommand);
			    if (insert.ProgrammabilityType == ProgrammabilityType.Trigger && !string.IsNullOrEmpty(insert.Order))
			        _sqlCmd.ExecuteNonQuery(SqlTpl.GetTriggerOrderCommand(insert.CreateCommand, insert.Order));
			}
		}

		/// <summary>
		/// Релизация триггера UPDATE
		/// </summary>
		public void ExecUpdateCmd(List<Programmability> inserted, List<Programmability> deleted)
		{
		    foreach (Programmability delete in deleted)
		    {
		        var listDependFields = new List<Dictionary<string, object>>();
		        Programmability insert = inserted.SingleOrDefault(a => a.Id == delete.Id);
		        if (insert == null)
		            throw new Exception("Не найдена запись изменения");
		        if (delete.IdProgrammabilityType != insert.IdProgrammabilityType)
		            throw new Exception("Нельзя менять тип объекта. Удалите старый и создайте новый");

		        if (delete.IsDisabled && !insert.IsDisabled)
		        {
                    _dropObject(delete);
                    _sqlCmd.ExecuteNonQuery(insert.CreateCommand);
		        }
                if (!delete.IsDisabled && insert.IsDisabled)
                {
                    _dropObject(delete);
                }

		        var depends =
		            _sqlCmd.Select2(string.Format(SqlTpl.SelectDepends, insert.Id, Programmability.EntityIdStatic));
		        if (!delete.CreateCommand.Equals(insert.CreateCommand, StringComparison.OrdinalIgnoreCase) && !insert.IsDisabled && !delete.IsDisabled)
		        {
		            foreach (var depend in depends)
		            {
		                listDependFields.AddRange(_sqlCmd.Select2(string.Format(SqlTpl.SelectField, depend["idobject"])));
		            }

		            foreach (var field in listDependFields)
		            {
		                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DeleteCalculatedType, field["id"]));
		            }
		            if (delete.Name != insert.Name || delete.Schema != insert.Schema)
		            {
		                _dropObject(delete);
		                _sqlCmd.ExecuteNonQuery(insert.CreateCommand);
		            }
		            else
		            {
		                var result = SqlTpl.CheckExist(insert);
		                if (result.Any())
		                {
		                    _sqlCmd.ExecuteNonQuery(SqlTpl.AlterObject(insert.ProgrammabilityType, Convert.ToString(insert.CreateCommand)));
		                }
		                else
		                {
		                    _sqlCmd.ExecuteNonQuery(insert.CreateCommand);
		                }
		            }
		            foreach (var field in listDependFields)
		            {
		                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.CreateCalculatedType, field["id"],
                           field["idcalculatedfieldtype"], 1));
                        _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.ReturnAllowNullToField, field["id"], (bool)field["allownull"] ? 1 : 0));
		            }
		        }
		    }
		}

		/// <summary>
		/// Релизация триггера DELETE
		/// </summary>
		public void ExecDeleteCmd(List<Programmability> deleted)
		{
            foreach (var delete in deleted)
		    {
		        _dropObject(delete);
                _sqlCmd.ExecuteNonQuery(string.Format(SqlTpl.DeleteItemDep,delete.Id, delete.EntityId));
		    }
		}
	    #endregion
	}
}
