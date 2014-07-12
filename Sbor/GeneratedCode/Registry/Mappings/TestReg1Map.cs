using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestReg1
    /// </summary>
	public class TestReg1Map : EntityTypeConfiguration<TestReg1>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestReg1Map()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestReg1", "reg");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			this.Property(t => t.Text).HasColumnName("Text");
			this.Property(t => t.IdTerminator).HasColumnName("idTerminator");
			this.Property(t => t.IdTerminatorEntity).HasColumnName("idTerminatorEntity");
			this.Property(t => t.IdTerminateOperation).HasColumnName("idTerminateOperation");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			
            // Relationships
			this.HasRequired(t => t.Registrator).WithMany().HasForeignKey(d => d.IdRegistrator);
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			this.HasOptional(t => t.TerminatorEntity).WithMany().HasForeignKey(d => d.IdTerminatorEntity);
			this.HasOptional(t => t.TerminateOperation).WithMany().HasForeignKey(d => d.IdTerminateOperation);
			
        }
    }
}
