using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PermissionsInputAdditionalRequirements
    /// </summary>
	public class PermissionsInputAdditionalRequirementsMap : EntityTypeConfiguration<PermissionsInputAdditionalRequirements>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PermissionsInputAdditionalRequirementsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PermissionsInputAdditionalRequirements", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.EnterAdditionalRequirements).HasColumnName("EnterAdditionalRequirements");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			
        }
    }
}
