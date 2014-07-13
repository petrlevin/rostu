using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;
using System.Data.Common;
using Platform.BusinessLogic.EntityFramework;

using Platform.BusinessLogic.Reference;using Platform.BusinessLogic.Reference.Mappings;using Platform.BusinessLogic.Registry;using Platform.BusinessLogic.Registry.Mappings;using Platform.BusinessLogic.Tablepart;using Platform.BusinessLogic.Tablepart.Mappings;
namespace Platform.BusinessLogic
{
	/// <summary>
	/// Дата-контекст
	/// </summary>
	public partial class DataContext : DbContext	{
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
		/// Статусы документов
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.DocStatus> DocStatus { get; set; }
		/// <summary>
		/// Операции сущностей
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.EntityOperation> EntityOperation { get; set; }
		/// <summary>
		/// Операции
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.Operation> Operation { get; set; }
		/// <summary>
		/// Сериализованный элемент сущности
		/// </summary>
		public DbSet<Platform.BusinessLogic.Registry.SerializedEntityItem> SerializedEntityItem { get; set; }
		/// <summary>
		/// Группы доступа
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.AccessGroup> AccessGroup { get; set; }
		/// <summary>
		/// Иерархия периодов
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.HierarchyPeriod> HierarchyPeriod { get; set; }
		/// <summary>
		/// Настройки сущности
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.EntitySetting> EntitySetting { get; set; }
		/// <summary>
		/// Настройки полей сущности
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.EntityFieldSetting> EntityFieldSetting { get; set; }
		/// <summary>
		/// Шаблон импорта
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.TemplateImport> TemplateImport { get; set; }
		/// <summary>
		/// Шаблон экспорта
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.TemplateExport> TemplateExport { get; set; }
		/// <summary>
		/// Сущности выборки шаблона выгрузки
		/// </summary>
		public DbSet<Platform.BusinessLogic.Tablepart.TemplateExport_Entity> TemplateExport_Entity { get; set; }
		/// <summary>
		/// Шаблоны импорта из Excel
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.TemplateImportXLS> TemplateImportXLS { get; set; }
		/// <summary>
		/// Сопоставление полей
		/// </summary>
		public DbSet<Platform.BusinessLogic.Tablepart.TemplateImportXLS_FieldsMap> TemplateImportXLS_FieldsMap { get; set; }
		/// <summary>
		/// Ключевые поля
		/// </summary>
		public DbSet<Platform.BusinessLogic.Tablepart.TemplateImportXLS_KeyField> TemplateImportXLS_KeyField { get; set; }
		/// <summary>
		/// Хранилище файлов
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.FileStore> FileStore { get; set; }
		/// <summary>
		/// Файлы
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.FileLink> FileLink { get; set; }
		/// <summary>
		/// Наименования сущностей для панели навигации
		/// </summary>
		public DbSet<Platform.BusinessLogic.Reference.ItemsCaptionsForNavigationPanel> ItemsCaptionsForNavigationPanel { get; set; }
		
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
			modelBuilder.Configurations.Add(new DocStatusMap());
			modelBuilder.Configurations.Add(new EntityOperationMap());
			modelBuilder.Configurations.Add(new OperationMap());
			modelBuilder.Configurations.Add(new SerializedEntityItemMap());
			modelBuilder.Configurations.Add(new AccessGroupMap());
			modelBuilder.Configurations.Add(new HierarchyPeriodMap());
			modelBuilder.Configurations.Add(new EntitySettingMap());
			modelBuilder.Configurations.Add(new EntityFieldSettingMap());
			modelBuilder.Configurations.Add(new TemplateImportMap());
			modelBuilder.Configurations.Add(new TemplateExportMap());
			modelBuilder.Configurations.Add(new TemplateExport_EntityMap());
			modelBuilder.Configurations.Add(new TemplateImportXLSMap());
			modelBuilder.Configurations.Add(new TemplateImportXLS_FieldsMapMap());
			modelBuilder.Configurations.Add(new TemplateImportXLS_KeyFieldMap());
			modelBuilder.Configurations.Add(new FileStoreMap());
			modelBuilder.Configurations.Add(new FileLinkMap());
			modelBuilder.Configurations.Add(new ItemsCaptionsForNavigationPanelMap());
			 
			CustomOnModelCreating(modelBuilder);
			base.OnModelCreating(modelBuilder);
		}

		partial void CustomOnModelCreating(DbModelBuilder modelBuilder);
		static partial void OnStaticConstruct();
	}
}