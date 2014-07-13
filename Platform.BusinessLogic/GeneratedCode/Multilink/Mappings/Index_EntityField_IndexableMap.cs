using System.Data.Entity.ModelConfiguration;
using Platform.PrimaryEntities.Multilink;

namespace Platform.BusinessLogic.GeneratedCode.Multilink.Mappings
{
	/// <summary>
	/// Маппинг для Index_EntityField_Indexable
	/// </summary>
	public class Index_EntityField_IndexableMap : EntityTypeConfiguration<Index_EntityField_Indexable>
	{
		public Index_EntityField_IndexableMap()
		{
			// Primary Key
			this.HasKey(t => new {t.IdIndex, t.IdEntityField});

			this.ToTable("Index_EntityField_Indexable", "ml");
			this.Property(t => t.IdIndex).HasColumnName("idIndex");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			this.Property(t => t.IdIndexOrder).HasColumnName("idIndexOrder");
			this.Property(t => t.IdEntityFieldOrder).HasColumnName("idEntityFieldOrder");

			//this.HasOptional(o => o.Index).WithMany(m => m.MlIndexableEntityFields).HasForeignKey(f => f.IdIndex);
		}
	}
}
