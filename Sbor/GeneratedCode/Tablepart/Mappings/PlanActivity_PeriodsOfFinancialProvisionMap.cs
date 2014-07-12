using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_PeriodsOfFinancialProvision
    /// </summary>
	public class PlanActivity_PeriodsOfFinancialProvisionMap : EntityTypeConfiguration<PlanActivity_PeriodsOfFinancialProvision>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_PeriodsOfFinancialProvisionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_PeriodsOfFinancialProvision", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(18,2).HasColumnName("Value");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.AdditionalValue).HasPrecision(18,2).HasColumnName("AdditionalValue");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.PeriodsOfFinancialProvisions).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Master).WithMany(t => t.PlanActivity_PeriodsOfFinancialProvision).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
