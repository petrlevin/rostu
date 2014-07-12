using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace Tools.MigrationHelper.Core.Context
{
	public class DevDbRevision
	{
		public string Path { get; set; }
		public int Revision { get; set; }
	}

    public class DevDbRevisionMap : EntityTypeConfiguration<DevDbRevision>
    {
        public DevDbRevisionMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Path, t.Revision });

            // Properties
            // Table & Column Mappings
            this.ToTable("DevDbRevision", "dbo");
            this.Property(t => t.Path).HasColumnName("path");
            this.Property(t => t.Revision).HasColumnName("revision");
        }
    }

	public class DevDbRevisionContext : DbContext
	{
		public DevDbRevisionContext(string connString)
			: base(connString)
		{
		}

        static DevDbRevisionContext()
		{
            Database.SetInitializer<DevDbRevisionContext>(null); // must be turned off before mini profiler runs
		}

		public DbSet<DevDbRevision> DevDbRevision { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new DevDbRevisionMap());
		}
	}
}
