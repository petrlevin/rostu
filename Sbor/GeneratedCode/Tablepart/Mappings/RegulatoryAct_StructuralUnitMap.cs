using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegulatoryAct_StructuralUnit
    /// </summary>
	public class RegulatoryAct_StructuralUnitMap : EntityTypeConfiguration<RegulatoryAct_StructuralUnit>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegulatoryAct_StructuralUnitMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegulatoryAct_StructuralUnit", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Article).HasColumnName("Article");
			this.Property(t => t.Part).HasColumnName("Part");
			this.Property(t => t.Item).HasColumnName("Item");
			this.Property(t => t.SubItem).HasColumnName("SubItem");
			this.Property(t => t.Paragraph).HasColumnName("Paragraph");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Id).HasColumnName("id");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.StructuralUnit).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
