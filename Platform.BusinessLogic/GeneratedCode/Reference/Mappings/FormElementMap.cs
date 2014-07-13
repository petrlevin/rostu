using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.GeneratedCode.Reference.Mappings
{
	public class FormElementMap : EntityTypeConfiguration<FormElement>
	{
		public FormElementMap()
		{
			this.HasKey(t => t.Id);

			this.ToTable("FormElements", "tp");
			
			this.Property(t => t.Id).HasColumnName("id");

			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdInterfaceControl).HasColumnName("idInterfaceControl");
			this.Property(t => t.IdEntityFieldType).HasColumnName("idEntityFieldType");
			this.Property(t => t.IdCalculatedFieldType).HasColumnName("idCalculatedFieldType");
			this.Property(t => t.Formula).HasColumnName("Formula");
			this.Property(t => t.Properties).HasColumnName("Properties");
			this.Property(t => t.Order).HasColumnName("Order");
			this.Property(t => t.Name).HasColumnName("Name");

			this.Ignore(t => t.ElementType);
			this.Ignore(t => t.EntityFieldType);
			this.Ignore(t => t.Form);

			this.HasRequired(t => t.EntityField).WithMany().HasForeignKey(d => d.IdEntityField);
			this.HasRequired(t => t.Control).WithMany().HasForeignKey(d => d.IdInterfaceControl);

            this.HasOptional(t => t.EntityField).WithMany().HasForeignKey(d => d.IdEntityField);
            this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
		}
	}
}
