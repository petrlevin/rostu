using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности StartedOperation
    /// </summary>
	public class StartedOperationMap : EntityTypeConfiguration<StartedOperation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public StartedOperationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("StartedOperation", "reg");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdEntityOperation).HasColumnName("idEntityOperation");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.Date).HasColumnName("Date");
			
            // Relationships
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasRequired(t => t.EntityOperation).WithMany().HasForeignKey(d => d.IdEntityOperation);
			this.HasRequired(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			
        }
    }
}
