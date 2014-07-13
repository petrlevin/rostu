using System;
using System.Collections.Generic;
using System.Text;
using Platform.DbClr;
using Platform.DbClr.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Multilink;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.TriggerActions.Reference
{
	/// <summary>
	/// Триггеры для сущности HierarchicalLink
	/// </summary>
	public class HierarchicalLinkTrigger : ITriggerAction<HierarchicalLink>
	{
		/*
		/// <summary>
		/// Класс для формирования SQL команд
		/// </summary>
		private static class SqlTpl
		{
			/// <summary>
			/// Удаление элементов мультилинка HierarchicalLinkEntityFieldPathElements для указанного HierarchicalLink
			/// </summary>
			public const string ClearMultiLink = "DELETE FROM [ml].[HierarchicalLinkEntityFieldPathElements] WHERE [IdHierarchicalLink]={0}";

			private const string InsertIntoMultiLink =
				"INSERT INTO [ml].[HierarchicalLinkEntityFieldPathElements] ([idHierarchicalLink], [idEntityField], [Order]) VALUES ";

			/// <summary>
			/// Вставка значений в мультилинк HierarchicalLinkEntityFieldPathElements
			/// </summary>
			/// <param name="hierarchicalLink">Обрабатываемая иерархическая ссылка</param>
			/// <returns></returns>
			public static string InsertMultilink(HierarchicalLink hierarchicalLink)
			{
				StringBuilder result = new StringBuilder(InsertIntoMultiLink);
				bool first = true;
				foreach (HierarchicalLinkEntityFieldPathElements hierarchicalLinkEntityFieldPathElementse in hierarchicalLink.PathElements)
				{
					if (first)
						first = false;
					else
						result.Append(", ");
					result.AppendFormat("('{0}', '{1}', {2})",
										hierarchicalLinkEntityFieldPathElementse.IdHierarchicalLink,
										hierarchicalLinkEntityFieldPathElementse.IdEntityField,
										hierarchicalLinkEntityFieldPathElementse.OrdEntityField);
				}
				return result.ToString();
			}
		}

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
		public void ExecInsertCmd()
		{
			if (PathElements.Count == 0)
				throw new Exception("ExecInsertCmd: не заполнен массив PathElements");
			sqlCmd.ExecuteNonQuery(SqlTpl.ClearMultiLink, Id);
			sqlCmd.ExecuteNonQuery(SqlTpl.InsertMultilink(this));
		}

		/// <summary>
		/// Идентификатор элемента.
		/// </summary>
		/// <remarks>
		/// Сущности, участвующие в триггерах обязаны иметь идентификатор, чтобы сравнивать измененные поля.
		/// Напрямую поле может не использоваться, однако импользуется в <see cref="GenericTrigger{TEntity,TTrigger}.Update"/> 
		/// </remarks>
		public int Id { get; set; }

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
		public void ExecInsertCmd(List<Platform.PrimaryEntities.Reference.HierarchicalLink> inserted)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Релизация триггера UPDATE
		/// </summary>
		public void ExecUpdateCmd(Dictionary<string, object> updatedValues)
		{
			if (PathElements.Count == 0)
				throw new Exception("ExecUpdateCmd: не заполнен массив PathElements");
			sqlCmd.ExecuteNonQuery(SqlTpl.ClearMultiLink, Id);
			sqlCmd.ExecuteNonQuery(SqlTpl.InsertMultilink(this));
		}

		/// <summary>
		/// Релизация триггера DELETE
		/// </summary>
		public void ExecDeleteCmd()
		{
			if (PathElements.Count == 0)
				throw new Exception("ExecInsertCmd: не заполнен массив PathElements");
			sqlCmd.ExecuteNonQuery(SqlTpl.ClearMultiLink, Id);
		}

        public HierarchicalLink()
            : base()
        {

        }
		*/

		#region Implementation of ITriggerAction<HierarchicalLink>

		/// <summary>
		/// Релизация триггера INSERT
		/// </summary>
		public void ExecInsertCmd(List<Platform.PrimaryEntities.Reference.HierarchicalLink> inserted)
		{
			return;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Релизация триггера UPDATE
		/// </summary>
		public void ExecUpdateCmd(List<Platform.PrimaryEntities.Reference.HierarchicalLink> inserted, List<Platform.PrimaryEntities.Reference.HierarchicalLink> deleted)
		{
			return;
			throw new NotImplementedException();
		}

		/// <summary>
		/// Релизация триггера DELETE
		/// </summary>
		public void ExecDeleteCmd(List<Platform.PrimaryEntities.Reference.HierarchicalLink> deleted)
		{
			return;
			throw new NotImplementedException();
		}

		#endregion
	}
}
