using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class UserRoleMap : EntityTypeConfiguration<UserRole>
    {
        public UserRoleMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("UserRole", "ml");
			            this.Property(t => t.idUser).HasColumnName("idUser");
			            this.Property(t => t.idRole).HasColumnName("idRole");
			
            // Relationships
			            this.HasRequired(t => t._idUser)
                .WithMany()
                .HasForeignKey(d => d.idUser);
			            this.HasRequired(t => t._idRole)
                .WithMany()
                .HasForeignKey(d => d.idRole);
			 
        }
    }
}