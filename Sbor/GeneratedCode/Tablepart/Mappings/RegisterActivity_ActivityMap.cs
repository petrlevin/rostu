using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegisterActivity_Activity
    /// </summary>
	public class RegisterActivity_ActivityMap : EntityTypeConfiguration<RegisterActivity_Activity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegisterActivity_ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegisterActivity_Activity", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdIndicatorActivity_Volume).HasColumnName("idIndicatorActivity_Volume");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			this.Property(t => t.IdRegistryKeyActivity).HasColumnName("idRegistryKeyActivity");
			this.Property(t => t.IdRegystryActivity_ActivityMain).HasColumnName("idRegystryActivity_ActivityMain");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Activity).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			this.HasRequired(t => t.IndicatorActivity_Volume).WithMany().HasForeignKey(d => d.IdIndicatorActivity_Volume);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			this.HasOptional(t => t.RegystryActivity_ActivityMain).WithMany(t => t.ChildrenByidRegystryActivity_ActivityMain).HasForeignKey(d => d.IdRegystryActivity_ActivityMain);
			
        }
    }
}
