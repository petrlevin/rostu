using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LimitBudgetAllocations_ControlRelation
    /// </summary>
	public class LimitBudgetAllocations_ControlRelationMap : EntityTypeConfiguration<LimitBudgetAllocations_ControlRelation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LimitBudgetAllocations_ControlRelationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LimitBudgetAllocations_ControlRelation", "tp");
			this.Property(t => t.IdBranchCode).HasColumnName("idBranchCode");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdExpenseObligationType).HasColumnName("idExpenseObligationType");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			this.Property(t => t.IdKVSR).HasColumnName("idKVSR");
			this.Property(t => t.IdKFO).HasColumnName("idKFO");
			this.Property(t => t.IdRZPR).HasColumnName("idRZPR");
			this.Property(t => t.IdKCSR).HasColumnName("idKCSR");
			this.Property(t => t.IdKVR).HasColumnName("idKVR");
			this.Property(t => t.IdKOSGU).HasColumnName("idKOSGU");
			this.Property(t => t.IdDFK).HasColumnName("idDFK");
			this.Property(t => t.IdDKR).HasColumnName("idDKR");
			this.Property(t => t.IdDEK).HasColumnName("idDEK");
			this.Property(t => t.IdCodeSubsidy).HasColumnName("idCodeSubsidy");
			this.Property(t => t.Year).HasColumnName("Year");
			this.Property(t => t.DiffAllocations).HasPrecision(18,2).HasColumnName("DiffAllocations");
			this.Property(t => t.UnallocatedAllocations).HasPrecision(18,2).HasColumnName("UnallocatedAllocations");
			this.Property(t => t.TotalDocumentAllocations).HasPrecision(18,2).HasColumnName("TotalDocumentAllocations");
			this.Property(t => t.AllocatedAllocations).HasPrecision(18,2).HasColumnName("AllocatedAllocations");
			this.Property(t => t.WithCompanyAllocations).HasPrecision(18,2).HasColumnName("WithCompanyAllocations");
			this.Property(t => t.PlanGRBSAllocations).HasPrecision(18,2).HasColumnName("PlanGRBSAllocations");
			
            // Relationships
			this.HasOptional(t => t.BranchCode).WithMany().HasForeignKey(d => d.IdBranchCode);
			this.HasRequired(t => t.Owner).WithMany(t => t.ControlRelation).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			this.HasOptional(t => t.KVSR).WithMany().HasForeignKey(d => d.IdKVSR);
			this.HasOptional(t => t.KFO).WithMany().HasForeignKey(d => d.IdKFO);
			this.HasOptional(t => t.RZPR).WithMany().HasForeignKey(d => d.IdRZPR);
			this.HasOptional(t => t.KCSR).WithMany().HasForeignKey(d => d.IdKCSR);
			this.HasOptional(t => t.KVR).WithMany().HasForeignKey(d => d.IdKVR);
			this.HasOptional(t => t.KOSGU).WithMany().HasForeignKey(d => d.IdKOSGU);
			this.HasOptional(t => t.DFK).WithMany().HasForeignKey(d => d.IdDFK);
			this.HasOptional(t => t.DKR).WithMany().HasForeignKey(d => d.IdDKR);
			this.HasOptional(t => t.DEK).WithMany().HasForeignKey(d => d.IdDEK);
			this.HasOptional(t => t.CodeSubsidy).WithMany().HasForeignKey(d => d.IdCodeSubsidy);
			
        }
    }
}
