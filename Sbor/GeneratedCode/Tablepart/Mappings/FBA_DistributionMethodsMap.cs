using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_DistributionMethods
    /// </summary>
	public class FBA_DistributionMethodsMap : EntityTypeConfiguration<FBA_DistributionMethods>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_DistributionMethodsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_DistributionMethods", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdIndirectCostsDistributionMethod).HasColumnName("idIndirectCostsDistributionMethod");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DistributionMethodss).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
