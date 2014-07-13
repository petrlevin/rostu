using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Organization
    /// </summary>
	public class OrganizationMap : EntityTypeConfiguration<Organization>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public OrganizationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Organization", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.INN).HasColumnName("INN");
			this.Property(t => t.KPP).HasColumnName("KPP");
			this.Property(t => t.CodeOrgBud).HasColumnName("CodeOrgBud");
			this.Property(t => t.IdOKATO).HasColumnName("idOKATO");
			this.Property(t => t.PostAdress).HasColumnName("PostAdress");
			this.Property(t => t.LegalAddress).HasColumnName("LegalAddress");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.Okpo).HasColumnName("Okpo");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.OKATO).WithMany().HasForeignKey(d => d.IdOKATO);
			
        }
    }
}
