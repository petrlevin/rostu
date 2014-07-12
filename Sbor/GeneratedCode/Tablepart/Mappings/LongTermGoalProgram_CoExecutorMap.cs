using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LongTermGoalProgram_CoExecutor
    /// </summary>
	public class LongTermGoalProgram_CoExecutorMap : EntityTypeConfiguration<LongTermGoalProgram_CoExecutor>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LongTermGoalProgram_CoExecutorMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LongTermGoalProgram_CoExecutor", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			this.Property(t => t.IdResponsibleExecutantType).HasColumnName("idResponsibleExecutantType");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SBPs).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			this.HasRequired(t => t.ResponsibleExecutantType).WithMany().HasForeignKey(d => d.IdResponsibleExecutantType);
			
        }
    }
}
