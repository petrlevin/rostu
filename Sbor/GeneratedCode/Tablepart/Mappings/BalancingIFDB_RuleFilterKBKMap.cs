using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_RuleFilterKBK
    /// </summary>
	public class BalancingIFDB_RuleFilterKBKMap : EntityTypeConfiguration<BalancingIFDB_RuleFilterKBK>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_RuleFilterKBKMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_RuleFilterKBK", "tp");
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
			this.Property(t => t.IdFilterFieldType_OKATO).HasColumnName("idFilterFieldType_OKATO");
			this.Property(t => t.IdFilterFieldType_AuthorityOfExpenseObligation).HasColumnName("idFilterFieldType_AuthorityOfExpenseObligation");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RuleFilterKBKs).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.BalancingIFDB_RuleFilterKBK).HasForeignKey(d => d.IdMaster);
			this.HasMany(t => t.FinanceSources).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idFinanceSource").ToTable("BalancingIFDB_RuleFilterKBK_FinanceSource", "ml"));
			this.HasMany(t => t.KFOs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idKFO").ToTable("BalancingIFDB_RuleFilterKBK_KFO", "ml"));
			this.HasMany(t => t.KVSRs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idKVSR").ToTable("BalancingIFDB_RuleFilterKBK_KVSR", "ml"));
			this.HasMany(t => t.RZPRs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idRZPR").ToTable("BalancingIFDB_RuleFilterKBK_RZPR", "ml"));
			this.HasMany(t => t.KCSRs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idKCSR").ToTable("BalancingIFDB_RuleFilterKBK_KCSR", "ml"));
			this.HasMany(t => t.KVRs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idKVR").ToTable("BalancingIFDB_RuleFilterKBK_KVR", "ml"));
			this.HasMany(t => t.KOSGUs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idKOSGU").ToTable("BalancingIFDB_RuleFilterKBK_KOSGU", "ml"));
			this.HasMany(t => t.DFKs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idDFK").ToTable("BalancingIFDB_RuleFilterKBK_DFK", "ml"));
			this.HasMany(t => t.DKRs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idDKR").ToTable("BalancingIFDB_RuleFilterKBK_DKR", "ml"));
			this.HasMany(t => t.DEKs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idDEK").ToTable("BalancingIFDB_RuleFilterKBK_DEK", "ml"));
			this.HasMany(t => t.CodeSubsidys).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idCodeSubsidy").ToTable("BalancingIFDB_RuleFilterKBK_CodeSubsidy", "ml"));
			this.HasMany(t => t.BranchCodes).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idBranchCode").ToTable("BalancingIFDB_RuleFilterKBK_BranchCode", "ml"));
			this.HasMany(t => t.OKATOs).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idOKATO").ToTable("BalancingIFDB_RuleFilterKBK_OKATO", "ml"));
			this.HasMany(t => t.AuthorityOfExpenseObligations).WithMany().Map(m => m.MapLeftKey("idBalancingIFDB_RuleFilterKBK").MapRightKey("idAuthorityOfExpenseObligation").ToTable("BalancingIFDB_RuleFilterKBK_AuthorityOfExpenseObligation", "ml"));
			
        }
    }
}
