using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ApprovalBalancingIFDB_Blank
    /// </summary>
	public class ApprovalBalancingIFDB_BlankMap : EntityTypeConfiguration<ApprovalBalancingIFDB_Blank>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ApprovalBalancingIFDB_BlankMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ApprovalBalancingIFDB_Blank", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdBlankType).HasColumnName("idBlankType");
			this.Property(t => t.IdBlankValueType_ExpenseObligationType).HasColumnName("idBlankValueType_ExpenseObligationType");
			this.Property(t => t.IdBlankValueType_FinanceSource).HasColumnName("idBlankValueType_FinanceSource");
			this.Property(t => t.IdBlankValueType_KFO).HasColumnName("idBlankValueType_KFO");
			this.Property(t => t.IdBlankValueType_KVSR).HasColumnName("idBlankValueType_KVSR");
			this.Property(t => t.IdBlankValueType_RZPR).HasColumnName("idBlankValueType_RZPR");
			this.Property(t => t.IdBlankValueType_KCSR).HasColumnName("idBlankValueType_KCSR");
			this.Property(t => t.IdBlankValueType_KVR).HasColumnName("idBlankValueType_KVR");
			this.Property(t => t.IdBlankValueType_KOSGU).HasColumnName("idBlankValueType_KOSGU");
			this.Property(t => t.IdBlankValueType_DFK).HasColumnName("idBlankValueType_DFK");
			this.Property(t => t.IdBlankValueType_DKR).HasColumnName("idBlankValueType_DKR");
			this.Property(t => t.IdBlankValueType_DEK).HasColumnName("idBlankValueType_DEK");
			this.Property(t => t.IdBlankValueType_CodeSubsidy).HasColumnName("idBlankValueType_CodeSubsidy");
			this.Property(t => t.IdBlankValueType_BranchCode).HasColumnName("idBlankValueType_BranchCode");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Blanks).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
