using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FinancialIndicator
    /// </summary>
	public class FinancialIndicatorMap : EntityTypeConfiguration<FinancialIndicator>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FinancialIndicatorMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FinancialIndicator", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.RowCode).HasColumnName("RowCode");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
