using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BudgetExpenseStructure_BaseFilter_ExpenseObligationType
    /// </summary>
	public class BudgetExpenseStructure_BaseFilter_ExpenseObligationTypeMap : EntityTypeConfiguration<BudgetExpenseStructure_BaseFilter_ExpenseObligationType>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BudgetExpenseStructure_BaseFilter_ExpenseObligationTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BudgetExpenseStructure_BaseFilter_ExpenseObligationType", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdExpenseObligationType).HasColumnName("idExpenseObligationType");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ExpenseObligationTypes).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
