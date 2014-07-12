using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BudgetExpenseStructure_BaseFilter
    /// </summary>
	public class BudgetExpenseStructure_BaseFilterMap : EntityTypeConfiguration<BudgetExpenseStructure_BaseFilter>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BudgetExpenseStructure_BaseFilterMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BudgetExpenseStructure_BaseFilter", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdFilterFieldType_ExpenseObligationType).HasColumnName("idFilterFieldType_ExpenseObligationType");
			this.Property(t => t.IdFilterFieldType_FinanceSource).HasColumnName("idFilterFieldType_FinanceSource");
			this.Property(t => t.IdFilterFieldType_KFO).HasColumnName("idFilterFieldType_KFO");
			this.Property(t => t.IdFilterFieldType_KVSR).HasColumnName("idFilterFieldType_KVSR");
			this.Property(t => t.IdFilterFieldType_RZPR).HasColumnName("idFilterFieldType_RZPR");
			this.Property(t => t.IdFilterFieldType_KCSR).HasColumnName("idFilterFieldType_KCSR");
			this.Property(t => t.IdFilterFieldType_KVR).HasColumnName("idFilterFieldType_KVR");
			this.Property(t => t.IdFilterFieldType_KOSGU).HasColumnName("idFilterFieldType_KOSGU");
			this.Property(t => t.IdFilterFieldType_DFK).HasColumnName("idFilterFieldType_DFK");
			this.Property(t => t.IdFilterFieldType_DKR).HasColumnName("idFilterFieldType_DKR");
			this.Property(t => t.IdFilterFieldType_DEK).HasColumnName("idFilterFieldType_DEK");
			this.Property(t => t.IdFilterFieldType_CodeSubsidy).HasColumnName("idFilterFieldType_CodeSubsidy");
			this.Property(t => t.IdFilterFieldType_BranchCode).HasColumnName("idFilterFieldType_BranchCode");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.BaseFilter).HasForeignKey(d => d.IdOwner);
			this.HasMany(t => t.FinanceSource).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idFinanceSource").ToTable("BudgetExpenseStructure_BaseFilter_FinanceSource", "ml"));
			this.HasMany(t => t.KFO).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idKFO").ToTable("BudgetExpenseStructure_BaseFilter_KFO", "ml"));
			this.HasMany(t => t.KVSR).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idKVSR").ToTable("BudgetExpenseStructure_BaseFilter_KVSR", "ml"));
			this.HasMany(t => t.RZPR).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idRZPR").ToTable("BudgetExpenseStructure_BaseFilter_RZPR", "ml"));
			this.HasMany(t => t.KCSR).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idKCSR").ToTable("BudgetExpenseStructure_BaseFilter_KCSR", "ml"));
			this.HasMany(t => t.KVR).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idKVR").ToTable("BudgetExpenseStructure_BaseFilter_KVR", "ml"));
			this.HasMany(t => t.KOSGU).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idKOSGU").ToTable("BudgetExpenseStructure_BaseFilter_KOSGU", "ml"));
			this.HasMany(t => t.DFK).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idDFK").ToTable("BudgetExpenseStructure_BaseFilter_DFK", "ml"));
			this.HasMany(t => t.DKR).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idDKR").ToTable("BudgetExpenseStructure_BaseFilter_DKR", "ml"));
			this.HasMany(t => t.DEK).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idDEK").ToTable("BudgetExpenseStructure_BaseFilter_DEK", "ml"));
			this.HasMany(t => t.CodeSubsidy).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idCodeSubsidy").ToTable("BudgetExpenseStructure_BaseFilter_CodeSubsidy", "ml"));
			this.HasMany(t => t.BranchCode).WithMany().Map(m => m.MapLeftKey("idBudgetExpenseStructure_BaseFilter").MapRightKey("idBranchCode").ToTable("BudgetExpenseStructure_BaseFilter_BranchCode", "ml"));
			
        }
    }
}
