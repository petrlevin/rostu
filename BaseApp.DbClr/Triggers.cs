using BaseApp.DbClr.TriggerActions.Reference;
using Platform.DbClr;
using Platform.DbClr.Interfaces;

namespace BaseApp.DbClr
{
	/// <summary>
	/// Класс описывающий триггеры для таблиц
	/// </summary>
	public class Triggers
	{
		/// <summary>
		/// Триггер для таблицы [ref].[Entity]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[Entity]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void EntityIUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.Entity, EntityTrigger>();
			trigger.Process();
		}

		/// <summary>
		/// Триггер для таблицы [ref].[Entity]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[Entity]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void EntityLogicIUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.Entity, EntityLogicTrigger>();
			trigger.Process();
		}

		/// <summary>
		/// Триггер для таблицы [ref].[EntityField]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[EntityField]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void EntityFieldIUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.EntityField, EntityFieldTrigger>();
			trigger.Process();
		}

		/// <summary>
		/// Триггер для таблицы [ref].[EntityField]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[EntityField]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void EntityFieldLogicIUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.EntityField, EntityFieldLogicTrigger>();
			trigger.Process();
		}

		/// <summary>
		/// Триггер для таблицы [ref].[Index]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[Index]", Event = "FOR UPDATE, DELETE")]
		public static void IndexUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.Index, IndexTrigger>();
			trigger.Process();
		}

		/// <summary>
		/// Триггер для таблицы [ref].[FormElement]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[FormElement]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void ModelFieldIUD()
		{
		}

		/// <summary>
		/// Триггер для таблицы [ref].[HierarchicalLink]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[HierarchicalLink]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void HierarchicalLinkIUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.HierarchicalLink, HierarchicalLinkTrigger>();
			trigger.Process();
		}

		/// <summary>
		/// Триггер для таблицы [ref].[Programmability]
		/// </summary>
		[Microsoft.SqlServer.Server.SqlTrigger(Target = "[ref].[Programmability]", Event = "FOR INSERT, UPDATE, DELETE")]
		public static void ProgrammabilityIUD()
		{
			ITrigger trigger = new GenericTrigger<Platform.PrimaryEntities.Reference.Programmability, ProgrammabilityTrigger>();
			trigger.Process();
		}
	}
}
