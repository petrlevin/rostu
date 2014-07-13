using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности User
    /// </summary>
	public class UserMap : EntityTypeConfiguration<User>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("User", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Email).HasColumnName("Email");
			this.Property(t => t.Password).HasColumnName("Password");
			this.Property(t => t.DateofLastEntry).HasColumnName("DateofLastEntry");
			this.Property(t => t.Department).HasColumnName("Department");
			this.Property(t => t.Site).HasColumnName("Site");
			this.Property(t => t.IISAddress).HasColumnName("IISAddress");
			this.Property(t => t.Telephone).HasColumnName("Telephone");
			this.Property(t => t.IdAccessGroup).HasColumnName("idAccessGroup");
			this.Property(t => t.IdOrganization).HasColumnName("idOrganization");
			this.Property(t => t.IdResponsiblePerson).HasColumnName("idResponsiblePerson");
			this.Property(t => t.ChangePasswordNextTime).HasColumnName("ChangePasswordNextTime");
			this.Property(t => t.IsBlocked).HasColumnName("IsBlocked");
			
            // Relationships
			this.HasRequired(t => t.AccessGroup).WithMany().HasForeignKey(d => d.IdAccessGroup);
			this.HasOptional(t => t.Organization).WithMany().HasForeignKey(d => d.IdOrganization);
			this.HasOptional(t => t.ResponsiblePerson).WithMany().HasForeignKey(d => d.IdResponsiblePerson);
			this.HasMany(t => t.Roles).WithMany(r => r.Users).Map(m => m.MapLeftKey("idUser").MapRightKey("idRole").ToTable("UserRole", "ml"));
			
        }
    }
}
