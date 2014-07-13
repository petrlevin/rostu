using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Role_RefStatus
    /// </summary>
	public class Role_RefStatusMap : EntityTypeConfiguration<Role_RefStatus>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Role_RefStatusMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Role_RefStatus", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.SwitchOn).HasColumnName("SwitchOn");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Role_RefStatus).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.Role_RefStatus).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
