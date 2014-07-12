using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PlanActivity_OrderOfControlTheExecutionTasks
    /// </summary>
	public class PlanActivity_OrderOfControlTheExecutionTasksMap : EntityTypeConfiguration<PlanActivity_OrderOfControlTheExecutionTasks>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PlanActivity_OrderOfControlTheExecutionTasksMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PlanActivity_OrderOfControlTheExecutionTasks", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.FormOfControl).HasColumnName("FormOfControl");
			this.Property(t => t.Periodicity).HasColumnName("Periodicity");
			this.Property(t => t.OrgansOfExecutiveAuthoritiesInCharge).HasColumnName("OrgansOfExecutiveAuthoritiesInCharge");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.OrderOfControlTheExecutionTaskss).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.PlanActivity_OrderOfControlTheExecutionTasks).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
