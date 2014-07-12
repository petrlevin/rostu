using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestDocument2
    /// </summary>
	public class TestDocument2Map : EntityTypeConfiguration<TestDocument2>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestDocument2Map()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestDocument2", "doc");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.SSS).HasColumnName("SSS");
			
            // Relationships
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
