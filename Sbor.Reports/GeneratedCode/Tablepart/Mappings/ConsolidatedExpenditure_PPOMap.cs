using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ConsolidatedExpenditure_PPO
    /// </summary>
	public class ConsolidatedExpenditure_PPOMap : EntityTypeConfiguration<ConsolidatedExpenditure_PPO>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ConsolidatedExpenditure_PPOMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ConsolidatedExpenditure_PPO", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.PublicLegalFormations).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			
        }
    }
}
