using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Activity_CodeAuthority
    /// </summary>
	public class Activity_CodeAuthorityMap : EntityTypeConfiguration<Activity_CodeAuthority>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Activity_CodeAuthorityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Activity_CodeAuthority", "tp");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IsMain).HasColumnName("IsMain");
			this.Property(t => t.IdAuthorityOfExpenseObligation).HasColumnName("idAuthorityOfExpenseObligation");
			this.Property(t => t.Id).HasColumnName("id");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.CodeAuthority).HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.AuthorityOfExpenseObligation).WithMany().HasForeignKey(d => d.IdAuthorityOfExpenseObligation);
			
        }
    }
}
