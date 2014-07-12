using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности GoalTarget
    /// </summary>
	public class GoalTargetMap : EntityTypeConfiguration<GoalTarget>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public GoalTargetMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("GoalTarget", "reg");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.IdSystemGoalElement).HasColumnName("idSystemGoalElement");
			this.Property(t => t.IdGoalIndicator).HasColumnName("idGoalIndicator");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IdApproved).HasColumnName("idApproved");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.IdTerminator).HasColumnName("idTerminator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.IdTerminatorEntity).HasColumnName("idTerminatorEntity");
			this.Property(t => t.IdApprovedEntity).HasColumnName("idApprovedEntity");
			this.Property(t => t.DateCreate).HasColumnName("DateCreate");
			this.Property(t => t.IdTerminateOperation).HasColumnName("IdTerminateOperation");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.SystemGoalElement).WithMany().HasForeignKey(d => d.IdSystemGoalElement);
			this.HasRequired(t => t.GoalIndicator).WithMany().HasForeignKey(d => d.IdGoalIndicator);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasOptional(t => t.TerminatorEntity).WithMany().HasForeignKey(d => d.IdTerminatorEntity);
			this.HasOptional(t => t.ApprovedEntity).WithMany().HasForeignKey(d => d.IdApprovedEntity);
			this.HasOptional(t => t.TerminateOperation).WithMany().HasForeignKey(d => d.IdTerminateOperation);
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			
        }
    }
}
