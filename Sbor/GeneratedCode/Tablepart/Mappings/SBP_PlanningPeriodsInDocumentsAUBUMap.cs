using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности SBP_PlanningPeriodsInDocumentsAUBU
    /// </summary>
	public class SBP_PlanningPeriodsInDocumentsAUBUMap : EntityTypeConfiguration<SBP_PlanningPeriodsInDocumentsAUBU>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public SBP_PlanningPeriodsInDocumentsAUBUMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SBP_PlanningPeriodsInDocumentsAUBU", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdDocAUBUPeriodType_OFG).HasColumnName("idDocAUBUPeriodType_OFG");
			this.Property(t => t.IdDocAUBUPeriodType_PFG1).HasColumnName("idDocAUBUPeriodType_PFG1");
			this.Property(t => t.IdDocAUBUPeriodType_PFG2).HasColumnName("idDocAUBUPeriodType_PFG2");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SBP_PlanningPeriodsInDocumentsAUBU).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			
        }
    }
}
