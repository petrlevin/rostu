using System.Data.Entity.ModelConfiguration;

namespace BaseApp.GeneratedCode.Mappings
{
    public class UnitDimensionMap : EntityTypeConfiguration<UnitDimension>
    {
        public UnitDimensionMap()
        {
            // Primary Key
            this.HasKey(t => t.id);

            // Properties
            // Table & Column Mappings
            this.ToTable("UnitDimension", "ref");
			            this.Property(t => t.id).HasColumnName("id");
			            this.Property(t => t.OKEICode).HasColumnName("OKEICode");
			            this.Property(t => t.Caption).HasColumnName("Caption");
			            this.Property(t => t.Symbol).HasColumnName("Symbol");
			            this.Property(t => t.InternationalAbbreviation).HasColumnName("InternationalAbbreviation");
			
            // Relationships
			 
        }
    }
}