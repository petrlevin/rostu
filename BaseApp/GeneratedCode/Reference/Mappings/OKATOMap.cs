using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности OKATO
    /// </summary>
	public class OKATOMap : EntityTypeConfiguration<OKATO>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public OKATOMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("OKATO", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			this.Property(t => t.IdBudgetLevel).HasColumnName("idBudgetLevel");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasOptional(t => t.BudgetLevel).WithMany().HasForeignKey(d => d.IdBudgetLevel);
			
        }
    }
}
