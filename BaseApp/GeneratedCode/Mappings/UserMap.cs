using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("User", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			            this.Property(t => t.Name).HasColumnName("Name");
			            this.Property(t => t.Email).HasColumnName("Email");
			            this.Property(t => t.Password).HasColumnName("Password");
			            this.Property(t => t.DateofLastEntry).HasColumnName("DateofLastEntry");
			            this.Property(t => t.Department).HasColumnName("Department");
			            this.Property(t => t.Site).HasColumnName("Site");
			            this.Property(t => t.IISAddress).HasColumnName("IISAddress");
			            this.Property(t => t.Telephone).HasColumnName("Telephone");
			            this.Property(t => t.idAccessGroup).HasColumnName("idAccessGroup");
			
            // Relationships
			            this.HasOptional(t => t._idAccessGroup)
                .WithMany()
                .HasForeignKey(d => d.idAccessGroup);
			 
        }
    }
}