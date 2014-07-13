using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ResponsiblePerson
    /// </summary>
	public class ResponsiblePersonMap : EntityTypeConfiguration<ResponsiblePerson>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ResponsiblePersonMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ResponsiblePerson", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdOrganization).HasColumnName("idOrganization");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdOfficialCapacity).HasColumnName("idOfficialCapacity");
			this.Property(t => t.IdRoleResponsiblePerson).HasColumnName("idRoleResponsiblePerson");
			this.Property(t => t.Phone).HasColumnName("Phone");
			this.Property(t => t.Email).HasColumnName("Email");
			this.Property(t => t.MoreInformation).HasColumnName("MoreInformation");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Organization).WithMany(t => t.ResponsiblePerson).HasForeignKey(d => d.IdOrganization);
			this.HasRequired(t => t.OfficialCapacity).WithMany().HasForeignKey(d => d.IdOfficialCapacity);
			
        }
    }
}
