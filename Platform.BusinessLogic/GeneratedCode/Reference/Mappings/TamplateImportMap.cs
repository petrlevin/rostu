using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TamplateImport
    /// </summary>
	public class TamplateImportMap : EntityTypeConfiguration<TamplateImport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TamplateImportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TamplateImport", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			
            // Relationships
			
        }
    }
}
