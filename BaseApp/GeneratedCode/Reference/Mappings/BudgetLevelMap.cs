using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BudgetLevel
    /// </summary>
	public class BudgetLevelMap : EntityTypeConfiguration<BudgetLevel>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BudgetLevelMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BudgetLevel", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
