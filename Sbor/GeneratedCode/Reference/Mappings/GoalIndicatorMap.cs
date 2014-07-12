using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности GoalIndicator
    /// </summary>
	public class GoalIndicatorMap : EntityTypeConfiguration<GoalIndicator>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public GoalIndicatorMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("GoalIndicator", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.IdTermsOfPerception).HasColumnName("idTermsOfPerception");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.IdCalculationFormula).HasColumnName("idCalculationFormula");
			this.Property(t => t.IdRegulatoryAct).HasColumnName("idRegulatoryAct");
			this.Property(t => t.IndicatedCumulatively).HasColumnName("IndicatedCumulatively");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.CalculationFormula).WithMany().HasForeignKey(d => d.IdCalculationFormula);
			this.HasOptional(t => t.RegulatoryAct).WithMany().HasForeignKey(d => d.IdRegulatoryAct);
			
        }
    }
}
