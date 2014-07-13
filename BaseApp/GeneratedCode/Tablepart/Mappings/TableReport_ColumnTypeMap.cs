using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TableReport_ColumnType
    /// </summary>
	public class TableReport_ColumnTypeMap : EntityTypeConfiguration<TableReport_ColumnType>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TableReport_ColumnTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TableReport_ColumnType", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.FieldName).HasColumnName("FieldName");
			this.Property(t => t.IdEntityFieldType).HasColumnName("idEntityFieldType");
			this.Property(t => t.Precision).HasColumnName("Precision");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ColumnTypes).HasForeignKey(d => d.IdOwner);
			
        }
    }
}
