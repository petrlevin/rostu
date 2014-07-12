using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Activity
    /// </summary>
	public class ActivityMap : EntityTypeConfiguration<Activity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Activity", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Code).HasColumnName("Code");
			this.Property(t => t.IdActivityType).HasColumnName("idActivityType");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.FullCaption).HasColumnName("FullCaption");
			this.Property(t => t.OrganSetPrice).HasColumnName("OrganSetPrice");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.IdPaidType).HasColumnName("idPaidType");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasMany(t => t.Activity_SBP).WithMany().Map(m => m.MapLeftKey("idActivity").MapRightKey("idSBP").ToTable("Activity_SBP", "ml"));
			this.HasMany(t => t.Activity_Contingent).WithMany(r => r.Activity_Contingent).Map(m => m.MapLeftKey("idActivity").MapRightKey("idContingent").ToTable("Activity_Contingent", "ml"));
			
        }
    }
}
