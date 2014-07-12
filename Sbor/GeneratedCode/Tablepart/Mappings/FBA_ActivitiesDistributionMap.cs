using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_ActivitiesDistribution
    /// </summary>
	public class FBA_ActivitiesDistributionMap : EntityTypeConfiguration<FBA_ActivitiesDistribution>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_ActivitiesDistributionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_ActivitiesDistribution", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdFBA_Activity).HasColumnName("idFBA_Activity");
			this.Property(t => t.OFG_Direct).HasPrecision(18,2).HasColumnName("OFG_Direct");
			this.Property(t => t.PFG1_Direct).HasPrecision(18,2).HasColumnName("PFG1_Direct");
			this.Property(t => t.PFG2_Direct).HasPrecision(18,2).HasColumnName("PFG2_Direct");
			this.Property(t => t.OFG_Activity).HasPrecision(20,2).HasColumnName("OFG_Activity");
			this.Property(t => t.PFG1_Activity).HasPrecision(20,2).HasColumnName("PFG1_Activity");
			this.Property(t => t.PFG2_Activity).HasPrecision(20,2).HasColumnName("PFG2_Activity");
			this.Property(t => t.FactorOFG).HasColumnName("FactorOFG");
			this.Property(t => t.FactorPFG1).HasColumnName("FactorPFG1");
			this.Property(t => t.FactorPFG2).HasColumnName("FactorPFG2");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivitiesDistributions).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.FBA_ActivitiesDistribution).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.FBA_Activity).WithMany(t => t.FBA_ActivitiesDistribution).HasForeignKey(d => d.IdFBA_Activity);
			
        }
    }
}
