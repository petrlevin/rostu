using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ElementTypeSystemGoal_Document
    /// </summary>
	public class ElementTypeSystemGoal_DocumentMap : EntityTypeConfiguration<ElementTypeSystemGoal_Document>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ElementTypeSystemGoal_DocumentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ElementTypeSystemGoal_Document", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdDocType).HasColumnName("idDocType");
			this.Property(t => t.IsDefault).HasColumnName("isDefault");
			this.Property(t => t.Id).HasColumnName("id");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Document).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.DocType).WithMany().HasForeignKey(d => d.IdDocType);
			
        }
    }
}
