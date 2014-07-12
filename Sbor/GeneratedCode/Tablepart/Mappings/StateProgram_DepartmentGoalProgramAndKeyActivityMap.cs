using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности StateProgram_DepartmentGoalProgramAndKeyActivity
    /// </summary>
	public class StateProgram_DepartmentGoalProgramAndKeyActivityMap : EntityTypeConfiguration<StateProgram_DepartmentGoalProgramAndKeyActivity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public StateProgram_DepartmentGoalProgramAndKeyActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("StateProgram_DepartmentGoalProgramAndKeyActivity", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdAnalyticalCodeStateProgram).HasColumnName("idAnalyticalCodeStateProgram");
			this.Property(t => t.IdDocType).HasColumnName("idDocType");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdResponsibleExecutantType).HasColumnName("idResponsibleExecutantType");
			this.Property(t => t.IdSystemGoal).HasColumnName("idSystemGoal");
			this.Property(t => t.DateStart).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DateEnd");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdDocument).HasColumnName("idDocument");
			this.Property(t => t.IdDocumentEntity).HasColumnName("idDocumentEntity");
			this.Property(t => t.IdActiveDocStatus).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idActiveDocStatus");
			this.Property(t => t.IdActiveDocument).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idActiveDocument");
			this.Property(t => t.ActiveDocumentCaption).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("ActiveDocumentCaption");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.DepartmentGoalProgramAndKeyActivity).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.AnalyticalCodeStateProgram).WithMany().HasForeignKey(d => d.IdAnalyticalCodeStateProgram);
			this.HasRequired(t => t.DocType).WithMany().HasForeignKey(d => d.IdDocType);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.ResponsibleExecutantType).WithMany().HasForeignKey(d => d.IdResponsibleExecutantType);
			this.HasRequired(t => t.SystemGoal).WithMany().HasForeignKey(d => d.IdSystemGoal);
			this.HasOptional(t => t.DocumentEntity).WithMany().HasForeignKey(d => d.IdDocumentEntity);
			
        }
    }
}
