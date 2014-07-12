using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using Platform.PrimaryEntities.Reference;

namespace Tools.MigrationHelper.Core.Context
{
    public class ProgrammabilityMap : EntityTypeConfiguration<Programmability>
    {
        public ProgrammabilityMap()
        {
            HasKey(t => t.Id);

            ToTable("Programmability", "ref");
            this.Property(t => t.Schema).HasColumnName("Schema");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.CreateCommand).HasColumnName("CreateCommand");
            this.Property(t => t.IdProgrammabilityType).HasColumnName("idProgrammabilityType");
            this.Property(t => t.Order).HasColumnName("Order");
            this.Property(t => t.IdProject).HasColumnName("idProject");
            this.Property(t => t.IsDisabled).HasColumnName("isDisabled");
            this.Ignore(t => t.ProgrammabilityType);
            this.Ignore(t => t.Project);
        }
    }

    public class ItemsDependency
    {
        public int Id { get; set; }
        public int IdObject { get; set; }
        public int IdObjectEntity { get; set; }
        public int IdDependsOn { get; set; }
        public int IdDependsOnEntity { get; set; }
    }

    public class ItemsDependenciesMap : EntityTypeConfiguration<ItemsDependency>
    {
        public ItemsDependenciesMap()
        {
            HasKey(t => t.Id);

            ToTable("ItemsDependencies", "reg");
            this.Property(t => t.IdObject).HasColumnName("idObject");
            this.Property(t => t.IdObjectEntity).HasColumnName("idObjectEntity");
            this.Property(t => t.IdDependsOn).HasColumnName("idDependsOn");
            this.Property(t => t.IdDependsOnEntity).HasColumnName("idDependsOnEntity");
        }
    }

    public class ProgrammabilityContext : DbContext
    {
        public ProgrammabilityContext(string connString)
            : base(connString)
        {
        }

        static ProgrammabilityContext()
        {
            Database.SetInitializer<UpdateRevisionContext>(null); // must be turned off before mini profiler runs
        }

        public DbSet<ItemsDependency> ItemsDependencies { get; set; }
        public DbSet<Programmability> Programmabilities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ProgrammabilityMap());
            modelBuilder.Configurations.Add(new ItemsDependenciesMap());
        }
    }
}
