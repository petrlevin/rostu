using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reports.Report.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности UserActivityReport
    /// </summary>
	public class UserActivityReportMap : EntityTypeConfiguration<UserActivityReport>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public UserActivityReportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("UserActivityReport", "rep");
			this.Property(t => t.ReportProfileCaption).HasColumnName("ReportProfileCaption");
			this.Property(t => t.IdReportProfileType).HasColumnName("idReportProfileType");
			this.Property(t => t.IdReportProfileUser).HasColumnName("idReportProfileUser");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IsTemporary).HasColumnName("isTemporary");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			this.Property(t => t.DateFrom).HasColumnName("DateFrom");
			this.Property(t => t.DateTo).HasColumnName("DateTo");
			this.Property(t => t.IdAuditEntity).HasColumnName("idAuditEntity");
			this.Property(t => t.IdAuditEntityEntity).HasColumnName("idAuditEntityEntity");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			this.Property(t => t.IdElement).HasColumnName("idElement");
			
            // Relationships
			this.HasOptional(t => t.ReportProfileUser).WithMany().HasForeignKey(d => d.IdReportProfileUser);
			this.HasOptional(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			this.HasOptional(t => t.AuditEntityEntity).WithMany().HasForeignKey(d => d.IdAuditEntityEntity);
			this.HasOptional(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			this.HasOptional(t => t.Element).WithMany().HasForeignKey(d => d.IdElement);
			
        }
    }
}
