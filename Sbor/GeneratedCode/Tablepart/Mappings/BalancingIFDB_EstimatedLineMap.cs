using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_EstimatedLine
    /// </summary>
	public class BalancingIFDB_EstimatedLineMap : EntityTypeConfiguration<BalancingIFDB_EstimatedLine>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_EstimatedLineMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_EstimatedLine", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdEstimatedLine).HasColumnName("idEstimatedLine");
			this.Property(t => t.OldValue).HasPrecision(22,2).HasColumnName("OldValue");
			this.Property(t => t.NewValue).HasPrecision(22,2).HasColumnName("NewValue");
			this.Property(t => t.IdTaskCollection).HasColumnName("idTaskCollection");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.IsAdditionalNeed).HasColumnName("isAdditionalNeed");
			this.Property(t => t.IdOKATO).HasColumnName("idOKATO");
			this.Property(t => t.IdAuthorityOfExpenseObligation).HasColumnName("idAuthorityOfExpenseObligation");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.EstimatedLines).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.BalancingIFDB_EstimatedLine).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.EstimatedLine).WithMany().HasForeignKey(d => d.IdEstimatedLine);
			this.HasRequired(t => t.TaskCollection).WithMany().HasForeignKey(d => d.IdTaskCollection);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasOptional(t => t.OKATO).WithMany().HasForeignKey(d => d.IdOKATO);
			this.HasOptional(t => t.AuthorityOfExpenseObligation).WithMany().HasForeignKey(d => d.IdAuthorityOfExpenseObligation);
			
        }
    }
}
