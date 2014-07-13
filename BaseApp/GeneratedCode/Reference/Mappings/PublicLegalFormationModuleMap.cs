using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicLegalFormationModule
    /// </summary>
	public class PublicLegalFormationModuleMap : EntityTypeConfiguration<PublicLegalFormationModule>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicLegalFormationModuleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicLegalFormationModule", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasMany(t => t.IncludeModule).WithMany().Map(m => m.MapLeftKey("idPublicLegalFormationModule").MapRightKey("idModule").ToTable("PublicLegalFormationModule_Module", "ml"));
			
        }
    }
}
