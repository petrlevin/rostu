using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности AttributeOfProgram
    /// </summary>
	public class AttributeOfProgramMap : EntityTypeConfiguration<AttributeOfProgram>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public AttributeOfProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AttributeOfProgram", "reg");
			this.Property(t => t.IdApproved).HasColumnName("idApproved");
			this.Property(t => t.IdApprovedEntity).HasColumnName("idApprovedEntity");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.IdTerminator).HasColumnName("idTerminator");
			this.Property(t => t.IdTerminatorEntity).HasColumnName("idTerminatorEntity");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			this.Property(t => t.IdTerminateOperation).HasColumnName("idTerminateOperation");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.DateCreate).HasColumnName("DateCreate");
			this.Property(t => t.IdProgram).HasColumnName("idProgram");
			this.Property(t => t.IdAnalyticalCodeStateProgram).HasColumnName("idAnalyticalCodeStateProgram");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.IdGoalSystemElement).HasColumnName("idGoalSystemElement");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			
            // Relationships
			this.HasOptional(t => t.ApprovedEntity).WithMany().HasForeignKey(d => d.IdApprovedEntity);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasOptional(t => t.TerminatorEntity).WithMany().HasForeignKey(d => d.IdTerminatorEntity);
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			this.HasOptional(t => t.TerminateOperation).WithMany().HasForeignKey(d => d.IdTerminateOperation);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.Program).WithMany().HasForeignKey(d => d.IdProgram);
			this.HasOptional(t => t.AnalyticalCodeStateProgram).WithMany().HasForeignKey(d => d.IdAnalyticalCodeStateProgram);
			this.HasRequired(t => t.GoalSystemElement).WithMany().HasForeignKey(d => d.IdGoalSystemElement);
			this.HasOptional(t => t.Parent).WithMany().HasForeignKey(d => d.IdParent);
			
        }
    }
}
