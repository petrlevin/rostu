using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestReg2
    /// </summary>
	public class TestReg2Map : EntityTypeConfiguration<TestReg2>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestReg2Map()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestReg2", "reg");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			
            // Relationships
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			
        }
    }
}
