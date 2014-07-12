using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_PlannedVolumeIncome
    /// </summary>
	public class FBA_PlannedVolumeIncomeMap : EntityTypeConfiguration<FBA_PlannedVolumeIncome>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_PlannedVolumeIncomeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_PlannedVolumeIncome", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdFBA_Activity).HasColumnName("idFBA_Activity");
			this.Property(t => t.IdFinanceSource).HasColumnName("idFinanceSource");
			this.Property(t => t.IdKFO).HasColumnName("idKFO");
			this.Property(t => t.IdCodeSubsidy).HasColumnName("idCodeSubsidy");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.PlannedVolumeIncomes).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.FBA_Activity).WithMany(t => t.FBA_PlannedVolumeIncome).HasForeignKey(d => d.IdFBA_Activity);
			this.HasRequired(t => t.FinanceSource).WithMany().HasForeignKey(d => d.IdFinanceSource);
			this.HasRequired(t => t.KFO).WithMany().HasForeignKey(d => d.IdKFO);
			this.HasOptional(t => t.CodeSubsidy).WithMany().HasForeignKey(d => d.IdCodeSubsidy);
			
        }
    }
}
