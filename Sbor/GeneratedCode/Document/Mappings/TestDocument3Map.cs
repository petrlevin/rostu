using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestDocument3
    /// </summary>
	public class TestDocument3Map : EntityTypeConfiguration<TestDocument3>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestDocument3Map()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestDocument3", "doc");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Zumma).HasPrecision(10,2).HasColumnName("Zumma");
			
            // Relationships
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			
        }
    }
}
