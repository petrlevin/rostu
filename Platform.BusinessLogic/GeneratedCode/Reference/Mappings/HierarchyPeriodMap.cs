using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности HierarchyPeriod
    /// </summary>
	public class HierarchyPeriodMap : EntityTypeConfiguration<HierarchyPeriod>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public HierarchyPeriodMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("HierarchyPeriod", "ref");
			this.Property(t => t.Year).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("Year");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.DateStart).HasColumnName("DateStart");
			this.Property(t => t.DateEnd).HasColumnName("DateEnd");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
