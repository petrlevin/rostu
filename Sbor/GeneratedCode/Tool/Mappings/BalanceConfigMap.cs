using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tool.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalanceConfig
    /// </summary>
	public class BalanceConfigMap : EntityTypeConfiguration<BalanceConfig>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalanceConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalanceConfig", "tool");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IdSourcesDataTools).HasColumnName("idSourcesDataTools");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.IdBalancingIFDBType).HasColumnName("idBalancingIFDBType");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			
        }
    }
}
