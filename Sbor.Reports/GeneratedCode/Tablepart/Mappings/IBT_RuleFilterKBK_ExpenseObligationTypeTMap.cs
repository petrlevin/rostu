using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности IBT_RuleFilterKBK_ExpenseObligationTypeT
    /// </summary>
	public class IBT_RuleFilterKBK_ExpenseObligationTypeTMap : EntityTypeConfiguration<IBT_RuleFilterKBK_ExpenseObligationTypeT>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public IBT_RuleFilterKBK_ExpenseObligationTypeTMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("IBT_RuleFilterKBK_ExpenseObligationTypeT", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdExpenseObligationType).HasColumnName("idExpenseObligationType");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ExpenseObligationTypeTs).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
