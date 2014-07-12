using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности StateProgram_DGPKAResourceMaintenance
    /// </summary>
	public class StateProgram_DGPKAResourceMaintenanceMap : EntityTypeConfiguration<StateProgram_DGPKAResourceMaintenance>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public StateProgram_DGPKAResourceMaintenanceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("StateProgram_DGPKAResourceMaintenance", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DGPKAResourceMaintenance).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.StateProgram_DGPKAResourceMaintenance).HasForeignKey(d => d.IdMaster);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			
        }
    }
}
