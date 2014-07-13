using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Role_DocumentOperation
    /// </summary>
	public class Role_DocumentOperationMap : EntityTypeConfiguration<Role_DocumentOperation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Role_DocumentOperationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Role_DocumentOperation", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOperation).HasColumnName("idOperation");
			this.Property(t => t.SwitchOn).HasColumnName("SwitchOn");
			this.Property(t => t.InitialStatus).HasColumnName("InitialStatus");
			this.Property(t => t.FinalStatus).HasColumnName("FinalStatus");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Role_DocumentOperation).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Operation).WithMany().HasForeignKey(d => d.IdOperation);
			this.HasRequired(t => t.Master).WithMany(t => t.Role_DocumentOperation).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
