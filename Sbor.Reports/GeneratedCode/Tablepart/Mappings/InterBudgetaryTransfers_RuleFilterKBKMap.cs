using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности InterBudgetaryTransfers_RuleFilterKBK
    /// </summary>
	public class InterBudgetaryTransfers_RuleFilterKBKMap : EntityTypeConfiguration<InterBudgetaryTransfers_RuleFilterKBK>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public InterBudgetaryTransfers_RuleFilterKBKMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("InterBudgetaryTransfers_RuleFilterKBK", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdFilterFieldType_BranchCode).HasColumnName("idFilterFieldType_BranchCode");
			this.Property(t => t.IdFilterFieldType_CodeSubsidy).HasColumnName("idFilterFieldType_CodeSubsidy");
			this.Property(t => t.IdFilterFieldType_DEK).HasColumnName("idFilterFieldType_DEK");
			this.Property(t => t.IdFilterFieldType_DFK).HasColumnName("idFilterFieldType_DFK");
			this.Property(t => t.IdFilterFieldType_DKR).HasColumnName("idFilterFieldType_DKR");
			this.Property(t => t.IdFilterFieldType_ExpenseObligationType).HasColumnName("idFilterFieldType_ExpenseObligationType");
			this.Property(t => t.IdFilterFieldType_FinanceSource).HasColumnName("idFilterFieldType_FinanceSource");
			this.Property(t => t.IdFilterFieldType_KCSR).HasColumnName("idFilterFieldType_KCSR");
			this.Property(t => t.IdFilterFieldType_KFO).HasColumnName("idFilterFieldType_KFO");
			this.Property(t => t.IdFilterFieldType_KOSGU).HasColumnName("idFilterFieldType_KOSGU");
			this.Property(t => t.IdFilterFieldType_KVR).HasColumnName("idFilterFieldType_KVR");
			this.Property(t => t.IdFilterFieldType_KVSR).HasColumnName("idFilterFieldType_KVSR");
			this.Property(t => t.IdFilterFieldType_RZPR).HasColumnName("idFilterFieldType_RZPR");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RuleFilterKBKs).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.InterBudgetaryTransfers_RuleFilterKBK).HasForeignKey(d => d.IdMaster);
			this.HasMany(t => t.BranchCodes).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idBranchCode").ToTable("IBT_RuleFilterKBK_BranchCodes", "ml"));
			this.HasMany(t => t.CodeSubsidys).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idCodeSubsidy").ToTable("IBT_RuleFilterKBK_CodeSubsidys", "ml"));
			this.HasMany(t => t.DEKs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idDEK").ToTable("IBT_RuleFilterKBK_DEK", "ml"));
			this.HasMany(t => t.DFKs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idDFK").ToTable("IBT_RuleFilterKBK_DFK", "ml"));
			this.HasMany(t => t.DKRs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idDKR").ToTable("IBT_RuleFilterKBK_DKR", "ml"));
			this.HasMany(t => t.FinanceSources).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idFinanceSource").ToTable("IBT_RuleFilterKBK_FinanceSources", "ml"));
			this.HasMany(t => t.KCSRs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idKCSR").ToTable("IBT_RuleFilterKBK_KCSRs", "ml"));
			this.HasMany(t => t.KFOs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idKFO").ToTable("IBT_RuleFilterKBK_KFOs", "ml"));
			this.HasMany(t => t.KOSGUs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idKOSGU").ToTable("IBT_RuleFilterKBK_KOSGUs", "ml"));
			this.HasMany(t => t.KVRs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idKVR").ToTable("IBT_RuleFilterKBK_KVRs", "ml"));
			this.HasMany(t => t.KVSRs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idKVSR").ToTable("IBT_RuleFilterKBK_KVSRs", "ml"));
			this.HasMany(t => t.RZPRs).WithMany().Map(m => m.MapLeftKey("idInterBudgetaryTransfers_RuleFilterKBK").MapRightKey("idRZPR").ToTable("IBT_RuleFilterKBK_RZPRs", "ml"));
			
        }
    }
}
