using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности TypeRegulatoryAct
    /// </summary>
	public class TypeRegulatoryActMap : EntityTypeConfiguration<TypeRegulatoryAct>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public TypeRegulatoryActMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TypeRegulatoryAct", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdAccessGroup).HasColumnName("idAccessGroup");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasOptional(t => t.AccessGroup).WithMany().HasForeignKey(d => d.IdAccessGroup);
			
        }
    }
}
