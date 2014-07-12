using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности InterBudgetaryTransfers_CustomizableColumns
    /// </summary>
	public class InterBudgetaryTransfers_CustomizableColumnsMap : EntityTypeConfiguration<InterBudgetaryTransfers_CustomizableColumns>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public InterBudgetaryTransfers_CustomizableColumnsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("InterBudgetaryTransfers_CustomizableColumns", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Order).HasColumnName("Order");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.CustomizableColumns).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
