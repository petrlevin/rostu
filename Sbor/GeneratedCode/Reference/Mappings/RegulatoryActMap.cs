using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegulatoryAct
    /// </summary>
	public class RegulatoryActMap : EntityTypeConfiguration<RegulatoryAct>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegulatoryActMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegulatoryAct", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudgetLevel).HasColumnName("idBudgetLevel");
			this.Property(t => t.IdTypeRegulatoryAct).HasColumnName("idTypeRegulatoryAct");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.AuthorityRegulatoryAct).HasColumnName("AuthorityRegulatoryAct");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.BudgetLevel).WithMany().HasForeignKey(d => d.IdBudgetLevel);
			this.HasRequired(t => t.TypeRegulatoryAct).WithMany().HasForeignKey(d => d.IdTypeRegulatoryAct);
			
        }
    }
}
