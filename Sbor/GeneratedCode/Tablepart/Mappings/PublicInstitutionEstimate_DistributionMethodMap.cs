using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicInstitutionEstimate_DistributionMethod
    /// </summary>
	public class PublicInstitutionEstimate_DistributionMethodMap : EntityTypeConfiguration<PublicInstitutionEstimate_DistributionMethod>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicInstitutionEstimate_DistributionMethodMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicInstitutionEstimate_DistributionMethod", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdIndirectCostsDistributionMethod).HasColumnName("idIndirectCostsDistributionMethod");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DistributionMethods).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
