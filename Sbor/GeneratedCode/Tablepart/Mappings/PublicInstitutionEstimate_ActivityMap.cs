using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicInstitutionEstimate_Activity
    /// </summary>
	public class PublicInstitutionEstimate_ActivityMap : EntityTypeConfiguration<PublicInstitutionEstimate_Activity>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicInstitutionEstimate_ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicInstitutionEstimate_Activity", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			this.Property(t => t.IdIndicatorActivity).HasColumnName("idIndicatorActivity");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Activities).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			this.HasRequired(t => t.IndicatorActivity).WithMany().HasForeignKey(d => d.IdIndicatorActivity);
			this.HasRequired(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			
        }
    }
}
