using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности UnitDimension
    /// </summary>
	public class UnitDimensionMap : EntityTypeConfiguration<UnitDimension>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public UnitDimensionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("UnitDimension", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.OKEICode).HasColumnName("OKEICode");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Symbol).HasColumnName("Symbol");
			this.Property(t => t.InternationalAbbreviation).HasColumnName("InternationalAbbreviation");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			
        }
    }
}
