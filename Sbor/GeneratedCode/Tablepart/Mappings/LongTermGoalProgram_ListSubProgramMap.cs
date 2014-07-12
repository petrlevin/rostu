using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LongTermGoalProgram_ListSubProgram
    /// </summary>
	public class LongTermGoalProgram_ListSubProgramMap : EntityTypeConfiguration<LongTermGoalProgram_ListSubProgram>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LongTermGoalProgram_ListSubProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LongTermGoalProgram_ListSubProgram", "tp");
			this.Property(t => t.IdActualDocStatus).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idActualDocStatus");
			this.Property(t => t.IdActualDocument).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("idActualDocument");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdAnalyticalCodeStateProgram).HasColumnName("idAnalyticalCodeStateProgram");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdResponsibleExecutantType).HasColumnName("idResponsibleExecutantType");
			this.Property(t => t.IdSystemGoal).HasColumnName("idSystemGoal");
			this.Property(t => t.DateStart).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DateEnd");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdDocument).HasColumnName("idDocument");
			this.Property(t => t.IdDocumentEntity).HasColumnName("idDocumentEntity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ListSubProgram).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.AnalyticalCodeStateProgram).WithMany().HasForeignKey(d => d.IdAnalyticalCodeStateProgram);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.ResponsibleExecutantType).WithMany().HasForeignKey(d => d.IdResponsibleExecutantType);
			this.HasRequired(t => t.SystemGoal).WithMany().HasForeignKey(d => d.IdSystemGoal);
			this.HasOptional(t => t.DocumentEntity).WithMany().HasForeignKey(d => d.IdDocumentEntity);
			
        }
    }
}
