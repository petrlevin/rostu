using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_ResourceMaintenance
    /// </summary>
	public class ActivityOfSBP_ResourceMaintenanceMap : EntityTypeConfiguration<ActivityOfSBP_ResourceMaintenance>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_ResourceMaintenanceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_ResourceMaintenance", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ResourceMaintenance).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			
        }
    }
}
