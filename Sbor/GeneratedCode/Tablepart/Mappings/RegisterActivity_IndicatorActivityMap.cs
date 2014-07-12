using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegisterActivity_IndicatorActivity
    /// </summary>
	public class RegisterActivity_IndicatorActivityMap : EntityTypeConfiguration<RegisterActivity_IndicatorActivity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegisterActivity_IndicatorActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegisterActivity_IndicatorActivity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("IdMaster");
			this.Property(t => t.IndicatorActivity).HasColumnName("IndicatorActivity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.IndicatorActivity).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.RegisterActivity_IndicatorActivity).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.DicatorActivity).WithMany().HasForeignKey(d => d.IndicatorActivity);
			
        }
    }
}
