using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности StateProgram_SubProgramResourceMaintenance_Value
    /// </summary>
	public class StateProgram_SubProgramResourceMaintenance_ValueMap : EntityTypeConfiguration<StateProgram_SubProgramResourceMaintenance_Value>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public StateProgram_SubProgramResourceMaintenance_ValueMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("StateProgram_SubProgramResourceMaintenance_Value", "tp");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value).HasPrecision(22,2).HasColumnName("Value");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.AdditionalValue).HasPrecision(22,2).HasColumnName("AdditionalValue");
			
            // Relationships
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasRequired(t => t.Owner).WithMany(t => t.SubProgramResourceMaintenance_Value).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Master).WithMany(t => t.StateProgram_SubProgramResourceMaintenance_Value).HasForeignKey(d => d.IdMaster);
			
        }
    }
}
