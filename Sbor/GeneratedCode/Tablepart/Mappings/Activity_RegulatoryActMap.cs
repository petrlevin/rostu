using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Activity_RegulatoryAct
    /// </summary>
	public class Activity_RegulatoryActMap : EntityTypeConfiguration<Activity_RegulatoryAct>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Activity_RegulatoryActMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Activity_RegulatoryAct", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdRegulatoryAct).HasColumnName("idRegulatoryAct");
			this.Property(t => t.IsBasis).HasColumnName("IsBasis");
			this.Property(t => t.IsEstablishQualityStandard).HasColumnName("IsEstablishQualityStandard");
			this.Property(t => t.IsSetMaxPrice).HasColumnName("IsSetMaxPrice");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RegulatoryAct).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.RegulatoryAct).WithMany().HasForeignKey(d => d.IdRegulatoryAct);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			
        }
    }
}
