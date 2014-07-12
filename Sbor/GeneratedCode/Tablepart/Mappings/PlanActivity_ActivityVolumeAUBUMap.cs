using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_ActivityVolumeAUBU
    /// </summary>
	public class PlanActivity_ActivityVolumeAUBUMap : EntityTypeConfiguration<PlanActivity_ActivityVolumeAUBU>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_ActivityVolumeAUBUMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_ActivityVolumeAUBU", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Volume).HasPrecision(20,5).HasColumnName("Volume");
			this.Property(t => t.AdditionalVolume).HasPrecision(20,5).HasColumnName("AdditionalVolume");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivityVolumeAUBU).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.PlanActivity_ActivityVolumeAUBU).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			
        }
    }
}
