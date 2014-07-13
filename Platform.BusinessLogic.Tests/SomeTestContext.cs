using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Actions;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;
using SomeBusiness.Reference;
using BaseEntity = Platform.PrimaryEntities.BaseEntity;


// ReSharper disable CheckNamespace
namespace SomeBusiness
// ReSharper restore CheckNamespace
{
	public interface ITestContext : IDisposable
    {
        DbSet<Some> Somes { get; }
        DbSet<Other> Others { get; }
        
    }

	[ExcludeFromCodeCoverage]
	public class SomeTestContext : DbContext, ITestContext
    {
        public DbSet<Some> Somes { get; set; }
        public DbSet<Other> Others { get; set; }

        public SomeTestContext()
            : base("Name=SomeTestContext")
        {
            
        }

        //public SomeTestContext([Dependency("ConnectionString")] string connectionString)
        //    : base(connectionString)
        //{
            
        //}
    }


	[ExcludeFromCodeCoverage]
	public class BussinessTestContext : DataContext, ITestContext
    {
        public DbSet<Some> Somes { get; set; }
        public DbSet<Other> Others { get; set; }

        public BussinessTestContext()
            : base("BussinessTestContext")
        {

        }

        //public SomeTestContext([Dependency("ConnectionString")] string connectionString)
        //    : base(connectionString)
        //{

        //}
    }


    namespace Reference
    {
        interface ISome
        {
            string Name { get; set; }
        }

		[ExcludeFromCodeCoverage]
        public class Some : BaseEntity, IIdentitied, ISome
        {
            [Key]

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public Int32 Id { get; set; }

            public string Name { get; set; }


            
        }

		[ExcludeFromCodeCoverage]
		public class Other : ToolEntity, IInitNew
        {
            [Key]

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int Counter { get; set; }

            public string Name { get; set; }

            public int? SomeId { get; set; }

            public string Color { get; set; }
        

            [ForeignKey("SomeId")]
            public virtual Some Some { get; set; }

            [Control(ControlType.Any, Sequence.Before | Sequence.After)]
            public void Control()
            {
                Counter++;
            }

            public ClientActionList Clean()
            {
                Counter++;
                this.DocStatus = new DocStatus();
                return null;
            }


            public void Init(DbContext dbContext)
            {
                Name = "Ivan";
                if (Counter !=100 )
                    Counter++;
            }

        }

    }

}