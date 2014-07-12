using System.Data.Entity.ModelConfiguration;

namespace Sbor.Tablepart.Mappings
{
    public class StateProgram_GoalSystemElementMap : EntityTypeConfiguration<tpStateProgram_GoalSystemElement>
    {
        public StateProgram_GoalSystemElementMap()
        {
            // Primary Key
            this.HasKey(t =>  t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("StateProgram_GoalSystemElement", "tp");
			            this.Property(t => t.IdOwner).HasColumnName("idOwner");
			            this.Property(t => t.Id).HasColumnName("id");
			            this.Property(t => t.FromAnotherDocumentSE).HasColumnName("FromAnotherDocumentSE");
			            this.Property(t => t.IdSystemGoal).HasColumnName("idSystemGoal");
			            this.Property(t => t.IsMainGoal).HasColumnName("IsMainGoal");
			            this.Property(t => t.IdGoalIndicator).HasColumnName("idGoalIndicator");
			            this.Property(t => t.Code).HasColumnName("Code");
			            this.Property(t => t.IdParentSystemGoal).HasColumnName("idParentSystemGoal");
			            this.Property(t => t.IdElementTypeSystemGoal).HasColumnName("idElementTypeSystemGoal");
			            this.Property(t => t.DateStart).HasColumnName("DateStart");
			            this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			            this.Property(t => t.IdSBP).HasColumnName("idSBP");
			
            // Relationships
			            this.HasRequired(t => t.Owner)
                .WithMany()
                .HasForeignKey(d => d.IdOwner);
			            this.HasRequired(t => t.SystemGoal)
                .WithMany()
                .HasForeignKey(d => d.IdSystemGoal);
			            this.HasRequired(t => t.GoalIndicator)
                .WithMany()
                .HasForeignKey(d => d.IdGoalIndicator);
			            this.HasRequired(t => t.ParentSystemGoal)
                .WithMany()
                .HasForeignKey(d => d.IdParentSystemGoal);
			            this.HasRequired(t => t.ElementTypeSystemGoal)
                .WithMany()
                .HasForeignKey(d => d.IdElementTypeSystemGoal);
			            this.HasRequired(t => t.SBP)
                .WithMany()
                .HasForeignKey(d => d.IdSBP);
			        }
    }
}
