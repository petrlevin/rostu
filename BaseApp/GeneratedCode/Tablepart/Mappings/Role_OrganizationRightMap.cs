using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Role_OrganizationRight
    /// </summary>
	public class Role_OrganizationRightMap : EntityTypeConfiguration<Role_OrganizationRight>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Role_OrganizationRightMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Role_OrganizationRight", "tp");
			this.Property(t => t.IdParentField).HasColumnName("idParentField");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdElement).HasColumnName("idElement");
			this.Property(t => t.IdElementEntity).HasColumnName("idElementEntity");
			this.Property(t => t.EditingFlag).HasColumnName("EditingFlag");
			this.Property(t => t.Disabled).HasColumnName("Disabled");
			
            // Relationships
			this.HasOptional(t => t.ParentField).WithMany().HasForeignKey(d => d.IdParentField);
			this.HasRequired(t => t.Owner).WithMany(t => t.Role_OrganizationRight).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.ElementEntity).WithMany().HasForeignKey(d => d.IdElementEntity);
			
        }
    }
}
