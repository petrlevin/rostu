using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;
using Platforms.Tests.Common;

namespace Platform.Dal.Tests
{
	public class DemoTest : SqlTests
	{
		[SetUp]
		public void SetUp()
		{
			IUnityContainer uc = new UnityContainer();
			DependencyInjection.RegisterIn(uc, true, false, connectionString);
			IoC.InitWith(new DependencyResolverBase(uc));
		}


		/// <summary>
		/// Делался только для проверки, пока не удалять
		/// </summary>
		[Test]
		public void Test()
		{
			Entity entity = Objects.ById<Entity>(-1677721568);
			//var fields = entity.Fields.ToList();
			//fields.Add(new EntityField() { Name = "test", Expression = "idPublicLegalFormation.idAccessGroup.idStatus.Caption" });
			//entity.Fields = fields;
			ISelectQueryBuilder queryBuilder=new SelectQueryBuilder(entity);
			AddJoinedFields addJoinedFields = new AddJoinedFields();
			TSqlStatement statment = queryBuilder.GetTSqlStatement();
			statment = addJoinedFields.Decorate(statment, queryBuilder);
			var ttt = statment.Render();
		}

		[Test]
		public void Test2()
		{
			var ttt = Get(-2013265867, 1);
		}

		private string Get(int? idEntity, int? idItem)
		{
			if (!idEntity.HasValue || !idItem.HasValue)
				return "";

			string result;
			IEntity entity = Objects.ById<Entity>(idEntity.Value);
			IEntityField captionField = entity.CaptionField;
			StringBuilder textCommand = new StringBuilder("SELECT [{0}].[{1}] FROM ");
			if (captionField.EntityFieldType == EntityFieldType.String || captionField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand = new StringBuilder();
				textCommand.AppendFormat("SELECT {0} FROM [{1}].[{2}]", captionField.Name, entity.Schema, entity.Name);
			}
			else if (captionField.EntityFieldType == EntityFieldType.Link)
			{
				const char alias = 'a';
				textCommand.AppendFormat("[{0}].[{1}] [{2}] ", entity.Schema, entity.Name, alias);
				string joinType = captionField.AllowNull ? "LEFT OUTER JOIN" : "INNER JOIN";
				_reqursiveCaption(textCommand, entity, captionField, joinType, alias);
			}
			else
			{
				throw new Exception("Не реализовано для " + captionField.EntityFieldType);
			}
			result = ExecCommand(textCommand.ToString() + "WHERE [a].[id]=" + idItem.ToString());
			return result;
		}

		private string ExecCommand(string textCommand)
		{
			string result;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(textCommand, connection))
				{
					object res = command.ExecuteScalar();
					result = Convert.ToString(res);
				}
				connection.Close();
			}
			return result;
		}

		private void _reqursiveCaption(StringBuilder textCommand, IEntity entity, IEntityField captionField, string joinType, char alias)
		{
			char nextAlias = alias;
			nextAlias++;
			IEntity nextEntity = captionField.EntityLink;
			IEntityField nextCaptionField = nextEntity.CaptionField;
			textCommand.AppendFormat("{0} [{1}].[{2}] [{3}] ON [{4}].[{5}]=[{3}].[id] ", joinType, nextEntity.Schema,
									 nextEntity.Name, nextAlias, alias, captionField.Name);
			if (nextCaptionField.EntityFieldType == EntityFieldType.String || nextCaptionField.EntityFieldType == EntityFieldType.Text)
			{
				textCommand.Replace("{0}", nextAlias.ToString());
				textCommand.Replace("{1}", nextCaptionField.Name);
			}
			else if (nextCaptionField.EntityFieldType == EntityFieldType.Link)
			{
				string nextJoinType = joinType == "LEFT OUTER JOIN"
										  ? "LEFT OUTER JOIN"
										  : (nextCaptionField.AllowNull ? "LEFT OUTER JOIN" : "INNER JOIN");
				_reqursiveCaption(textCommand, nextEntity, nextCaptionField, nextJoinType, nextAlias);
			}
			else
			{
				throw new Exception("Не реализовано для " + nextCaptionField.EntityFieldType);
			}
		}

	}
}
