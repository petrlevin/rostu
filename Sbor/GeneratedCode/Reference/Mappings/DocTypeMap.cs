using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности DocType
    /// </summary>
	public class DocTypeMap : EntityTypeConfiguration<DocType>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public DocTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DocType", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			
        }
    }
}
