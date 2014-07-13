using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class PublicLegalFormationMap : EntityTypeConfiguration<PublicLegalFormation>
    {
        public PublicLegalFormationMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicLegalFormation", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.idParent).HasColumnName("idParent");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			            this.Property(t => t.idBudgetLevel).HasColumnName("idBudgetLevel");
			            this.Property(t => t.idAccessGroup).HasColumnName("idAccessGroup");
			            this.Property(t => t.Subject).HasColumnName("Subject");
			            this.Property(t => t.idMethodofFormingCode_GoalSetting).HasColumnName("idMethodofFormingCode_GoalSetting");
			            this.Property(t => t.idMethodofFormingCode_TargetIndex).HasColumnName("idMethodofFormingCode_TargetIndex");
			            this.Property(t => t.idMethodofFormingCode_Activity).HasColumnName("idMethodofFormingCode_Activity");
			
            // Relationships
			            this.HasOptional(t => t._idParent)
                .WithMany()
                .HasForeignKey(d => d.idParent);
			            this.HasRequired(t => t._idBudgetLevel)
                .WithMany()
                .HasForeignKey(d => d.idBudgetLevel);
			            this.HasRequired(t => t._idAccessGroup)
                .WithMany()
                .HasForeignKey(d => d.idAccessGroup);
			            this.HasRequired(t => t._idMethodofFormingCode_GoalSetting)
                .WithMany()
                .HasForeignKey(d => d.idMethodofFormingCode_GoalSetting);
			            this.HasRequired(t => t._idMethodofFormingCode_TargetIndex)
                .WithMany()
                .HasForeignKey(d => d.idMethodofFormingCode_TargetIndex);
			            this.HasRequired(t => t._idMethodofFormingCode_Activity)
                .WithMany()
                .HasForeignKey(d => d.idMethodofFormingCode_Activity);
			 
        }
    }
}