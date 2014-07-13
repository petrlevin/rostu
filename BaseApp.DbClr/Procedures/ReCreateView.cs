using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using Platform.DbClr;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.DbClr.Procedures
{
	/// <summary>
	/// Созадние или пересоздание представления (view) для сущности или формы
	/// </summary>
	public class ReCreateView
	{
		/// <summary>
		/// Удаление существующего представления и создание нового
		/// </summary>
		/// <param name="idEntity">Идентификатор сущности</param>
		[SqlProcedure]
		public static void Exec(int idEntity)
		{
			Entity entity = Objects.ById<Entity>(idEntity);
			Helper.ExecCommand(DropView(entity));
			Helper.ExecCommand(CreateView(entity));
		}

		/// <summary>
		/// Возвращает SQL команду для создания представления(view)
		/// </summary>
		/// <param name="entity">Сущность</param>
		private static string CreateView(Entity entity)
		{
			List<IEntityField> entityFields = entity.Fields.Where(
				a =>
				(a.EntityFieldType != EntityFieldType.Multilink &&
				a.EntityFieldType != EntityFieldType.Tablepart &&
				a.EntityFieldType != EntityFieldType.DataEndpoint &&
				a.EntityFieldType != EntityFieldType.VirtualTablePart) &&
				(!a.IdCalculatedFieldType.HasValue || (
				 a.CalculatedFieldType != CalculatedFieldType.ClientComputed &&
				 a.CalculatedFieldType != CalculatedFieldType.AppComputed && 
                 a.CalculatedFieldType != CalculatedFieldType.NumeratorExpression &&
                 a.CalculatedFieldType != CalculatedFieldType.DbComputedFunction))
				).ToList();
			StringBuilder textCommand = new StringBuilder("CREATE VIEW ");
			textCommand.AppendFormat("[gen].[{0}] AS SELECT ", entity.Name);
			char alias = 'a';
			StringBuilder tables = new StringBuilder("FROM ");
			tables.AppendFormat("[{0}].[{1}] AS [{2}] ", new object[] { entity.Schema, entity.Name, alias });
			StringBuilder fields = new StringBuilder();
			bool first = true;
			foreach (EntityField field in entityFields)
			{

			    if (!first)
				{
					fields.Append(",");
				}
				else
				{
					first = false;
				}
                if (field.IdCalculatedFieldType.HasValue && field.CalculatedFieldType == CalculatedFieldType.Joined)
                {
                    alias++;
                    tables.Append(GetJoinToJoined(field, alias, 0));
                    fields.AppendFormat("{0} AS [{1}]", GetFieldToJoined(field, alias), field.Name + "_Caption");
                    continue;
                }
				switch (field.EntityFieldType)
				{
					case EntityFieldType.Link:
                    case EntityFieldType.FileLink:
						{
							if (field.IdEntityLink.HasValue)
							{
								alias++;
								tables.Append(GetJoinToLink(field, alias, 0));
								fields.AppendFormat("[a].[{0}],", field.Name);
								fields.AppendFormat("{0} AS [{1}]", GetFieldToLink(field, alias, 0),
													field.Name + "_Caption");
							}
							break;
						}
					case EntityFieldType.ReferenceEntity:
                    case EntityFieldType.DocumentEntity:
                    case EntityFieldType.TablepartEntity:
                    case EntityFieldType.ToolEntity:
				        {
                            fields.AppendFormat("(dbo.GetCaption ([a].[{0}],[a].[{1}])) AS {2}", field.Name + "Entity", field.Name, field.Name + "_Caption");
                            break;
				        }
				    default:
						fields.AppendFormat("[a].[{0}]", field.Name);
						break;
				}
			}
			return textCommand.AppendFormat("{0} {1}", fields.ToString(), tables.ToString()).ToString();
		}

	    /// <summary>
		/// Возвращает SQL команду для удаления представления(view)
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		private static string DropView(Entity entity)
		{
			return
				String.Format(
					"IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[gen].[{0}]')) DROP VIEW [gen].[{0}];", entity.Name);
		}


		/// <summary>
		/// Строит цепочку из джоинов для того чтобы получить реальное поле, содержащее наименование
		/// </summary>
		/// <param name="entityField">Поле сущности (обязательно с типом Link)</param>
		/// <param name="alias">Алиас таблицы</param>
		/// <param name="numAlias">Номер, добавляемый к алиасу таблицы</param>
		/// <returns>string</returns>
		private static string GetJoinToLink(EntityField entityField, char alias, int numAlias)
		{
            if (!entityField.IsFieldWithForeignKey() )
				throw new Exception("GetJoinToLink: обрабатывется только поле с типом Link");
			if (!entityField.IdEntityLink.HasValue)
				throw new Exception(
					String.Format("GetJoinToLink: у поля [{0}] сущности [{1}] не заполнен столбец IdEntityLink",
								  entityField.Name, entityField.Entity.Name));
			StringBuilder result = new StringBuilder();
			Entity linkEntity = Objects.ById<Entity>(entityField.IdEntityLink.Value);

			result.AppendFormat("{0} JOIN [{1}].[{2}] AS [{3}] ON [{5}].[{4}]=[{3}].[id] ",
								new object[]
									{
										entityField.AllowNull ? "LEFT OUTER" : "INNER",
										linkEntity.Schema, linkEntity.Name,
										alias.ToString() + numAlias, entityField.Name,
										numAlias==0 ? "a" : alias.ToString()+(numAlias-1).ToString()
									});
			EntityField captionField = (EntityField) linkEntity.CaptionField;
			if (captionField.EntityFieldType == EntityFieldType.Link  )
			{
				int newNumAlias = ++numAlias;
				result.Append(GetJoinToLink(captionField, alias, newNumAlias));
			}
			return result.ToString();
		}

		/// <summary>
		/// Возвращает поле которое содержит наименование
		/// </summary>
		/// <param name="entityField">Поле сущности (обязательно с типом Link)</param>
		/// <param name="alias">Алиас таблицы</param>
		/// <param name="numAlias">Номер, добавляемый к алиасу таблицы</param>
		/// <returns>string</returns>
		private static string GetFieldToLink(EntityField entityField, char alias, int numAlias)
		{
            if ( !entityField.IsFieldWithForeignKey() )
				throw new Exception("GetJoinToLink: обрабатывется только поле с типом Link");
			if (!entityField.IdEntityLink.HasValue)
				throw new Exception(
					String.Format("GetJoinToLink: у поля [{0}] сущности [{1}] не заполнен столбец IdEntityLink",
								  entityField.Name, entityField.Entity.Name));
			Entity linkEntity = Objects.ById<Entity>(entityField.IdEntityLink.Value);
			string result = "";

			EntityField captionField = (EntityField) linkEntity.CaptionField;
			if (captionField.EntityFieldType == EntityFieldType.Link)
			{
				int newNumAlias = numAlias + 1;
				result = GetFieldToLink(captionField, alias, newNumAlias);
			}
			else
			{
				result = String.Format("[{0}].[{1}]", alias.ToString() + numAlias, captionField.Name);
			}
			return result;
		}

	    private static string GetFieldToJoined(EntityField entityField, char alias)
	    {
	        var joinedFields = entityField.Expression.Split('.');

            var newAlias = alias.ToString() + (joinedFields.Count() - 2).ToString();

            return string.Format("[{0}].[{1}]", newAlias, joinedFields.Last());
	    }

        private static string GetJoinForDbFunction(EntityField entityField, char alias)
        {
            return string.Format("LEFT OUTER JOIN {0} as {1} ON [a].[id]={1}.[id] ", entityField.Expression, alias);
        }

        private static string GetJoinToJoined(EntityField entityField, char alias, int numAlias)
	    {
            StringBuilder result = new StringBuilder();

            string[] joinedFields = entityField.Expression.Split(new char[] { '.' });
            Entity leftEntity = entityField.Entity;
            IEntityField leftField = leftEntity.Fields.SingleOrDefault(a => a.Name.Equals(joinedFields[0], StringComparison.OrdinalIgnoreCase));

            IEntity entity = leftField.EntityLink;
            string qualifiedJoinType = entityField.AllowNull ? "LEFT OUTER" : "INNER";
            string leftTableAlias = alias.ToString();
            string rightTableAlias = alias.ToString() + numAlias;
            joinedFields = joinedFields.Skip(1).ToArray();

            foreach (string field in joinedFields)
            {
                result.AppendFormat("{0} JOIN [{1}].[{2}] AS [{3}] ON [{5}].[{4}]=[{3}].[id] ",
										qualifiedJoinType,
										entity.Schema, 
                                        entity.Name,
										rightTableAlias, 
                                        leftField.Name,
										numAlias==0 ? "a" : leftTableAlias
									);

                leftField = entity.Fields.Single(a => a.Name.Equals(field, StringComparison.OrdinalIgnoreCase));
                if (leftField.IdEntityLink == null)
                    break;
                numAlias++;
                qualifiedJoinType = leftField.AllowNull ? "LEFT OUTER" : "INNER";
                leftTableAlias = rightTableAlias;
                rightTableAlias = alias.ToString() + numAlias;
                entity = Objects.ById<Entity>(leftField.IdEntityLink.Value);
            }


            return result.ToString();
	    }

	}
}
