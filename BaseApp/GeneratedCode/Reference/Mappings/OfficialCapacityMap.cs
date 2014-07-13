using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности OfficialCapacity
    /// </summary>
	public class OfficialCapacityMap : EntityTypeConfiguration<OfficialCapacity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public OfficialCapacityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("OfficialCapacity", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.Department).HasColumnName("Department");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			
        }
    }
}
