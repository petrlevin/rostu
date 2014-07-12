using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности RuleReferExpenceAsRoByKOSGU
    /// </summary>
	public class RuleReferExpenceAsRoByKOSGUMap : EntityTypeConfiguration<RuleReferExpenceAsRoByKOSGU>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public RuleReferExpenceAsRoByKOSGUMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RuleReferExpenceAsRoByKOSGU", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdAuthorityOfExpenseObligation).HasColumnName("idAuthorityOfExpenseObligation");
			this.Property(t => t.IdKOSGU).HasColumnName("idKOSGU");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasRequired(t => t.AuthorityOfExpenseObligation).WithMany().HasForeignKey(d => d.IdAuthorityOfExpenseObligation);
			this.HasRequired(t => t.KOSGU).WithMany().HasForeignKey(d => d.IdKOSGU);
			
        }
    }
}
