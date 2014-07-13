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
	/// ������� ����� ��� DML-��������� (DML - Data Manipulation Language)
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
					throw new ArgumentOutOfRangeException("���������������� �������� ��������� SqlContext.TriggerContext.TriggerAction");
			}
		}
		#endregion

		#region Protected Methods

		/// <summary>
		/// ����� �������� ����� ��������� ��������� �������� (������ SqlContext.TriggerContext).
		/// </summary>
		protected SqlTriggerContext triggerContext
		{
			get
			{
				return SqlContext.TriggerContext;
			}
		}

		/// <summary>
		/// ������ �� ��������� ������� INSERTED
		/// </summary>
		protected DataTable inserted
		{
			get { return GetDataRowsLazy(insertedTblName); }
		}
		
		/// <summary>
		/// ������ �� ��������� ������� DELETED
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
		/// ���������� ������ ��������, ��������������� ������� �� ������� INSERTED
		/// </summary>
		/// <typeparam name="T">����� ��������</typeparam>
		protected List<T> GetInsertedObjects<T>() where T : Metadata
		{
			return GetObjects<T>(insertedTblName);
		}

		/// <summary>
		/// ���������� ������ ��������, ��������������� ������� �� ������� DELETED
		/// </summary>
		/// <typeparam name="T">����� ��������</typeparam>
		protected List<T> GetDeletedObjects<T>() where T : Metadata
		{
			return GetObjects<T>(deletedTblName);
		}

		/// <summary>
		/// ���������� ��� ������ � ���������� ������� �������=�������� ��� ���������� �����.
		/// </summary>
		/// <param name="before">������ �� ������������ ��������</param>
		/// <param name="after">������ ����� ������������ ��������, ���������� ����������� ��������</param>
		/// <returns>������� ���������� �����</returns>
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
		/// ���������� ������� �������� (����������� ��� ���������) ��� ������������ ��������
		/// </summary>
		/// <returns>������� ��������</returns>
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
		/// ��� ��������� �������
		/// </summary>
		private const string insertedTblName = "INSERTED";

		/// <summary>
		/// ��� ��������� �������
		/// </summary>
		private const string deletedTblName = "DELETED";

		/// <summary>
		/// �������������� ������ �� ������ INSERTED � DELETED.
		/// ������� �� ���� ��������� � ������� <see cref="insertedTblName"/> � <see cref="deletedTblName"/>.
		/// </summary>
		private Dictionary<string, DataTable> table = new Dictionary<string, DataTable>();
		
		/// <summary>
		/// ������� �������� ������ ����� �� ������� <paramref name="from"/> (INSERTED ��� DELETED).
		/// ���� �������� ���������� � ����, ��������� � ������� �� ����������.
		/// </summary>
		/// <param name="from">�������� 2 ��������: <see cref="insertedTblName"/> � <see cref="deletedTblName"/></param>
		/// <returns>������ �� ��������� �������</returns>
		private DataTable GetDataRowsLazy(string from)
		{
			return table.ContainsKey(from) ? table[from] : (table[from] = GetDataRows(from));
		}

		/// <summary>
		/// ��������� ������ �� ��������� ������� <paramref name="from"/> (INSERTED ��� DELETED)
		/// </summary>
		/// <param name="from">�������� 2 ��������: <see cref="insertedTblName"/> � <see cref="deletedTblName"/></param>
		/// <returns>������ �� ��������� �������</returns>
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
