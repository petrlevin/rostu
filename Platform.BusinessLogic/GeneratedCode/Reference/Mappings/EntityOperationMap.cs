using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности EntityOperation
    /// </summary>
	public class EntityOperationMap : EntityTypeConfiguration<EntityOperation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public EntityOperationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("EntityOperation", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.IdOperation).HasColumnName("idOperation");
			this.Property(t => t.IsHidden).HasColumnName("isHidden");
			
            // Relationships
			this.HasRequired(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			this.HasRequired(t => t.Operation).WithMany().HasForeignKey(d => d.IdOperation);
			this.HasMany(t => t.OriginalStatus).WithMany().Map(m => m.MapLeftKey("idOperation").MapRightKey("idDocStatus").ToTable("EntityOperation_DocStatus_OriginalStatus", "ml"));
			this.HasMany(t => t.FinalStatus).WithMany().Map(m => m.MapLeftKey("idOperation").MapRightKey("idDocStatus").ToTable("EntityOperation_DocStatus_FinalStatus", "ml"));
			this.HasMany(t => t.EditableFields).WithMany().Map(m => m.MapLeftKey("idEntityOperation").MapRightKey("idEntityField").ToTable("EntityOperation_EntityField", "ml"));
			
        }
    }
}
