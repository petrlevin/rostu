using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности License
    /// </summary>
	public class LicenseMap : EntityTypeConfiguration<License>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LicenseMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("License", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Key).HasColumnName("Key");
			this.Property(t => t.UserCount).HasColumnName("UserCount");
			this.Property(t => t.EndDate).HasColumnName("EndDate");
			this.Property(t => t.PublicLegalFormation).HasColumnName("PublicLegalFormation");

			this.HasMany(t => t.Users).WithMany().Map(m => m.MapLeftKey("idLicense").MapRightKey("idUser").ToTable("License_User", "ml"));
        }
    }
}
