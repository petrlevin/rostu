

using Platform.Common;
// This file was automatically generated.
// Do not make changes directly to this file - edit the template instead.
// 
// The following connection settings were used to generate this file
// 
//     Configuration file:     ""
//     Connection String Name: ""
//     Connection String:      "Data Source=.;Initial Catalog=platform3_Audit;Integrated Security=true;Current Language=English"

// ReSharper disable RedundantUsingDirective
// ReSharper disable DoNotCallOverridableMethodsInConstructor
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

//using DatabaseGeneratedOption = System.ComponentModel.DataAnnotations.DatabaseGeneratedOption;

namespace Platform.BusinessLogic.Auditing.EfModel
{
    // ************************************************************************
    // Unit of work
    public interface IAuditDataContext : IDisposable
    {
        IDbSet<Data> Data { get; set; } // data
        IDbSet<MultilinkData> MultilinkData { get; set; } // multilink_data

        int SaveChanges();
    }

    // ************************************************************************
    // Database context
    public class AuditDataContext : DbContext, IAuditDataContext
    {
        public IDbSet<Data> Data { get; set; } // data
        public IDbSet<MultilinkData> MultilinkData { get; set; } // multilink_data

        static AuditDataContext()
        {
            Database.SetInitializer<AuditDataContext>(null);
        }

        public AuditDataContext()
            : base( IoC.Resolve<AuditConfiguration>().ConnectionString )
        {
        }

        public AuditDataContext(string connectionString) : base(connectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new DataConfiguration());
            modelBuilder.Configurations.Add(new MultilinkDataConfiguration());
        }
    }

    // ************************************************************************
    // POCO classes

    // data
    public class Data
    {
        public int Id { get; set; } // id (Primary key)
        public string Before { get; set; } // Before
        public string After { get; set; } // After
        public int EntityId { get; set; } // EntityId
        public byte Operation { get; set; } // Operation
        public int ElementId { get; set; } // ElementId
        public int IdUser { get; set; } // IdUser
        public DateTime Date { get; set; } // Date
    }

    // multilink_data
    public class MultilinkData
    {
        public int Id { get; set; } // id (Primary key)
        public int EntityId { get; set; } // EntityId
        public byte Operation { get; set; } // Operation
        public int FirstId { get; set; } // FirstId
        public int SecondId { get; set; } // SecondId
        public int IdUser { get; set; } // IdUser
        public DateTime Date { get; set; } // Date
    }


    // ************************************************************************
    // POCO Configuration

    // data
    internal class DataConfiguration : EntityTypeConfiguration<Data>
    {
        public DataConfiguration()
        {
            ToTable("dbo.data");
            HasKey(x => x.Id);

            Property(x => x.Id).HasColumnName("id").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.Before).HasColumnName("Before").IsOptional();
            Property(x => x.After).HasColumnName("After").IsOptional();
            Property(x => x.EntityId).HasColumnName("EntityId").IsRequired();
            Property(x => x.Operation).HasColumnName("Operation").IsRequired();
            Property(x => x.ElementId).HasColumnName("ElementId").IsRequired();
            Property(x => x.IdUser).HasColumnName("IdUser").IsRequired();
            Property(x => x.Date).HasColumnName("Date").IsRequired();
        }
    }

    // multilink_data
    internal class MultilinkDataConfiguration : EntityTypeConfiguration<MultilinkData>
    {
        public MultilinkDataConfiguration()
        {
            ToTable("dbo.multilink_data");
            HasKey(x => x.Id);

            Property(x => x.Id).HasColumnName("id").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.EntityId).HasColumnName("EntityId").IsRequired();
            Property(x => x.Operation).HasColumnName("Operation").IsRequired();
            Property(x => x.FirstId).HasColumnName("FirstId").IsRequired();
            Property(x => x.SecondId).HasColumnName("SecondId").IsRequired();
            Property(x => x.IdUser).HasColumnName("IdUser").IsRequired();
            Property(x => x.Date).HasColumnName("Date").IsRequired();
        }
    }

}

