using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.GeneratedCode.Reference.Mappings
{
    public class FormMap : EntityTypeConfiguration<Form>
    {
        public FormMap()
        {
            this.HasKey(t => t.Id);
            this.ToTable("Form", "ref");
            this.Property(t => t.Id).HasColumnName("id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Caption).HasColumnName("Caption");
            this.Property(t => t.IdEntity).HasColumnName("idEntity");
            this.Property(t => t.IdHierarchyViewField).HasColumnName("idHierarchyViewField");
        }
    }
}
