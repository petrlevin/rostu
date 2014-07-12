using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_Activity
    /// </summary>
	public class PlanActivity_ActivityMap : EntityTypeConfiguration<PlanActivity_Activity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_Activity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			this.Property(t => t.IdIndicatorActivity).HasColumnName("idIndicatorActivity");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdActivityOfSBP).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idActivityOfSBP");
			this.Property(t => t.IdActivityOfSBP_A).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idActivityOfSBP_A");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Activity).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			this.HasRequired(t => t.IndicatorActivity).WithMany().HasForeignKey(d => d.IdIndicatorActivity);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			
        }
    }
}
