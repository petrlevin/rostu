using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LongTermGoalProgram_ActivityResourceMaintenance
    /// </summary>
	public class LongTermGoalProgram_ActivityResourceMaintenanceMap : EntityTypeConfiguration<LongTermGoalProgram_ActivityResourceMaintenance>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LongTermGoalProgram_ActivityResourceMaintenanceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LongTermGoalProgram_ActivityResourceMaintenance", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityResourceMaintenance).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.LongTermGoalProgram_ActivityResourceMaintenance).HasForeignKey(d => d.IdMaster);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			
        }
    }
}
