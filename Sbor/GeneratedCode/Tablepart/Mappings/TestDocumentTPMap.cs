using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestDocumentTP
    /// </summary>
	public class TestDocumentTPMap : EntityTypeConfiguration<TestDocumentTP>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestDocumentTPMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestDocumentTP", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Value).HasColumnName("Value");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SomeTablePart).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
