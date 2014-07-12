using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ResponsibleExecutantType
    /// </summary>
	public class ResponsibleExecutantTypeMap : EntityTypeConfiguration<ResponsibleExecutantType>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ResponsibleExecutantTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ResponsibleExecutantType", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			
        }
    }
}
