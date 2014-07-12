using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_IndirectCosts_value
    /// </summary>
	public class FBA_IndirectCosts_valueMap : EntityTypeConfiguration<FBA_IndirectCosts_value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_IndirectCosts_valueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_IndirectCosts_value", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(18,2).HasColumnName("Value");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.IndirectCosts_values).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.FBA_IndirectCosts_value).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			
        }
    }
}
