//------------------------------------------------------------------------------
// <copyright file="CSSqlClassFile.cs" company="Microsoft">
//	 Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr.Interfaces;
using Platform.DbCmd;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.DbClr
{
	/// <summary>
	/// Базовый класс для DML-триггеров (DML - Data Manipulation Language)
	/// </summary>
	public abstract class DmlTriggerBase: ClrBase, ITrigger
	{
		#region Public Members
		public virtual void Process()
		{
			//SqlContext.Pipe.Send("Trigger FIRED");
			if (sqlCmd.ExecuteScalar<int>("SELECT @@ROWCOUNT") == 0)
				return;

			switch (SqlContext.TriggerContext.TriggerAction)
			{
				case TriggerAction.Insert:
					Insert();
					break;
				case TriggerAction.Update:
					Update();
					break;
				case TriggerAction.Delete:
					Delete();
					break;
				default:
					throw new ArgumentOutOfRangeException("Неподдерживаемое значение параметра SqlContext.TriggerContext.TriggerAction");
			}
		}
		#endregion

		#region Protected Methods

		/// <summary>
		/// Более короткая форма обращения контексту триггера (вместо SqlContext.TriggerContext).
		/// </summary>
		protected SqlTriggerContext triggerContext
		{
			get
			{
				return SqlContext.TriggerContext;
			}
		}

		/// <summary>
		/// Строки из служебной таблицы INSERTED
		/// </summary>
		protected DataTable inserted
		{
			get { return GetDataRowsLazy(insertedTblName); }
		}
		
		/// <summary>
		/// Строки из служебной таблицы DELETED
		/// </summary>
		protected DataTable deleted
		{
			get { return GetDataRowsLazy(deletedTblName); }
		}

		protected virtual void Insert()
		{
		}

		protected virtual void Update()
		{
		}

		protected virtual void Delete()
		{
		}

		/// <summary>
		/// Возвращает список объектов, соотвутствующих строкам из таблицы INSERTED
		/// </summary>
		/// <typeparam name="T">Класс сущности</typeparam>
		protected List<T> GetInsertedObjects<T>() where T : Metadata
		{
			return GetObjects<T>(insertedTblName);
		}

		/// <summary>
		/// Возвращает список объектов, соотвутствующих строкам из таблицы DELETED
		/// </summary>
		/// <typeparam name="T">Класс сущности</typeparam>
		protected List<T> GetDeletedObjects<T>() where T : Metadata
		{
			return GetObjects<T>(deletedTblName);
		}

		/// <summary>
		/// Сравнивает две строки и возвращает словарь ИмяПоля=Значение для измененных полей.
		/// </summary>
		/// <param name="before">Строка до срабатывания триггера</param>
		/// <param name="after">Строка после срабатывания триггера, содержащая обновленные значения</param>
		/// <returns>Словарь измененных полей</returns>
		protected Dictionary<string, object> GetUpdatedFields(DataRow before, DataRow after)
		{
			var result = new Dictionary<string, object>();
			for (int columnNumber = 0; columnNumber < triggerContext.ColumnCount; columnNumber++)
			{
				if (triggerContext.IsUpdatedColumn(columnNumber) && (
					before[columnNumber] != after[columnNumber]
					|| (before[columnNumber] == null && after[columnNumber] == null)
					))
				{
					result.Add(inserted.Columns[columnNumber].ColumnName, after[columnNumber]);
				}
			}
			return result;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Возвращает объекты сущности (вставленные или удаленные) при срабатывании триггера
		/// </summary>
		/// <returns>Объекты сущности</returns>
		private List<T> GetObjects<T>(string from) where T : Metadata
		{
			var result = new List<T>();
			var table = GetDataRowsLazy(from);
			foreach (DataRow row in table.Rows)
			{
				T obj = Objects.Create<T>();
				obj.FromDataRow(row);
				result.Add(obj);
			}
			return result;
		}

		/// <summary>
		/// Имя служебной таблицы
		/// </summary>
		private const string insertedTblName = "INSERTED";

		/// <summary>
		/// Имя служебной таблицы
		/// </summary>
		private const string deletedTblName = "DELETED";

		/// <summary>
		/// Закешированные строки из таблиц INSERTED и DELETED.
		/// Словарь из двух элементов с ключами <see cref="insertedTblName"/> и <see cref="deletedTblName"/>.
		/// </summary>
		private Dictionary<string, DataTable> table = new Dictionary<string, DataTable>();
		
		/// <summary>
		/// Ленивый механизм чтения строк из таблицы <paramref name="from"/> (INSERTED или DELETED).
		/// Если значение существует в кеше, обращения к таблице не происходит.
		/// </summary>
		/// <param name="from">Возможны 2 варианта: <see cref="insertedTblName"/> и <see cref="deletedTblName"/></param>
		/// <returns>Строки из служебной таблицы</returns>
		private DataTable GetDataRowsLazy(string from)
		{
			return table.ContainsKey(from) ? table[from] : (table[from] = GetDataRows(from));
		}

		/// <summary>
		/// Считывает строки из служебной таблицы <paramref name="from"/> (INSERTED или DELETED)
		/// </summary>
		/// <param name="from">Возможны 2 варианта: <see cref="insertedTblName"/> и <see cref="deletedTblName"/></param>
		/// <returns>Строки из служебной таблицы</returns>
		private DataTable GetDataRows(string from)
		{
			var table = new DataTable();
			var adapter = new SqlDataAdapter(string.Format("SELECT * FROM {0}", from), SqlCmd.ContextConnection);
			adapter.Fill(table);
			return table;
		}

		#endregion
	}
}
