using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ExpenditureObligations_RegulatoryAct
    /// </summary>
	public class ExpenditureObligations_RegulatoryActMap : EntityTypeConfiguration<ExpenditureObligations_RegulatoryAct>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ExpenditureObligations_RegulatoryActMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ExpenditureObligations_RegulatoryAct", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdRegulatoryAct).HasColumnName("idRegulatoryAct");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RegulatoryAct).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.RegulatoryAct).WithMany().HasForeignKey(d => d.IdRegulatoryAct);
			
        }
    }
}
