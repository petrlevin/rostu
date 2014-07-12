using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ActivityOfSBP_SBPBlankActual
    /// </summary>
	public class ActivityOfSBP_SBPBlankActualMap : EntityTypeConfiguration<ActivityOfSBP_SBPBlankActual>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ActivityOfSBP_SBPBlankActualMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ActivityOfSBP_SBPBlankActual", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdSBP_BlankHistory).HasColumnName("idSBP_BlankHistory");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.SBPBlankActuals).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.SBP_BlankHistory).WithMany(t => t.ActivityOfSBP_SBPBlankActual).HasForeignKey(d => d.IdSBP_BlankHistory);
			
        }
    }
}
