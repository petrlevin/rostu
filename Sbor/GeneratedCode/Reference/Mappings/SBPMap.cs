using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SBP
    /// </summary>
	public class SBPMap : EntityTypeConfiguration<SBP>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SBPMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SBP", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdOrganization).HasColumnName("idOrganization");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdSBPType).HasColumnName("idSBPType");
			this.Property(t => t.IsFounder).HasColumnName("isFounder");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.IdKVSR).HasColumnName("idKVSR");
			this.Property(t => t.ValidityFrom).HasColumnName("ValidityFrom");
			this.Property(t => t.ValidityTo).HasColumnName("ValidityTo");
			this.Property(t => t.IdRoot).HasColumnName("idRoot");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Organization).WithMany().HasForeignKey(d => d.IdOrganization);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasOptional(t => t.KVSR).WithMany().HasForeignKey(d => d.IdKVSR);
			this.HasOptional(t => t.Root).WithMany(t => t.ChildrenByidRoot).HasForeignKey(d => d.IdRoot);
			
        }
    }
}
