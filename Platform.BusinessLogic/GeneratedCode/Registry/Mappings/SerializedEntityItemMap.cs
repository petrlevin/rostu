using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SerializedEntityItem
    /// </summary>
	public class SerializedEntityItemMap : EntityTypeConfiguration<SerializedEntityItem>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SerializedEntityItemMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SerializedEntityItem", "reg");
			this.Property(t => t.Data).HasColumnName("Data");
			this.Property(t => t.IdTool).HasColumnName("idTool");
			this.Property(t => t.IdToolEntity).HasColumnName("idToolEntity");
			this.Property(t => t.Id).HasColumnName("id");
			
            // Relationships
			this.HasRequired(t => t.ToolEntity).WithMany().HasForeignKey(d => d.IdToolEntity);
			
        }
    }
}
