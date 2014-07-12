using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TestObject
    /// </summary>
	public class TestObjectMap : EntityTypeConfiguration<TestObject>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TestObjectMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TestObject", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Value).HasColumnName("Value");
			
            // Relationships
			
        }
    }
}
