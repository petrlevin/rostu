using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegisterActivity
    /// </summary>
	public class RegisterActivityMap : EntityTypeConfiguration<RegisterActivity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegisterActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegisterActivity", "doc");
			this.Property(t => t.Header).HasColumnName("Header");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdDocStatus).HasColumnName("IdDocStatus");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdDocType).HasColumnName("idDocType");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IsApproved).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("isApproved");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.ReasonCancel).HasColumnName("ReasonCancel");
			
            // Relationships
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.DocType).WithMany().HasForeignKey(d => d.IdDocType);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
