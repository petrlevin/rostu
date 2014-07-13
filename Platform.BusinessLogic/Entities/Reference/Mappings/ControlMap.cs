using System.Data.Entity.ModelConfiguration;

namespace Platform.BusinessLogic.Reference.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности Control
    /// </summary>
	public class ControlMap : EntityTypeConfiguration<Control>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ControlMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Control", "ref");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Enabled).HasColumnName("Enabled");
			this.Property(t => t.Skippable).HasColumnName("Skippable");
            this.Property(t => t.Managed).HasColumnName("Managed");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Caption).HasColumnName("Caption");
			this.Property(t => t.UNK).HasColumnName("UNK");
			this.Property(t => t.IdEntity).HasColumnName("idEntity");
			
            // Relationships
			this.HasOptional(t => t.Entity).WithMany().HasForeignKey(d => d.IdEntity);
			
        }
    }
}
