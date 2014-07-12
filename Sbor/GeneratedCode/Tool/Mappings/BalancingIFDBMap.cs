using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tool.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB
    /// </summary>
	public class BalancingIFDBMap : EntityTypeConfiguration<BalancingIFDB>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDBMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB", "tool");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdSourcesDataTools).HasColumnName("idSourcesDataTools");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.IdBalancingIFDBType).HasColumnName("idBalancingIFDBType");
			this.Property(t => t.IdBalanceConfig_FilterRule).HasColumnName("idBalanceConfig_FilterRule");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasRequired(t => t.BalanceConfig_FilterRule).WithMany().HasForeignKey(d => d.IdBalanceConfig_FilterRule);
			this.HasRequired(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			
        }
    }
}
