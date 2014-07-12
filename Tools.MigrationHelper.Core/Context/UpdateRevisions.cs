using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using Platform.PrimaryEntities.Reference;

namespace Tools.MigrationHelper.Core.Context
{
    public class UpdateRevision
    {
        public int Id { get; set; }
        public int? MajorVersionNumber { get; set; }
        public int? MinorVersionNumber { get; set; }
        public int? BuildNumber { get; set; }
        public int Revision { get; set; }
        public DateTime Date { get; set; }
        public byte[] File { get; set; }
    }

    public class UpdateRevisionMap : EntityTypeConfiguration<UpdateRevision>
    {
        public UpdateRevisionMap()
        {
            HasKey(t => t.Id);

            ToTable("UpdateRevision", "reg");
            this.Property(t => t.Id).HasColumnName("id");
            this.Property(t => t.Date).HasColumnName("Date");
            this.Property(t => t.Revision).HasColumnName("Revision");
            this.Property(t => t.MajorVersionNumber).HasColumnName("MajorVersionNumber");
            this.Property(t => t.MinorVersionNumber).HasColumnName("MinorVersionNumber");
            this.Property(t => t.BuildNumber).HasColumnName("BuildNumber");
            this.Property(t => t.File).HasColumnName("File");
        }
    }

    public class UpdateRevisionContext : DbContext
    {
        public UpdateRevisionContext(string connString)
            : base(connString)
        {
        }

        static UpdateRevisionContext()
        {
            Database.SetInitializer<UpdateRevisionContext>(null); // must be turned off before mini profiler runs
        }

        public DbSet<UpdateRevision> UpdateRevisions { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new UpdateRevisionMap());
        }
    }
}
