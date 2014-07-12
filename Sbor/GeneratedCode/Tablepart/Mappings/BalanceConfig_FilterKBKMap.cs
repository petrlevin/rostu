using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalanceConfig_FilterKBK
    /// </summary>
	public class BalanceConfig_FilterKBKMap : EntityTypeConfiguration<BalanceConfig_FilterKBK>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalanceConfig_FilterKBKMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalanceConfig_FilterKBK", "tp");
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
			this.HasRequired(t => t.Owner).WithMany(t => t.FilterKBKs).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.BalanceConfig_FilterKBK).HasForeignKey(d => d.IdMaster);
			this.HasMany(t => t.FinanceSources).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idFinanceSource").ToTable("BalanceConfig_FilterKBK_FinanceSource", "ml"));
			this.HasMany(t => t.KFOs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idKFO").ToTable("BalanceConfig_FilterKBK_KFO", "ml"));
			this.HasMany(t => t.KVSRs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idKVSR").ToTable("BalanceConfig_FilterKBK_KVSR", "ml"));
			this.HasMany(t => t.RZPRs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idRZPR").ToTable("BalanceConfig_FilterKBK_RZPR", "ml"));
			this.HasMany(t => t.KCSRs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idKCSR").ToTable("BalanceConfig_FilterKBK_KCSR", "ml"));
			this.HasMany(t => t.KVRs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idKVR").ToTable("BalanceConfig_FilterKBK_KVR", "ml"));
			this.HasMany(t => t.KOSGUs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idKOSGU").ToTable("BalanceConfig_FilterKBK_KOSGU", "ml"));
			this.HasMany(t => t.DFKs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idDFK").ToTable("BalanceConfig_FilterKBK_DFK", "ml"));
			this.HasMany(t => t.DKRs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idDKR").ToTable("BalanceConfig_FilterKBK_DKR", "ml"));
			this.HasMany(t => t.DEKs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idDEK").ToTable("BalanceConfig_FilterKBK_DEK", "ml"));
			this.HasMany(t => t.CodeSubsidys).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idCodeSubsidy").ToTable("BalanceConfig_FilterKBK_CodeSubsidy", "ml"));
			this.HasMany(t => t.BranchCodes).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idBranchCode").ToTable("BalanceConfig_FilterKBK_BranchCode", "ml"));
			this.HasMany(t => t.OKATOs).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idOKATO").ToTable("BalanceConfig_FilterKBK_OKATO", "ml"));
			this.HasMany(t => t.AuthorityOfExpenseObligations).WithMany().Map(m => m.MapLeftKey("idBalanceConfig_FilterKBK").MapRightKey("idAuthorityOfExpenseObligation").ToTable("BalanceConfig_FilterKBK_AuthorityOfExpenseObligation", "ml"));
			
        }
    }
}
