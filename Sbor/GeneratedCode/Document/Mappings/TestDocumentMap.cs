using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestDocument
    /// </summary>
	public class TestDocumentMap : EntityTypeConfiguration<TestDocument>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestDocumentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestDocument", "doc");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.Zumma).HasPrecision(10,2).HasColumnName("Zumma");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdSbp).HasColumnName("idSbp");
			this.Property(t => t.IdOther).HasColumnName("idOther");
			this.Property(t => t.IdThis).HasColumnName("idThis");
			this.Property(t => t.AAA).HasColumnName("AAA");
			
            // Relationships
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasOptional(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Sbp).WithMany().HasForeignKey(d => d.IdSbp);
			this.HasOptional(t => t.Other).WithMany().HasForeignKey(d => d.IdOther);
			this.HasOptional(t => t.This).WithMany(t => t.ChildrenByidThis).HasForeignKey(d => d.IdThis);
			
        }
    }
}
