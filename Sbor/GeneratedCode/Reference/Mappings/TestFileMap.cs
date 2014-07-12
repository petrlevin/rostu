using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestFile
    /// </summary>
	public class TestFileMap : EntityTypeConfiguration<TestFile>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestFileMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestFile", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.IdTemplateFile).HasColumnName("idTemplateFile");
			
            // Relationships
			
        }
    }
}
