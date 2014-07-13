using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.GeneratedCode.Reference.Mappings
{
	public class InterfaceControlMap : EntityTypeConfiguration<InterfaceControl>
	{
		public InterfaceControlMap()
		{
			HasKey(t => t.Id);

			ToTable("InterfaceControl", "ref");
			Property(t => t.Alias).HasColumnName("Alias");
			Property(t => t.Caption).HasColumnName("Caption");
			Property(t => t.ComponentName).HasColumnName("ComponentName");
			Property(t => t.Description).HasColumnName("Description");
			Property(t => t.DefaultProperties).HasColumnName("DefaultProperties");
			Property(t => t.LabelProperty).HasColumnName("LabelProperty");
		}
	}
}
