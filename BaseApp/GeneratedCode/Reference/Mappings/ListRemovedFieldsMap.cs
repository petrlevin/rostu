using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ListRemovedFields
    /// </summary>
	public class ListRemovedFieldsMap : EntityTypeConfiguration<ListRemovedFields>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ListRemovedFieldsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ListRemovedFields", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Number).HasColumnName("Number");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			
            // Relationships
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			
        }
    }
}
