using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности AuthorityOfExpenseObligation
    /// </summary>
	public class AuthorityOfExpenseObligationMap : EntityTypeConfiguration<AuthorityOfExpenseObligation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public AuthorityOfExpenseObligationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AuthorityOfExpenseObligation", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.IdAuthorityType).HasColumnName("idAuthorityType");
			this.Property(t => t.Description).HasColumnName("Description");
			this.Property(t => t.LineNumber).HasColumnName("LineNumber");
			this.Property(t => t.IdRefStatus).HasColumnName("idRefStatus");
			
            // Relationships
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			
        }
    }
}
