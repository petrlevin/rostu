using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности StateProgram_SubProgramResourceMaintenance
    /// </summary>
	public class StateProgram_SubProgramResourceMaintenanceMap : EntityTypeConfiguration<StateProgram_SubProgramResourceMaintenance>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public StateProgram_SubProgramResourceMaintenanceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("StateProgram_SubProgramResourceMaintenance", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SubProgramResourceMaintenance).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.StateProgram_SubProgramResourceMaintenance).HasForeignKey(d => d.IdMaster);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			
        }
    }
}
