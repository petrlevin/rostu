using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Reference.Mappings
{
	public class EntityFieldMap : EntityTypeConfiguration<EntityField>
	{
		public EntityFieldMap()
		{
			this.HasKey(t => t.Id);

			this.ToTable("EntityField", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IsSystem).HasColumnName("isSystem");
			this.Property(t => t.IsHidden).HasColumnName("isHidden");
			this.Property(t => t.IdEntityLink).HasColumnName("idEntityLink");
			this.Property(t => t.IdEntityFieldType).HasColumnName("idEntityFieldType");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.IdCalculatedFieldType).HasColumnName("idCalculatedFieldType");
			this.Property(t => t.IdForeignKeyType).HasColumnName("idForeignKeyType");
			this.Property(t => t.IdOwnerField).HasColumnName("idOwnerField");
			this.Property(t => t.IsCaption).HasColumnName("isCaption");
			this.Property(t => t.IsDescription).HasColumnName("isDescription");
			this.Property(t => t.Precision).HasColumnName("Precision");
			this.Property(t => t.ReadOnly).HasColumnName("ReadOnly");
			this.Property(t => t.Size).HasColumnName("Size");
			this.Property(t => t.AllowNull).HasColumnName("AllowNull");
			this.Property(t => t.Expression).HasColumnName("Expression");
			this.Property(t => t.DefaultValue).HasColumnName("DefaultValue");
			this.Ignore(t => t.FieldDefaultValueType);
			this.Ignore(t => t.EntityCaption);
			this.Ignore(t => t.CalculatedFieldType);
			this.Ignore(t => t.Entity);
			this.Ignore(t => t.EntityCaption);
			this.Ignore(t => t.EntityFieldType);
			this.Ignore(t => t.EntityId);
			this.Ignore(t => t.EntityLink);
			this.Ignore(t => t.ForeignKeyType);
			this.Ignore(t => t.SqlType);

			this.Ignore(t => t.IdFieldDefaultValueType);
		}
	}
}
