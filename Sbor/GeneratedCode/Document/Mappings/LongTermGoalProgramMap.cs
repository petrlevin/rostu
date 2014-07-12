using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Document.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LongTermGoalProgram
    /// </summary>
	public class LongTermGoalProgramMap : EntityTypeConfiguration<LongTermGoalProgram>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LongTermGoalProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LongTermGoalProgram", "doc");
			this.Property(t => t.DateLastEdit).HasColumnName("DateLastEdit");
			this.Property(t => t.IdDocStatus).HasColumnName("idDocStatus");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdDocType).HasColumnName("idDocType");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IdAnalyticalCodeStateProgram).HasColumnName("idAnalyticalCodeStateProgram");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IsApproved).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("isApproved");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdResponsibleExecutantType).HasColumnName("idResponsibleExecutantType");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.HasMasterDoc).HasColumnName("HasMasterDoc");
			this.Property(t => t.IdMasterStateProgram).HasColumnName("idMasterStateProgram");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IsRequireClarification).HasColumnName("isRequireClarification");
			this.Property(t => t.ReasonClarification).HasColumnName("ReasonClarification");
			this.Property(t => t.DateTerminate).HasColumnName("DateTerminate");
			this.Property(t => t.ReasonTerminate).HasColumnName("ReasonTerminate");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.ReasonCancel).HasColumnName("ReasonCancel");
			this.Property(t => t.IdMasterLongTermGoalProgram).HasColumnName("idMasterLongTermGoalProgram");
			this.Property(t => t.Header).HasColumnName("Header");
			this.Property(t => t.HasAdditionalNeed).HasColumnName("HasAdditionalNeed");
			
            // Relationships
			this.HasRequired(t => t.DocStatus).WithMany().HasForeignKey(d => d.IdDocStatus);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.DocType).WithMany().HasForeignKey(d => d.IdDocType);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasOptional(t => t.AnalyticalCodeStateProgram).WithMany().HasForeignKey(d => d.IdAnalyticalCodeStateProgram);
			this.HasRequired(t => t.ResponsibleExecutantType).WithMany().HasForeignKey(d => d.IdResponsibleExecutantType);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasOptional(t => t.MasterStateProgram).WithMany().HasForeignKey(d => d.IdMasterStateProgram);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasOptional(t => t.MasterLongTermGoalProgram).WithMany(t => t.ChildrenByidMasterLongTermGoalProgram).HasForeignKey(d => d.IdMasterLongTermGoalProgram);
			
        }
    }
}
