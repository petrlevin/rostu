using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FBA_Activity
    /// </summary>
	public class FBA_ActivityMap : EntityTypeConfiguration<FBA_Activity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FBA_ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FBA_Activity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IsOwnActivity).HasColumnName("isOwnActivity");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Activity).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			
        }
    }
}
