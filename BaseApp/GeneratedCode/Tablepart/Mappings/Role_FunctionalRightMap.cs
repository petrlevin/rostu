using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Role_FunctionalRight
    /// </summary>
	public class Role_FunctionalRightMap : EntityTypeConfiguration<Role_FunctionalRight>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Role_FunctionalRightMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Role_FunctionalRight", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.EditingFlag).HasColumnName("EditingFlag");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.FunctionalRights).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			
        }
    }
}
