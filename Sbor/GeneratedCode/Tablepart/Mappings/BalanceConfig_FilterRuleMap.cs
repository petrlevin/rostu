using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalanceConfig_FilterRule
    /// </summary>
	public class BalanceConfig_FilterRuleMap : EntityTypeConfiguration<BalanceConfig_FilterRule>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalanceConfig_FilterRuleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalanceConfig_FilterRule", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Caption).HasColumnName("Caption");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.FilterRules).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
