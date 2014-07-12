using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_SetShowKBK
    /// </summary>
	public class BalancingIFDB_SetShowKBKMap : EntityTypeConfiguration<BalancingIFDB_SetShowKBK>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_SetShowKBKMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_SetShowKBK", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SetShowKBKs).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.EntityField).WithMany().HasForeignKey(d => d.IdEntityField);
			
        }
    }
}
