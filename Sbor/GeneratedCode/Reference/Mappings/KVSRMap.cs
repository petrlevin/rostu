using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности KVSR
    /// </summary>
	public class KVSRMap : EntityTypeConfiguration<KVSR>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public KVSRMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("KVSR", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.ValidityFrom).HasColumnName("ValidityFrom");
			this.Property(t => t.ValidityTo).HasColumnName("ValidityTo");
			this.Property(t => t.IdRoot).HasColumnName("idRoot");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Root).WithMany(t => t.ChildrenByidRoot).HasForeignKey(d => d.IdRoot);
			
        }
    }
}
