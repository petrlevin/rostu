using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BudgetExpenseStructure_CustomColumn
    /// </summary>
	public class BudgetExpenseStructure_CustomColumnMap : EntityTypeConfiguration<BudgetExpenseStructure_CustomColumn>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BudgetExpenseStructure_CustomColumnMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BudgetExpenseStructure_CustomColumn", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Name).HasColumnName("Name");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.CustomColumns).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
