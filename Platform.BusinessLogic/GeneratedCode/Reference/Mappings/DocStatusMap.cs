using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности DocStatus
    /// </summary>
	public class DocStatusMap : EntityTypeConfiguration<DocStatus>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public DocStatusMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DocStatus", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			
            // Relationships
			
        }
    }
}
