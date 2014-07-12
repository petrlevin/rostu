using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_DistributionAdditionalParameter
    /// </summary>
	public class FBA_DistributionAdditionalParameterMap : EntityTypeConfiguration<FBA_DistributionAdditionalParameter>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_DistributionAdditionalParameterMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_DistributionAdditionalParameter", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdKOSGU).HasColumnName("idKOSGU");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DistributionAdditionalParameters).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.FBA_DistributionAdditionalParameter).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.KOSGU).WithMany().HasForeignKey(d => d.IdKOSGU);
			
        }
    }
}
