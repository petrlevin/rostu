using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ExpenditureObligations_RegulatoryAct_StructuralUnit
    /// </summary>
	public class ExpenditureObligations_RegulatoryAct_StructuralUnitMap : EntityTypeConfiguration<ExpenditureObligations_RegulatoryAct_StructuralUnit>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ExpenditureObligations_RegulatoryAct_StructuralUnitMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ExpenditureObligations_RegulatoryAct_StructuralUnit", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdRegulatoryAct_StructuralUnit).HasColumnName("idRegulatoryAct_StructuralUnit");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RegulatoryAct_StructuralUnit).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.ExpenditureObligations_RegulatoryAct_StructuralUnit).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.RegulatoryAct_StructuralUnit).WithMany(t => t.ExpenditureObligations_RegulatoryAct_StructuralUnit).HasForeignKey(d => d.IdRegulatoryAct_StructuralUnit);
			
        }
    }
}
