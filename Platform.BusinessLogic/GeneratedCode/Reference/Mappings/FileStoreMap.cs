using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FileStore
    /// </summary>
	public class FileStoreMap : EntityTypeConfiguration<FileStore>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FileStoreMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FileStore", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.File).HasColumnName("file");
			
            // Relationships
			
        }
    }
}
