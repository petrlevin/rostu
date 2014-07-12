using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TemplateJustification
    /// </summary>
	public class TemplateJustificationMap : EntityTypeConfiguration<TemplateJustification>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TemplateJustificationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TemplateJustification", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			
        }
    }
}
