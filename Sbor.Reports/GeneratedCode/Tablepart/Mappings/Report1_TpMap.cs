using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Report1_Tp
    /// </summary>
	public class Report1_TpMap : EntityTypeConfiguration<Report1_Tp>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Report1_TpMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Report1_Tp", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Name).HasColumnName("Name");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SomeTablepart).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
