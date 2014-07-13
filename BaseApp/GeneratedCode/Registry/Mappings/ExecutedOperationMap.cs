using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseApp.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности ExecutedOperation
    /// </summary>
	public class ExecutedOperationMap : EntityTypeConfiguration<ExecutedOperation>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public ExecutedOperationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ExecutedOperation", "reg");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.Date).HasColumnName("Date");
			this.Property(t => t.IdUser).HasColumnName("idUser");
			this.Property(t => t.IdEntityOperation).HasColumnName("idEntityOperation");
			this.Property(t => t.IdOriginalStatus).HasColumnName("idOriginalStatus");
			this.Property(t => t.IdFinalStatus).HasColumnName("idFinalStatus");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			
            // Relationships
			this.HasRequired(t => t.User).WithMany().HasForeignKey(d => d.IdUser);
			this.HasRequired(t => t.EntityOperation).WithMany().HasForeignKey(d => d.IdEntityOperation);
			this.HasRequired(t => t.OriginalStatus).WithMany().HasForeignKey(d => d.IdOriginalStatus);
			this.HasRequired(t => t.FinalStatus).WithMany().HasForeignKey(d => d.IdFinalStatus);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			
        }
    }
}
