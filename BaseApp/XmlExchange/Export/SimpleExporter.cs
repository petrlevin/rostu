using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BaseApp.Numerators;
using BaseApp.Reference;
using BaseApp.Rights;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.Common;
using Platform.Dal.QueryBuilders;
using Platform.DbCmd;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.XmlExchange.Export
{
	public class SimpleExporter
	{
		private int _idEntity;

		private DataContext _dataContext;

		private static SqlConnection _connection;

		public SimpleExporter(int idEntity, SqlConnection connection = null, DbContext dbContext = null)
		{
			_idEntity = idEntity;
			_dataContext = (dbContext ?? IoC.Resolve<DbContext>()).Cast<DataContext>();
			_connection = connection ?? IoC.Resolve<SqlConnection>("DbConnection");
		}

		public string Execute()
		{
			XDocument document = new XDocument(new XElement("root")) {Declaration = new XDeclaration("1.0", "utf-8", "yes")};

			Entity entity = Objects.ById<Entity>(_idEntity);
			document.Root.Add(_getXml(entity));
			return document.ToString(SaveOptions.DisableFormatting);
		}

		private List<XElement> _getXml(IEntity entity, string filterFieldName=null, IEnumerable<int> listIds=null, IEnumerable<int> excludedLinkedFieldIds=null)
		{
			string elementName = string.Format("{0}.{1}", entity.Schema, entity.Name);
			List<string> sysDimensionName = new List<string>
				{
					"idPublicLegalFormation",
					"idVersion",
					"idBudget"
				};
			List<string> baseEntityName = new List<string>
				{
					"Entity",
					"EntityField",
				};

			List<EntityFieldType> generikLinks=new List<EntityFieldType>
				{
					EntityFieldType.ReferenceEntity,
					EntityFieldType.DocumentEntity,
					EntityFieldType.TablepartEntity,
					EntityFieldType.ToolEntity
				};

			List<string> listFields =
				entity.RealFields.Where(a => !a.IdCalculatedFieldType.HasValue && !sysDimensionName.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).Select(a => a.Name).ToList();
			if (entity.Name.Equals("user", StringComparison.OrdinalIgnoreCase))
			{
				listFields = listFields.Where(a => !a.Equals("password", StringComparison.OrdinalIgnoreCase) && !a.Equals("ChangePasswordNextTime", StringComparison.OrdinalIgnoreCase)).ToList();
			}
			QueryBuilder query = new SelectQueryBuilder(entity, listFields);
			AddSysDimensionsFilter addSysDimensionsFilter=new AddSysDimensionsFilter(null);
			SelectStatement selectStatement = (SelectStatement) query.GetTSqlStatement();
			if (entity.Name.Equals("user", StringComparison.OrdinalIgnoreCase))
			{
				selectStatement =
					selectStatement.AddFields(new List<SelectColumn> { Helper.CreateColumn("12345".ToLiteral(), "Password"), Helper.CreateColumn(1.ToLiteral(), "ChangePasswordNextTime") });
			}
			selectStatement = (SelectStatement) addSysDimensionsFilter.Decorate(selectStatement, query);
			if (listIds != null)
			{
				selectStatement = selectStatement.Where(BinaryExpressionType.And,
														Helper.CreateInPredicate(("a." + filterFieldName).ToColumn(), listIds));
			}
			if (entity.EntityType==EntityType.Document)
			{
				IEntityField parentField = entity.Fields.FirstOrDefault(a => a.IdEntityLink.HasValue && a.IdEntityLink == a.IdEntity);
				if (parentField!=null)
				{
					Expression joinExpression = Helper.CreateBinaryExpression(("b." + parentField.Name).ToColumn(), "a.id".ToColumn(),
					                                                          BinaryExpressionType.Equals);
					selectStatement = selectStatement.Join(QualifiedJoinType.LeftOuter,
					                                       Helper.CreateSchemaObjectTableSource(entity.Schema, entity.Name, "b"),
					                                       joinExpression);
					selectStatement = selectStatement.Where(BinaryExpressionType.And, Helper.CreateCheckFieldIsNull("a", "id"));
				}
			}
			selectStatement.AddForClause(elementName);
			SelectStatement selectStatement2 = new SelectStatement();
			QuerySpecification querySpecification=new QuerySpecification();
			querySpecification.SelectElements.Add(selectStatement.ToSubquery());
			selectStatement2.QueryExpression = querySpecification;
			selectStatement2.AddForClause("root");
			SqlCmd sqlCmd = new SqlCmd(_connection);
			string tt = sqlCmd.ExecuteScalar<string>(selectStatement2.Render());
			XDocument document = XDocument.Parse(tt);
			List<IEntityField> linkedFields =
				entity.RealFields.Where(
					a =>
					a.EntityFieldType == EntityFieldType.Link && a.IdEntityLink.HasValue && a.EntityLink.EntityType != EntityType.Enum &&
					listFields.Contains(a.Name, StringComparer.OrdinalIgnoreCase) &&
					!baseEntityName.Contains(a.EntityLink.Name, StringComparer.OrdinalIgnoreCase)).ToList();
			if (excludedLinkedFieldIds != null)
				linkedFields = linkedFields.Where(a => !excludedLinkedFieldIds.Contains(a.Id)).ToList();
			foreach (IEntityField entityField in linkedFields)
			{
				List<int> newListIds =
					document.Root.Elements(elementName).Descendants(entityField.Name).Where(a => !string.IsNullOrEmpty(a.Value)).Select
						(
							a => int.Parse(a.Value)).ToList();
				if (newListIds.Count>0)
				{
					document.Root.AddFirst(_getXml(entityField.EntityLink, "id", newListIds));
				}
			}
			List<IEntityField> genericLinksFields = entity.Fields.Where(a => generikLinks.Contains(a.EntityFieldType)).ToList();
			foreach (IEntityField genericLinksField in genericLinksFields)
			{
				foreach (var genericElements in document.Root.Elements(elementName).Elements())
				{
					XElement ttt =
						genericElements.Elements().SingleOrDefault(
							a => a.Name.LocalName.Equals(genericLinksField.Name) && !string.IsNullOrEmpty(a.Value));
				}
				
				List<int> newListIds =
					document.Root.Elements(elementName).Descendants(genericLinksField.Name).Where(a => !string.IsNullOrEmpty(a.Value)).Select
						(
							a => int.Parse(a.Value)).ToList();
				foreach (int newListId in newListIds)
				{
					
				}
			}

			List<int> idsOwner =
				document.Root.Elements(elementName).Descendants("id").Where(a => !string.IsNullOrEmpty(a.Value)).Select
					(a => int.Parse(a.Value)).ToList();
			if (entity.Fields.Any(a => a.EntityFieldType == EntityFieldType.Tablepart))
			{
				document.Root.Add(_getXmlForTableParts(entity, idsOwner, new List<int>()));
			}
			if (entity.Fields.Any(a => a.EntityFieldType == EntityFieldType.Multilink && !(excludedLinkedFieldIds ?? new List<int>()).Contains(a.Id)))
			{
				document.Root.Add(_getXmlForMultilink(entity, idsOwner));
			}
			return document.Root.Elements().ToList();
		}

		private List<XElement>  _getXmlForMultilink(IEntity entity, List<int> idsOwner)
		{
			List<IEntityField> listFieldsMl = entity.Fields.Where(a => a.EntityFieldType == EntityFieldType.Multilink).ToList();
			XDocument document = new XDocument(new XElement("root"));
			foreach (IEntityField entityField in listFieldsMl)
			{
				IEntityField filterField =
					entityField.EntityLink.Fields.Single(a => a.IdEntityLink.HasValue && a.IdEntityLink.Value == entity.Id);

				IEntityField rightField = entityField.EntityLink.Fields.Single(a => a.IdEntityLink.HasValue && a.IdEntityLink.Value != entity.Id);

				document.Root.AddFirst(_getXml(entityField.EntityLink, filterField.Name, idsOwner, new List<int> { filterField.Id, rightField.Id }));
				
				List<int> filterIds = document.Root.Elements(string.Format("{0}.{1}", entityField.EntityLink.Schema, entityField.EntityLink.Name)).Descendants(rightField.Name).Where(a => !string.IsNullOrEmpty(a.Value)).Select(a => int.Parse(a.Value)).ToList();
				IEntityField rightMlField =
					rightField.EntityLink.Fields.SingleOrDefault(
						a => a.EntityFieldType == EntityFieldType.Multilink && a.IdEntityLink.HasValue && a.IdEntityLink.Value == entityField.IdEntityLink);

				document.Root.AddFirst(_getXml(rightField.EntityLink, "id", filterIds, rightMlField==null ? null : new List<int> { rightMlField.Id }));
			}
			return document.Root.Elements().ToList();
		}
		
		private List<XElement> _getXmlForTableParts(IEntity entity, List<int> idsOwner, List<int> excludedIdEntity)
		{
			List<int> idsEntityLink =
				entity.Fields.Where(a => a.EntityFieldType == EntityFieldType.Tablepart && a.IdEntityLink.HasValue && !excludedIdEntity.Contains(a.IdEntityLink.Value)).Select(
					a => a.IdEntityLink.Value).ToList();
			if (idsEntityLink.Count == 0)
				return new XDocument(new XElement("root")).Root.Elements().ToList();

			List<IEntityField> listFieldsTp = entity.Fields.
				Where(a => a.EntityFieldType == EntityFieldType.Tablepart
						&& a.IdEntityLink.HasValue
						   && !a.EntityLink.Fields.Any(b => b.IdEntityLink.HasValue
				                                            && b.EntityFieldType == EntityFieldType.Link
				                                            &&
				                                            idsEntityLink.Where(c => c != a.IdEntityLink).Contains(b.IdEntityLink.Value)))
				.ToList();

			List<int> newExcludedEntity = new List<int>();
			XDocument document=new XDocument(new XElement("root"));
			foreach (IEntityField entityField in listFieldsTp)
			{
				IEntityField filterField =
					entityField.EntityLink.Fields.Single(a => a.IdEntityLink.HasValue && a.IdEntityLink.Value == entity.Id);
				IEnumerable<int> excludedLinkedFieldIds =
					entityField.EntityLink.Fields.Where(
						a =>
						a.IdEntityLink.HasValue && (a.IdEntityLink.Value == entity.Id || a.EntityLink.EntityType == EntityType.Tablepart))
						.
						Select(a => a.Id);
				document.Root.Add(_getXml(entityField.EntityLink, filterField.Name, idsOwner, excludedLinkedFieldIds));
				newExcludedEntity.Add(entityField.IdEntityLink.Value);

			}
			if (newExcludedEntity.Count>0)
			{
				document.Root.Add(_getXmlForTableParts(entity, idsOwner, newExcludedEntity));
			}

			return document.Root.Elements().ToList();
		}

		
	}
}