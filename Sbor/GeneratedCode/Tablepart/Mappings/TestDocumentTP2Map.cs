using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestDocumentTP2
    /// </summary>
	public class TestDocumentTP2Map : EntityTypeConfiguration<TestDocumentTP2>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestDocumentTP2Map()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestDocumentTP2", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Amount).HasColumnName("Amount");
			this.Property(t => t.IdValue).HasColumnName("idValue");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.OtherTablePart).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Value).WithMany().HasForeignKey(d => d.IdValue);
			
        }
    }
}
