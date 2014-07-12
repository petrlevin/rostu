using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_Expense
    /// </summary>
	public class BalancingIFDB_ExpenseMap : EntityTypeConfiguration<BalancingIFDB_Expense>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_ExpenseMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_Expense", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdExpenseObligationType).HasColumnName("idExpenseObligationType");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			this.Property(t => t.IdKFO).HasColumnName("idKFO");
			this.Property(t => t.IdKVSR).HasColumnName("idKVSR");
			this.Property(t => t.IdRZPR).HasColumnName("idRZPR");
			this.Property(t => t.IdKCSR).HasColumnName("idKCSR");
			this.Property(t => t.IdKVR).HasColumnName("idKVR");
			this.Property(t => t.IdKOSGU).HasColumnName("idKOSGU");
			this.Property(t => t.IdDFK).HasColumnName("idDFK");
			this.Property(t => t.IdDKR).HasColumnName("idDKR");
			this.Property(t => t.IdDEK).HasColumnName("idDEK");
			this.Property(t => t.IdCodeSubsidy).HasColumnName("idCodeSubsidy");
			this.Property(t => t.IdBranchCode).HasColumnName("idBranchCode");
			this.Property(t => t.OFG).HasPrecision(22,2).HasColumnName("OFG");
			this.Property(t => t.AdditionalOFG).HasPrecision(22,2).HasColumnName("AdditionalOFG");
			this.Property(t => t.ChangeOFG).HasPrecision(22,2).HasColumnName("ChangeOFG");
			this.Property(t => t.ChangeAdditionalOFG).HasPrecision(22,2).HasColumnName("ChangeAdditionalOFG");
			this.Property(t => t.PFG1).HasPrecision(22,2).HasColumnName("PFG1");
			this.Property(t => t.AdditionalPFG1).HasPrecision(22,2).HasColumnName("AdditionalPFG1");
			this.Property(t => t.ChangePFG1).HasPrecision(22,2).HasColumnName("ChangePFG1");
			this.Property(t => t.ChangeAdditionalPFG1).HasPrecision(22,2).HasColumnName("ChangeAdditionalPFG1");
			this.Property(t => t.PFG2).HasPrecision(22,2).HasColumnName("PFG2");
			this.Property(t => t.AdditionalPFG2).HasPrecision(22,2).HasColumnName("AdditionalPFG2");
			this.Property(t => t.ChangePFG2).HasPrecision(22,2).HasColumnName("ChangePFG2");
			this.Property(t => t.ChangeAdditionalPFG2).HasPrecision(22,2).HasColumnName("ChangeAdditionalPFG2");
			this.Property(t => t.DifferenceOFG).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceOFG");
			this.Property(t => t.DifferenceAdditionalOFG).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceAdditionalOFG");
			this.Property(t => t.DifferencePFG1).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferencePFG1");
			this.Property(t => t.DifferenceAdditionalPFG1).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceAdditionalPFG1");
			this.Property(t => t.DifferencePFG2).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferencePFG2");
			this.Property(t => t.DifferenceAdditionalPFG2).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceAdditionalPFG2");
			this.Property(t => t.IdOKATO).HasColumnName("idOKATO");
			this.Property(t => t.IdAuthorityOfExpenseObligation).HasColumnName("idAuthorityOfExpenseObligation");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Expenses).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.BalancingIFDB_Expense).HasForeignKey(d => d.IdMaster);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			this.HasOptional(t => t.KFO).WithMany().HasForeignKey(d => d.IdKFO);
			this.HasOptional(t => t.KVSR).WithMany().HasForeignKey(d => d.IdKVSR);
			this.HasOptional(t => t.RZPR).WithMany().HasForeignKey(d => d.IdRZPR);
			this.HasOptional(t => t.KCSR).WithMany().HasForeignKey(d => d.IdKCSR);
			this.HasOptional(t => t.KVR).WithMany().HasForeignKey(d => d.IdKVR);
			this.HasOptional(t => t.KOSGU).WithMany().HasForeignKey(d => d.IdKOSGU);
			this.HasOptional(t => t.DFK).WithMany().HasForeignKey(d => d.IdDFK);
			this.HasOptional(t => t.DKR).WithMany().HasForeignKey(d => d.IdDKR);
			this.HasOptional(t => t.DEK).WithMany().HasForeignKey(d => d.IdDEK);
			this.HasOptional(t => t.CodeSubsidy).WithMany().HasForeignKey(d => d.IdCodeSubsidy);
			this.HasOptional(t => t.BranchCode).WithMany().HasForeignKey(d => d.IdBranchCode);
			this.HasOptional(t => t.OKATO).WithMany().HasForeignKey(d => d.IdOKATO);
			this.HasOptional(t => t.AuthorityOfExpenseObligation).WithMany().HasForeignKey(d => d.IdAuthorityOfExpenseObligation);
			
        }
    }
}
