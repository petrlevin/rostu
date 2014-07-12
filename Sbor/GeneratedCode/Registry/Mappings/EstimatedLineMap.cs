using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности EstimatedLine
    /// </summary>
	public class EstimatedLineMap : EntityTypeConfiguration<EstimatedLine>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public EstimatedLineMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("EstimatedLine", "reg");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.Caption).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("Caption");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdActivityBudgetaryType).HasColumnName("idActivityBudgetaryType");
			this.Property(t => t.IdExpenseObligationType).HasColumnName("idExpenseObligationType");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			this.Property(t => t.IdKFO).HasColumnName("idKFO");
			this.Property(t => t.IdKVSR).HasColumnName("idKVSR");
			this.Property(t => t.IdRZPR).HasColumnName("idRZPR");
			this.Property(t => t.IdKCSR).HasColumnName("idKCSR");
			this.Property(t => t.IdKOSGU).HasColumnName("idKOSGU");
			this.Property(t => t.IdDFK).HasColumnName("idDFK");
			this.Property(t => t.IdDKR).HasColumnName("idDKR");
			this.Property(t => t.IdDEK).HasColumnName("idDEK");
			this.Property(t => t.IdCodeSubsidy).HasColumnName("idCodeSubsidy");
			this.Property(t => t.IdKVR).HasColumnName("idKVR");
			this.Property(t => t.IdBranchCode).HasColumnName("idBranchCode");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			this.HasOptional(t => t.KFO).WithMany().HasForeignKey(d => d.IdKFO);
			this.HasOptional(t => t.KVSR).WithMany().HasForeignKey(d => d.IdKVSR);
			this.HasOptional(t => t.RZPR).WithMany().HasForeignKey(d => d.IdRZPR);
			this.HasOptional(t => t.KCSR).WithMany().HasForeignKey(d => d.IdKCSR);
			this.HasOptional(t => t.KOSGU).WithMany().HasForeignKey(d => d.IdKOSGU);
			this.HasOptional(t => t.DFK).WithMany().HasForeignKey(d => d.IdDFK);
			this.HasOptional(t => t.DKR).WithMany().HasForeignKey(d => d.IdDKR);
			this.HasOptional(t => t.DEK).WithMany().HasForeignKey(d => d.IdDEK);
			this.HasOptional(t => t.CodeSubsidy).WithMany().HasForeignKey(d => d.IdCodeSubsidy);
			this.HasOptional(t => t.KVR).WithMany().HasForeignKey(d => d.IdKVR);
			this.HasOptional(t => t.BranchCode).WithMany().HasForeignKey(d => d.IdBranchCode);
			
        }
    }
}
