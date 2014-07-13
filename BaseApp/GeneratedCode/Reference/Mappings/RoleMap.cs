using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Role
    /// </summary>
	public class RoleMap : EntityTypeConfiguration<Role>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RoleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Role", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdAccessGroup).HasColumnName("idAccessGroup");
			this.Property(t => t.IdRoleType).HasColumnName("idRoleType");
			this.Property(t => t.IdRoleKind).HasColumnName("idRoleKind");
			
            // Relationships
			this.HasOptional(t => t.AccessGroup).WithMany().HasForeignKey(d => d.IdAccessGroup);
			
        }
    }
}
