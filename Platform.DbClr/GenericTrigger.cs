using System.Data;
using Platform.DbClr.Interfaces;
using Platform.PrimaryEntities;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;

namespace Platform.DbClr
{
	public class GenericTrigger<TEntity, TTrigger> : DmlTriggerBase
		where TEntity : Metadata
		where TTrigger : ITriggerAction<TEntity>, new ()
    {
		protected override void Insert()
		{
			List<TEntity> metaList = this.GetInsertedObjects<TEntity>();
			if (metaList.Count == 0) return;
			TTrigger trigger=new TTrigger();
			trigger.ExecInsertCmd(metaList);
		}

		protected override void Update()
		{
			List<TEntity> insert = this.GetInsertedObjects<TEntity>();
			List<TEntity> delete = this.GetDeletedObjects<TEntity>();
			TTrigger trigger = new TTrigger();
			trigger.ExecUpdateCmd(insert, delete);
		}

		protected override void Delete()
		{
			List<TEntity> delete = this.GetDeletedObjects<TEntity>();
			TTrigger trigger = new TTrigger();
			trigger.ExecDeleteCmd(delete);
		}

		/// <summary>
		/// Находит строку в таблице по id
		/// </summary>
		/// <param name="table">Таблица</param>
		/// <param name="id">Значение поля id</param>
		/// <returns>Найденная строка</returns>
		private DataRow FindById(DataTable table, int id)
		{
			foreach(DataRow row in table.Rows)
			{
				if ((int)row["id"] == id)
					return row;
			}
			return null; // результат не найден
		}

		/// <summary>
		/// Выводит в консоль отладочную информацию.
		/// </summary>
		/// <param name="row"></param>
		/// <param name="updatedFields"></param>
		private void VerboseUpdate(DataRow row, Dictionary<string, object> updatedFields)
		{
			SqlContext.Pipe.Send(string.Format("Обновляем строку: {0}", row["id"]));
			foreach (var key in updatedFields.Keys)
			{
				SqlContext.Pipe.Send(string.Format("Поле: {0}; старое значение: {1}; новое значение: {2}", key, row[key], updatedFields[key]));
			}
		}
	}
}
