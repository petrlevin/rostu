using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_ChangeHistory
    /// </summary>
	public class BalancingIFDB_ChangeHistoryMap : EntityTypeConfiguration<BalancingIFDB_ChangeHistory>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_ChangeHistoryMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_ChangeHistory", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdBalancingIFDB_RuleIndex).HasColumnName("idBalancingIFDB_RuleIndex");
			this.Property(t => t.OldValue).HasPrecision(22,2).HasColumnName("OldValue");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ChangeHistory).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.BalancingIFDB_ChangeHistory).HasForeignKey(d => d.IdMaster);
			this.HasOptional(t => t.BalancingIFDB_RuleIndex).WithMany(t => t.BalancingIFDB_ChangeHistory).HasForeignKey(d => d.IdBalancingIFDB_RuleIndex);
			
        }
    }
}
