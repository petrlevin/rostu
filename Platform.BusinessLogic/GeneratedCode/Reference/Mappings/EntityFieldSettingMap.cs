using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности EntityFieldSetting
    /// </summary>
	public class EntityFieldSettingMap : EntityTypeConfiguration<EntityFieldSetting>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public EntityFieldSettingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("EntityFieldSetting", "ref");
			this.Property(t => t.IgnoreFilterByPublicLegalFormation).HasColumnName("IgnoreFilterByPublicLegalFormation");
			this.Property(t => t.IgnoreFilterByBudget).HasColumnName("IgnoreFilterByBudget");
			this.Property(t => t.IgnoreFilterByVersion).HasColumnName("IgnoreFilterByVersion");
			this.Property(t => t.IdAggregateFunction).HasColumnName("idAggregateFunction");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			this.Property(t => t.IgnoreOrganizationRights).HasColumnName("IgnoreOrganizationRights");
			this.Property(t => t.IdEntity_Owner).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idEntity_Owner");
			
            // Relationships
			this.HasRequired(t => t.EntityField).WithMany().HasForeignKey(d => d.IdEntityField);
			
        }
    }
}
