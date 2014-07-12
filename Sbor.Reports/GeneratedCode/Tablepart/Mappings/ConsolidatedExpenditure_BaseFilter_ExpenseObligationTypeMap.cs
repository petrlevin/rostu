using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ConsolidatedExpenditure_BaseFilter_ExpenseObligationType
    /// </summary>
	public class ConsolidatedExpenditure_BaseFilter_ExpenseObligationTypeMap : EntityTypeConfiguration<ConsolidatedExpenditure_BaseFilter_ExpenseObligationType>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ConsolidatedExpenditure_BaseFilter_ExpenseObligationTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ConsolidatedExpenditure_BaseFilter_ExpenseObligationType", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdExpenseObligationType).HasColumnName("idExpenseObligationType");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ExpenseObligationType).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
