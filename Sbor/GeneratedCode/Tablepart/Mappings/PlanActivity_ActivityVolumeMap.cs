using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_ActivityVolume
    /// </summary>
	public class PlanActivity_ActivityVolumeMap : EntityTypeConfiguration<PlanActivity_ActivityVolume>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_ActivityVolumeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_ActivityVolume", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Volume).HasPrecision(20,5).HasColumnName("Volume");
			this.Property(t => t.PriceLimit).HasPrecision(18,2).HasColumnName("PriceLimit");
			this.Property(t => t.AdditionalVolume).HasPrecision(20,5).HasColumnName("AdditionalVolume");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityVolumes).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.PlanActivity_ActivityVolume).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			
        }
    }
}
