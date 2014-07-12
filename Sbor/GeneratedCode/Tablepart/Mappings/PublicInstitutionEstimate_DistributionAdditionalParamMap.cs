using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicInstitutionEstimate_DistributionAdditionalParam
    /// </summary>
	public class PublicInstitutionEstimate_DistributionAdditionalParamMap : EntityTypeConfiguration<PublicInstitutionEstimate_DistributionAdditionalParam>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicInstitutionEstimate_DistributionAdditionalParamMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicInstitutionEstimate_DistributionAdditionalParam", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdKOSGU).HasColumnName("idKOSGU");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DistributionAdditionalParams).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.PublicInstitutionEstimate_DistributionAdditionalParam).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.KOSGU).WithMany().HasForeignKey(d => d.IdKOSGU);
			
        }
    }
}
