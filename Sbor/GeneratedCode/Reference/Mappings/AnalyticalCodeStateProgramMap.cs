using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности AnalyticalCodeStateProgram
    /// </summary>
	public class AnalyticalCodeStateProgramMap : EntityTypeConfiguration<AnalyticalCodeStateProgram>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public AnalyticalCodeStateProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AnalyticalCodeStateProgram", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdTypeOfAnalyticalCodeStateProgram).HasColumnName("idTypeOfAnalyticalCodeStateProgram");
			this.Property(t => t.AnalyticalCode).HasColumnName("AnalyticalCode");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
