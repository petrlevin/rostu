using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Activity_ExtInfo
    /// </summary>
	public class Activity_ExtInfoMap : EntityTypeConfiguration<Activity_ExtInfo>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Activity_ExtInfoMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Activity_ExtInfo", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Method).HasColumnName("Method");
			this.Property(t => t.Composition).HasColumnName("Composition");
			this.Property(t => t.Periodicity).HasColumnName("Periodicity");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdSBP).HasColumnName("idSBP");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ExtInfo).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.SBP).WithMany().HasForeignKey(d => d.IdSBP);
			
        }
    }
}
