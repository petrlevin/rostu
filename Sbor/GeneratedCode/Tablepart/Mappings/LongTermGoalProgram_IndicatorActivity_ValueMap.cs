using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LongTermGoalProgram_IndicatorActivity_Value
    /// </summary>
	public class LongTermGoalProgram_IndicatorActivity_ValueMap : EntityTypeConfiguration<LongTermGoalProgram_IndicatorActivity_Value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LongTermGoalProgram_IndicatorActivity_ValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LongTermGoalProgram_IndicatorActivity_Value", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(20,5).HasColumnName("Value");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.AdditionalValue).HasPrecision(20,5).HasColumnName("AdditionalValue");
			
            // Relationships
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.IndicatorActivity_Value).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.LongTermGoalProgram_IndicatorActivity_Value).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
