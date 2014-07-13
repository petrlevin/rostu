using System.Data.Entity.ModelConfiguration;

namespace BaseApp.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Control_Exceptions
    /// </summary>
	public class Control_ExceptionsMap : EntityTypeConfiguration<Control_Exceptions>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public Control_ExceptionsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Control_Exceptions", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.Enabled).HasColumnName("Enabled");
			this.Property(t => t.Skippable).HasColumnName("Skippable");

			
            // Relationships
            this.HasRequired(t => t.Owner).WithMany().HasForeignKey(d => d.IdOwner);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasOptional(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			
        }
    }
}
