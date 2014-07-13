using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ItemsCaptionsForNavigationPanel
    /// </summary>
	public class ItemsCaptionsForNavigationPanelMap : EntityTypeConfiguration<ItemsCaptionsForNavigationPanel>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ItemsCaptionsForNavigationPanelMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItemsCaptionsForNavigationPanel", "ref");
			this.Property(t => t.Comment).HasColumnName("Comment");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdEntitySetting).HasColumnName("idEntitySetting");
			
            // Relationships
			this.HasOptional(t => t.EntitySetting).WithMany(t => t.ItemsCaptionsForNavigationPanel).HasForeignKey(d => d.IdEntitySetting);
			
        }
    }
}
