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
	public class FilterMap : EntityTypeConfiguration<Filter>
	{
		public FilterMap()
		{
			// Primary Key
			this.HasKey(t => t.Id);

			this.ToTable("Filter", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntityField).HasColumnName("idEntityField");
			this.Property(t => t.IdLogicOperator).HasColumnName("idLogicOperator");
			this.Property(t => t.Not).HasColumnName("Not");
			this.Property(t => t.IdLeftEntityField).HasColumnName("idLeftEntityField");
			this.Property(t => t.IdComparisionOperator).HasColumnName("idComparisionOperator");
			this.Property(t => t.RightValue).HasColumnName("RightValue");
			this.Property(t => t.IdRightEntityField).HasColumnName("idRightEntityField");
			this.Property(t => t.RightSqlExpression).HasColumnName("RightSqlExpression");
			this.Property(t => t.IdParent).HasColumnName("idParent");
		    this.Property(t => t.WithParents).HasColumnName("WithParents");

			this.Ignore(t => t.ChildFilter);
			this.Ignore(t => t.ComparisionOperator);
			this.Ignore(t => t.LogicOperator);
			this.Ignore(t => t.RightEntityField);
		}
	}
}
