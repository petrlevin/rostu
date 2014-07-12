using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;
using System.Data.Common;
using Platform.BusinessLogic.EntityFramework;

using Tests.Reference;using Tests.Reference.Mappings;using Tests.Tablepart;using Tests.Tablepart.Mappings;
namespace Tests
{
	/// <summary>
	/// Дата-контекст
	/// </summary>
	public partial class DataContext : Sbor.Reports.DataContext	{
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
		/// ДТЧ
		/// </summary>
		public DbSet<Tests.Reference.testDtp> testDtp { get; set; }
		/// <summary>
		/// Дочерняя ТЧ [ТЧ справочника "ДТЧ"]
		/// </summary>
		public DbSet<Tests.Tablepart.testDtp_Child> testDtp_Child { get; set; }
		
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
			modelBuilder.Configurations.Add(new testDtpMap());
			modelBuilder.Configurations.Add(new testDtp_ChildMap());
			 
			CustomOnModelCreating(modelBuilder);
			base.OnModelCreating(modelBuilder);
		}

		partial void CustomOnModelCreating(DbModelBuilder modelBuilder);
		static partial void OnStaticConstruct();
	}
}