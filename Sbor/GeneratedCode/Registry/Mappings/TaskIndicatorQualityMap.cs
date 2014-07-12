using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TaskIndicatorQuality
    /// </summary>
	public class TaskIndicatorQualityMap : EntityTypeConfiguration<TaskIndicatorQuality>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TaskIndicatorQualityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TaskIndicatorQuality", "reg");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			this.Property(t => t.IdTerminateOperation).HasColumnName("idTerminateOperation");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdProgram).HasColumnName("idProgram");
			this.Property(t => t.IdTaskCollection).HasColumnName("idTaskCollection");
			this.Property(t => t.IdIndicatorActivity_Quality).HasColumnName("idIndicatorActivity_Quality");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.IdValueType).HasColumnName("idValueType");
			this.Property(t => t.Value).HasPrecision(20,5).HasColumnName("Value");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IdApproved).HasColumnName("idApproved");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.IdTerminator).HasColumnName("idTerminator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.IdApprovedEntity).HasColumnName("idApprovedEntity");
			this.Property(t => t.IdTerminatorEntity).HasColumnName("idTerminatorEntity");
			this.Property(t => t.IsAdditionalNeed).HasColumnName("IsAdditionalNeed");
			this.Property(t => t.DateCreate).HasColumnName("DateCreate");
			this.Property(t => t.ActivityAUBU).HasColumnName("ActivityAUBU");
			
            // Relationships
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			this.HasOptional(t => t.TerminateOperation).WithMany().HasForeignKey(d => d.IdTerminateOperation);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasOptional(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.Program).WithMany().HasForeignKey(d => d.IdProgram);
			this.HasRequired(t => t.TaskCollection).WithMany().HasForeignKey(d => d.IdTaskCollection);
			this.HasRequired(t => t.IndicatorActivity_Quality).WithMany().HasForeignKey(d => d.IdIndicatorActivity_Quality);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasOptional(t => t.ApprovedEntity).WithMany().HasForeignKey(d => d.IdApprovedEntity);
			this.HasOptional(t => t.TerminatorEntity).WithMany().HasForeignKey(d => d.IdTerminatorEntity);
			
        }
    }
}
