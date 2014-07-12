using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_RequirementsForTheTask
    /// </summary>
	public class PlanActivity_RequirementsForTheTaskMap : EntityTypeConfiguration<PlanActivity_RequirementsForTheTask>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_RequirementsForTheTaskMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_RequirementsForTheTask", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdActivityType).HasColumnName("idActivityType");
			this.Property(t => t.ReasonTerminationTask).HasColumnName("ReasonTerminationTask");
			this.Property(t => t.DatesReportingOnExecutionTask).HasColumnName("DatesReportingOnExecutionTask");
			this.Property(t => t.OtherRequirementsOnExecutionTask).HasColumnName("OtherRequirementsOnExecutionTask");
			this.Property(t => t.AnyOtherInformationOnExecutionTask).HasColumnName("AnyOtherInformationOnExecutionTask");
			this.Property(t => t.GroundsSuspendTasks).HasColumnName("GroundsSuspendTasks");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.RequirementsForTheTasks).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
