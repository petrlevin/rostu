using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;
using System.Data.Common;
using Platform.BusinessLogic.EntityFramework;

using BaseApp.Reference;using BaseApp.Reference.Mappings;using BaseApp.Registry;using BaseApp.Registry.Mappings;using BaseApp.Tablepart;using BaseApp.Tablepart.Mappings;
namespace BaseApp
{
	/// <summary>
	/// Дата-контекст
	/// </summary>
	public partial class DataContext : Platform.BusinessLogic.DataContext	{
		static DataContext()
		{
			Database.SetInitializer<DataContext>(null);
			OnStaticConstruct();
		}
		
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public DataContext()
			: this("PlatformDBConnectionString")
		{
			((IObjectContextAdapter) this).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;
		}

		/// <summary>
		/// Конструктор, с возможностью указания строки соединения
		/// </summary>
		public DataContext([Dependency("ConnectionString")] string connectionString)
			: base(DbContextInitializer.CreateConnection(connectionString), true)
		{
				DbContextInitializer.EnableTracing(this);

		}
		
		/// <summary>
		/// Конструктор, с возможностью указания существующего соединения
		/// </summary>
		public DataContext(DbConnection dbConnection, bool contextOwnsConnection) :
			base(dbConnection, contextOwnsConnection)
		{
			
		}

		/// <summary>
		/// Пользователи
		/// </summary>
		public DbSet<BaseApp.Reference.User> User { get; set; }
		/// <summary>
		/// Роли
		/// </summary>
		public DbSet<BaseApp.Reference.Role> Role { get; set; }
		/// <summary>
		/// Выполненные операции
		/// </summary>
		public DbSet<BaseApp.Registry.ExecutedOperation> ExecutedOperation { get; set; }
		/// <summary>
		/// Ответственные лица
		/// </summary>
		public DbSet<BaseApp.Reference.ResponsiblePerson> ResponsiblePerson { get; set; }
		/// <summary>
		/// Организации
		/// </summary>
		public DbSet<BaseApp.Reference.Organization> Organization { get; set; }
		/// <summary>
		/// Публично-правовые образования
		/// </summary>
		public DbSet<BaseApp.Reference.PublicLegalFormation> PublicLegalFormation { get; set; }
		/// <summary>
		/// ОКАТО
		/// </summary>
		public DbSet<BaseApp.Reference.OKATO> OKATO { get; set; }
		/// <summary>
		/// Должности
		/// </summary>
		public DbSet<BaseApp.Reference.OfficialCapacity> OfficialCapacity { get; set; }
		/// <summary>
		/// Уровни бюджета
		/// </summary>
		public DbSet<BaseApp.Reference.BudgetLevel> BudgetLevel { get; set; }
		/// <summary>
		/// Модули ППО
		/// </summary>
		public DbSet<BaseApp.Reference.PublicLegalFormationModule> PublicLegalFormationModule { get; set; }
		/// <summary>
		/// Бюджеты
		/// </summary>
		public DbSet<BaseApp.Reference.Budget> Budget { get; set; }
		/// <summary>
		/// ТЧ Функциональные права
		/// </summary>
		public DbSet<BaseApp.Tablepart.Role_FunctionalRight> Role_FunctionalRight { get; set; }
		/// <summary>
		/// ТЧ Операции документов
		/// </summary>
		public DbSet<BaseApp.Tablepart.Role_DocumentOperation> Role_DocumentOperation { get; set; }
		/// <summary>
		/// ТЧ Статусы справочников
		/// </summary>
		public DbSet<BaseApp.Tablepart.Role_RefStatus> Role_RefStatus { get; set; }
		/// <summary>
		/// ТЧ Организационные права
		/// </summary>
		public DbSet<BaseApp.Tablepart.Role_OrganizationRight> Role_OrganizationRight { get; set; }
		/// <summary>
		/// Оперативные отчеты
		/// </summary>
		public DbSet<BaseApp.Reference.TableReport> TableReport { get; set; }
		/// <summary>
		/// Начатые операции
		/// </summary>
		public DbSet<BaseApp.Registry.StartedOperation> StartedOperation { get; set; }
		/// <summary>
		/// Расширение организационных прав
		/// </summary>
		public DbSet<BaseApp.Reference.OrganizationRightExtension> OrganizationRightExtension { get; set; }
		/// <summary>
		/// Версии
		/// </summary>
		public DbSet<BaseApp.Reference.Version> Version { get; set; }
		/// <summary>
		/// Перечень выводимых полей
		/// </summary>
		public DbSet<BaseApp.Reference.ListRemovedFields> ListRemovedFields { get; set; }
		/// <summary>
		/// Модули
		/// </summary>
		public DbSet<BaseApp.Reference.Module> Module { get; set; }
		/// <summary>
		/// Типы колонок
		/// </summary>
		public DbSet<BaseApp.Tablepart.TableReport_ColumnType> TableReport_ColumnType { get; set; }
		/// <summary>
		/// Настройки проекта
		/// </summary>
		public DbSet<BaseApp.Reference.ProjectSettings> ProjectSettings { get; set; }
		
		/// <summary>
		/// Получение типизированного набора сущностей
		/// </summary>
		public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
		{
			return base.Set<TEntity>();
		}

		/// <summary>
		/// Событие при создании моделей
		/// </summary>
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new UserMap());
			modelBuilder.Configurations.Add(new RoleMap());
			modelBuilder.Configurations.Add(new ExecutedOperationMap());
			modelBuilder.Configurations.Add(new ResponsiblePersonMap());
			modelBuilder.Configurations.Add(new OrganizationMap());
			modelBuilder.Configurations.Add(new PublicLegalFormationMap());
			modelBuilder.Configurations.Add(new OKATOMap());
			modelBuilder.Configurations.Add(new OfficialCapacityMap());
			modelBuilder.Configurations.Add(new BudgetLevelMap());
			modelBuilder.Configurations.Add(new PublicLegalFormationModuleMap());
			modelBuilder.Configurations.Add(new BudgetMap());
			modelBuilder.Configurations.Add(new Role_FunctionalRightMap());
			modelBuilder.Configurations.Add(new Role_DocumentOperationMap());
			modelBuilder.Configurations.Add(new Role_RefStatusMap());
			modelBuilder.Configurations.Add(new Role_OrganizationRightMap());
			modelBuilder.Configurations.Add(new TableReportMap());
			modelBuilder.Configurations.Add(new StartedOperationMap());
			modelBuilder.Configurations.Add(new OrganizationRightExtensionMap());
			modelBuilder.Configurations.Add(new VersionMap());
			modelBuilder.Configurations.Add(new ListRemovedFieldsMap());
			modelBuilder.Configurations.Add(new ModuleMap());
			modelBuilder.Configurations.Add(new TableReport_ColumnTypeMap());
			modelBuilder.Configurations.Add(new ProjectSettingsMap());
			 
			CustomOnModelCreating(modelBuilder);
			base.OnModelCreating(modelBuilder);
		}

		partial void CustomOnModelCreating(DbModelBuilder modelBuilder);
		static partial void OnStaticConstruct();
	}
}