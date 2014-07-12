using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BudgetExpenseStructure_Columns
    /// </summary>
	public class BudgetExpenseStructure_ColumnsMap : EntityTypeConfiguration<BudgetExpenseStructure_Columns>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BudgetExpenseStructure_ColumnsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BudgetExpenseStructure_Columns", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdKBKEntity).HasColumnName("idKBKEntity");
			this.Property(t => t.Order).HasColumnName("Order");
			this.Property(t => t.IsGroupResult).HasColumnName("isGroupResult");
			this.Property(t => t.MinLevel).HasColumnName("minLevel");
			this.Property(t => t.MaxLevel).HasColumnName("maxLevel");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Columns).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.KBKEntity).WithMany().HasForeignKey(d => d.IdKBKEntity);
			
        }
    }
}
