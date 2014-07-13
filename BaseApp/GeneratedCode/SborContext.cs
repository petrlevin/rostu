using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;
using BaseApp.GeneratedCode.Reference;using BaseApp.GeneratedCode.Reference.Mappings;using BaseApp.GeneratedCode.Multilink;using BaseApp.GeneratedCode.Multilink.Mappings;
namespace BaseApp.GeneratedCode
{
    public partial class SborContext : DbContext
    {
        static SborContext()
        {
            Database.SetInitializer<SborContext>(null);
        }

		public SborContext(): base("Name=PlatformDBConnectionString")
		{
		}

		public SborContext([Dependency("ConnectionString")] string connectionString)
            : base(connectionString)
        {
            
        }

		public DbSet<User> User { get; set; }
		public DbSet<Role> Role { get; set; }
		public DbSet<UserRole> UserRole { get; set; }
		public DbSet<PublicLegalFormation> PublicLegalFormation { get; set; }
		public DbSet<BudgetLevel> BudgetLevel { get; set; }
		public DbSet<AccessGroup> AccessGroup { get; set; }
		public DbSet<UnitDimension> UnitDimension { get; set; }
		public DbSet<PublicLegalFormationModule> PublicLegalFormationModule { get; set; }
		public DbSet<Budget> Budget { get; set; }
		
		public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
			modelBuilder.Configurations.Add(new UserMap());
			modelBuilder.Configurations.Add(new RoleMap());
			modelBuilder.Configurations.Add(new UserRoleMap());
			modelBuilder.Configurations.Add(new PublicLegalFormationMap());
			modelBuilder.Configurations.Add(new BudgetLevelMap());
			modelBuilder.Configurations.Add(new AccessGroupMap());
			modelBuilder.Configurations.Add(new UnitDimensionMap());
			modelBuilder.Configurations.Add(new PublicLegalFormationModuleMap());
			modelBuilder.Configurations.Add(new BudgetMap());
			 
		}
    }
}