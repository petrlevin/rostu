using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности UserBandWidth
    /// </summary>
	public class UserBandWidthMap : EntityTypeConfiguration<UserBandWidth>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public UserBandWidthMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("UserBandWidth", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			this.Property(t => t.Ping).HasColumnName("Ping");
			this.Property(t => t.DownloadSpeed).HasPrecision(6,2).HasColumnName("DownloadSpeed");
			this.Property(t => t.Date).HasColumnName("Date");
			
            // Relationships
			this.HasRequired(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			
        }
    }
}
