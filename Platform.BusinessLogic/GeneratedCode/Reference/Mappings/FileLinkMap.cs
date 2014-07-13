using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности FileLink
    /// </summary>
	public class FileLinkMap : EntityTypeConfiguration<FileLink>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public FileLinkMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("FileLink", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsDbStore).HasColumnName("isDbStore");
			this.Property(t => t.IdFileStore).HasColumnName("idFileStore");
			this.Property(t => t.FilePath).HasColumnName("filePath");
			this.Property(t => t.Caption).HasColumnName("caption");
			this.Property(t => t.Description).HasColumnName("description");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.FileSize).HasColumnName("fileSize");
			this.Property(t => t.Extension).HasColumnName("extension");
			
            // Relationships
			this.HasOptional(t => t.FileStore).WithMany().HasForeignKey(d => d.IdFileStore);
			
        }
    }
}
