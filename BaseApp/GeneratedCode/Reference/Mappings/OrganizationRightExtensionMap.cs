using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности OrganizationRightExtension
    /// </summary>
	public class OrganizationRightExtensionMap : EntityTypeConfiguration<OrganizationRightExtension>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public OrganizationRightExtensionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("OrganizationRightExtension", "ref");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdKind).HasColumnName("idKind");
			this.Property(t => t.SqlTemplate).HasColumnName("SqlTemplate");
			
            // Relationships
			this.HasMany(t => t.Targets).WithMany().Map(m => m.MapLeftKey("OrganizationRightExtension").MapRightKey("Entity").ToTable("OrganizationRightExtension_Target", "ml"));
			this.HasMany(t => t.Results).WithMany().Map(m => m.MapLeftKey("OrganizationRightExtension").MapRightKey("Entity").ToTable("OrganizationRightExtension_Result", "ml"));
			
        }
    }
}
