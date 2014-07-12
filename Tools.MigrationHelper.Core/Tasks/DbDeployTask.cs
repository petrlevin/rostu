using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.DbEnums;
using Tools.MigrationHelper.Helpers;


namespace Tools.MigrationHelper.Core.Tasks
{
	public abstract class DbDeployTask : DeployBase
	{
		[TaskAttribute("connectionstring", Required = true)]
		public string ConnectionString { get; set; }

		protected override void Initialize()
		{
			if (!RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully)
				throw new BuildException(
					"Для корректной работы необходимо изменить Nant.exe.config \" <startup useLegacyV2RuntimeActivationPolicy=\"true\">\" ");
			base.Initialize();
		}




		/// <summary>
		/// Изменение стартового значение IDENTITY для таблиц
		/// </summary>
		protected void SetStartIdentity()
		{
			if (!IsDeveloper())
				return;
			Log(Level.Verbose, "Изменение стартового значение IDENTITY для таблиц...");
			try
			{
				int no = 32 - (DevId - 1);
				const int step = int.MaxValue/32*-1;

				using (SqlConnection connection = new SqlConnection(ConnectionString))
				{
					connection.Open();
					List<string> tablesName = new List<string>();
					using (
						SqlCommand command =
							new SqlCommand(
								"SELECT [a].[Name], [a].[idEntityType] FROM [ref].[Entity] AS [a] WHERE [idEntityType]<>1 AND exists(select 1 from ref.EntityField where [idEntity]=a.id and Name='id')",
								connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								tablesName.Add(string.Format("{0}.{1}", Schemas.ByEntityType((EntityType) reader.GetByte(1)),
								                             reader.GetString(0)));
							}
						}
					}
					foreach (string tableName in tablesName)
					{
						StringBuilder textCommand = new StringBuilder();
						textCommand.AppendLine("declare @newMax int;");
						textCommand.AppendFormat(
							"select @newMax=CASE WHEN ISNULL(MAX(id)+1,({0}*{1}))<=({0}*{1}) THEN ({0}*{1}) ELSE ISNULL(MAX(id)+1,({0}*{1})) END from [{2}] where id<(({0}-1)*{1});",
							no, step, tableName.Replace(".", "].["));
						textCommand.AppendLine();
						textCommand.AppendFormat("DBCC CHECKIDENT ('{0}', RESEED, @newMax);", tableName);
						using (SqlCommand command = new SqlCommand(textCommand.ToString(), connection))
						{
							command.ExecuteNonQuery();
						}
					}
					connection.Close();
				}
			}
			catch (Exception ex)
			{
				Fatal("Фатальная ошибка при изменении стартового значение IDENTITY для таблиц", ex);
			}
		}
	}
}
