using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicInstitutionEstimate_DistributionActivity
    /// </summary>
	public class PublicInstitutionEstimate_DistributionActivityMap : EntityTypeConfiguration<PublicInstitutionEstimate_DistributionActivity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicInstitutionEstimate_DistributionActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicInstitutionEstimate_DistributionActivity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdPublicInstitutionEstimate_Activity).HasColumnName("idPublicInstitutionEstimate_Activity");
			this.Property(t => t.DirectOFG).HasPrecision(20,2).HasColumnName("DirectOFG");
			this.Property(t => t.DirectPFG1).HasPrecision(20,2).HasColumnName("DirectPFG1");
			this.Property(t => t.DirectPFG2).HasPrecision(20,2).HasColumnName("DirectPFG2");
			this.Property(t => t.VolumeOFG).HasPrecision(20,2).HasColumnName("VolumeOFG");
			this.Property(t => t.VolumePFG1).HasPrecision(20,2).HasColumnName("VolumePFG1");
			this.Property(t => t.VolumePFG2).HasPrecision(20,2).HasColumnName("VolumePFG2");
			this.Property(t => t.FactorOFG).HasColumnName("FactorOFG");
			this.Property(t => t.FactorPFG1).HasColumnName("FactorPFG1");
			this.Property(t => t.FactorPFG2).HasColumnName("FactorPFG2");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DistributionActivities).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.PublicInstitutionEstimate_DistributionActivity).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.PublicInstitutionEstimate_Activity).WithMany(t => t.PublicInstitutionEstimate_DistributionActivity).HasForeignKey(d => d.IdPublicInstitutionEstimate_Activity);
			
        }
    }
}
