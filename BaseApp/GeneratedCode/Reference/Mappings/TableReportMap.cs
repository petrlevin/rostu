using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TableReport
    /// </summary>
	public class TableReportMap : EntityTypeConfiguration<TableReport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TableReportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TableReport", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.IdSolutionProject).HasColumnName("idSolutionProject");
			this.Property(t => t.Sql).HasColumnName("Sql");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
