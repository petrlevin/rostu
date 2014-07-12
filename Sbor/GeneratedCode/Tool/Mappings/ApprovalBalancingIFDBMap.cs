using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tool.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ApprovalBalancingIFDB
    /// </summary>
	public class ApprovalBalancingIFDBMap : EntityTypeConfiguration<ApprovalBalancingIFDB>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ApprovalBalancingIFDBMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ApprovalBalancingIFDB", "tool");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.IdSourcesDataTools).HasColumnName("idSourcesDataTools");
			this.Property(t => t.Number).HasColumnName("Number");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasMany(t => t.BalancingIFDBs).WithMany().Map(m => m.MapLeftKey("idApprovalBalancingIFDB").MapRightKey("idBalancingIFDB").ToTable("ApprovalBalancingIFDB_BalancingIFDB", "ml"));
			this.HasMany(t => t.LimitBudgetAllocations).WithMany().Map(m => m.MapLeftKey("idApprovalBalancingIFDB").MapRightKey("idLimitBudgetAllocations").ToTable("ApprovalBalancingIFDB_LimitBudgetAllocations", "ml"));
			this.HasMany(t => t.ActivityOfSBPs).WithMany().Map(m => m.MapLeftKey("idApprovalBalancingIFDB").MapRightKey("idActivityOfSBP").ToTable("ApprovalBalancingIFDB_ActivityOfSBP", "ml"));
			
        }
    }
}
