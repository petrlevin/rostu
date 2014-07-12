using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности PublicInstitutionEstimate_ActivityAUBU
    /// </summary>
	public class PublicInstitutionEstimate_ActivityAUBUMap : EntityTypeConfiguration<PublicInstitutionEstimate_ActivityAUBU>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public PublicInstitutionEstimate_ActivityAUBUMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("PublicInstitutionEstimate_ActivityAUBU", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdActivity).HasColumnName("idActivity");
			this.Property(t => t.IdContingent).HasColumnName("idContingent");
			this.Property(t => t.IdUnitDimension).HasColumnName("idUnitDimension");
			this.Property(t => t.IdIndicatorActivity).HasColumnName("idIndicatorActivity");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.ActivitiesAUBU).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.Activity).WithMany().HasForeignKey(d => d.IdActivity);
			this.HasOptional(t => t.Contingent).WithMany().HasForeignKey(d => d.IdContingent);
			this.HasOptional(t => t.UnitDimension).WithMany().HasForeignKey(d => d.IdUnitDimension);
			this.HasOptional(t => t.IndicatorActivity).WithMany().HasForeignKey(d => d.IdIndicatorActivity);
			
        }
    }
}
