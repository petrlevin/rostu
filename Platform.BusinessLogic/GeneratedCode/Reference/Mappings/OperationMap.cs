using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Operation
    /// </summary>
	public class OperationMap : EntityTypeConfiguration<Operation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public OperationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Operation", "ref");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.Id).HasColumnName("id");
			
            // Relationships
			
        }
    }
}
