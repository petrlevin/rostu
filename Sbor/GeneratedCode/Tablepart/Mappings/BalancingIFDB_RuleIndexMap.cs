using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_RuleIndex
    /// </summary>
	public class BalancingIFDB_RuleIndexMap : EntityTypeConfiguration<BalancingIFDB_RuleIndex>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_RuleIndexMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_RuleIndex", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IndexOFG).HasPrecision(12,5).HasColumnName("IndexOFG");
			this.Property(t => t.IndexPFG1).HasPrecision(12,5).HasColumnName("IndexPFG1");
			this.Property(t => t.IndexPFG2).HasPrecision(12,5).HasColumnName("IndexPFG2");
			this.Property(t => t.IsApplied).HasColumnName("isApplied");
			this.Property(t => t.ChangeCount).HasColumnName("ChangeCount");
			this.Property(t => t.ChangeNumber).HasColumnName("ChangeNumber");
			this.Property(t => t.IsIncludeAdditionalNeed).HasColumnName("isIncludeAdditionalNeed");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RuleIndexs).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
