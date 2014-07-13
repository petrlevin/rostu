using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности EntitySetting
    /// </summary>
	public class EntitySettingMap : EntityTypeConfiguration<EntitySetting>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public EntitySettingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("EntitySetting", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.IdItemForm).HasColumnName("idItemForm");
			this.Property(t => t.IdListForm).HasColumnName("idListForm");
			this.Property(t => t.IdSelectionForm).HasColumnName("idSelectionForm");
			this.Property(t => t.AlwaysShowLinearly).HasColumnName("AlwaysShowLinearly");
			this.Property(t => t.IdEntityField_Hierarchy).HasColumnName("idEntityField_Hierarchy");
			this.Property(t => t.ClassSelectionCaption).HasColumnName("ClassSelectionCaption");
			
            // Relationships
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			this.HasOptional(t => t.ItemForm).WithMany().HasForeignKey(d => d.IdItemForm);
			this.HasOptional(t => t.ListForm).WithMany().HasForeignKey(d => d.IdListForm);
			this.HasOptional(t => t.SelectionForm).WithMany().HasForeignKey(d => d.IdSelectionForm);
			this.HasOptional(t => t.EntityField_Hierarchy).WithMany().HasForeignKey(d => d.IdEntityField_Hierarchy);
			
        }
    }
}
