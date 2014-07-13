using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.DataAccess;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Reference.Mappings
{
	public class EntityMap : EntityTypeConfiguration<Entity>
	{
		public EntityMap()
		{
			// Primary Key
			this.HasKey(t => t.Id);

			this.ToTable("Entity", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.IdEntityType).HasColumnName("idEntityType");
			this.Property(t => t.IdProject).HasColumnName("idProject");
			this.Property(t => t.IsSystem).HasColumnName("isSystem");
			this.Property(t => t.IsVersioning).HasColumnName("isVersioning");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.GenerateEntityClass).HasColumnName("GenerateEntityClass");
            this.Property(t => t.IdEntityGroup).HasColumnName("idEntityGroup");
			this.Ignore(t => t.EntityType);
			this.Ignore(t => t.RealFields);
			this.Ignore(t => t.Schema);
			this.Ignore(t => t.EntityCaption);
			this.Ignore(t => t.Fields);
			this.Ignore(t => t.DescriptionField);
			this.Ignore(t => t.CaptionField);
		}
	}
}
