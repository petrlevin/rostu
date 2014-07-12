using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности AttributeOfSystemGoalElement
    /// </summary>
	public class AttributeOfSystemGoalElementMap : EntityTypeConfiguration<AttributeOfSystemGoalElement>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public AttributeOfSystemGoalElementMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AttributeOfSystemGoalElement", "reg");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdTerminator).HasColumnName("idTerminator");
			this.Property(t => t.IdApproved).HasColumnName("idApproved");
			this.Property(t => t.IdApprovedEntity).HasColumnName("idApprovedEntity");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.IdTerminatorEntity).HasColumnName("idTerminatorEntity");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			this.Property(t => t.IdTerminateOperation).HasColumnName("idTerminateOperation");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.IdSystemGoalElement).HasColumnName("idSystemGoalElement");
			this.Property(t => t.IdSystemGoalElement_Parent).HasColumnName("idSystemGoalElement_Parent");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdElementTypeSystemGoal).HasColumnName("idElementTypeSystemGoal");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.DateCreate).HasColumnName("DateCreate");
			
            // Relationships
			this.HasOptional(t => t.ApprovedEntity).WithMany().HasForeignKey(d => d.IdApprovedEntity);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasOptional(t => t.TerminatorEntity).WithMany().HasForeignKey(d => d.IdTerminatorEntity);
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			this.HasOptional(t => t.TerminateOperation).WithMany().HasForeignKey(d => d.IdTerminateOperation);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.SystemGoalElement).WithMany().HasForeignKey(d => d.IdSystemGoalElement);
			this.HasOptional(t => t.SystemGoalElement_Parent).WithMany().HasForeignKey(d => d.IdSystemGoalElement_Parent);
			this.HasOptional(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.ElementTypeSystemGoal).WithMany().HasForeignKey(d => d.IdElementTypeSystemGoal);
			
        }
    }
}
