using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности KCSR
    /// </summary>
	public class KCSRMap : EntityTypeConfiguration<KCSR>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public KCSRMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("KCSR", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.ValidityFrom).HasColumnName("ValidityFrom");
			this.Property(t => t.ValidityTo).HasColumnName("ValidityTo");
			this.Property(t => t.IdRoot).HasColumnName("idRoot");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasOptional(t => t.Root).WithMany(t => t.ChildrenByidRoot).HasForeignKey(d => d.IdRoot);
			
        }
    }
}
