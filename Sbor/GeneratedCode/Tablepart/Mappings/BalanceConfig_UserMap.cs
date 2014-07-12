using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalanceConfig_User
    /// </summary>
	public class BalanceConfig_UserMap : EntityTypeConfiguration<BalanceConfig_User>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalanceConfig_UserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalanceConfig_User", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Users).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			this.HasRequired(t => t.Master).WithMany(t => t.BalanceConfig_User).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
