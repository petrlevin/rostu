using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности testDtp
    /// </summary>
	public class testDtpMap : EntityTypeConfiguration<testDtp>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public testDtpMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("testDtp", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.V1).HasColumnName("v1");
			this.Property(t => t.V2).HasColumnName("v2");
			this.Property(t => t.V3).HasColumnName("v3");
			
            // Relationships
			
        }
    }
}
