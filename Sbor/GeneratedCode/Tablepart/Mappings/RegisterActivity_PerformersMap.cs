using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RegisterActivity_Performers
    /// </summary>
	public class RegisterActivity_PerformersMap : EntityTypeConfiguration<RegisterActivity_Performers>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RegisterActivity_PerformersMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RegisterActivity_Performers", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("IdMaster");
			this.Property(t => t.Performers).HasColumnName("Performers");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Performers).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Master).WithMany(t => t.RegisterActivity_Performers).HasForeignKey(d => d.IdMaster);
			this.HasRequired(t => t.Rformers).WithMany().HasForeignKey(d => d.Performers);
			
        }
    }
}
