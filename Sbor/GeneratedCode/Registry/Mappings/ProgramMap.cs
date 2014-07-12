using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Program
    /// </summary>
	public class ProgramMap : EntityTypeConfiguration<Program>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Program", "reg");
			this.Property(t => t.Caption).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("Caption");
			this.Property(t => t.IdTerminator).HasColumnName("idTerminator");
			this.Property(t => t.IdTerminatorEntity).HasColumnName("idTerminatorEntity");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.IdApproved).HasColumnName("idApproved");
			this.Property(t => t.IdApprovedEntity).HasColumnName("idApprovedEntity");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			this.Property(t => t.IdTerminateOperation).HasColumnName("idTerminateOperation");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.DateCreate).HasColumnName("DateCreate");
			this.Property(t => t.IdDocType).HasColumnName("idDocType");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			
            // Relationships
			this.HasOptional(t => t.TerminatorEntity).WithMany().HasForeignKey(d => d.IdTerminatorEntity);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasOptional(t => t.ApprovedEntity).WithMany().HasForeignKey(d => d.IdApprovedEntity);
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			this.HasOptional(t => t.TerminateOperation).WithMany().HasForeignKey(d => d.IdTerminateOperation);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.DocType).WithMany().HasForeignKey(d => d.IdDocType);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			
        }
    }
}
