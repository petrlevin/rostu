using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Registry.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности LimitVolumeAppropriations
    /// </summary>
	public class LimitVolumeAppropriationsMap : EntityTypeConfiguration<LimitVolumeAppropriations>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public LimitVolumeAppropriationsMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("LimitVolumeAppropriations", "reg");
			this.Property(t => t.IdExecutedOperation).HasColumnName("idExecutedOperation");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdPublicLegalFormation).HasColumnName("idPublicLegalFormation");
			this.Property(t => t.IdVersion).HasColumnName("idVersion");
			this.Property(t => t.IdBudget).HasColumnName("idBudget");
			this.Property(t => t.IdEstimatedLine).HasColumnName("idEstimatedLine");
			this.Property(t => t.IdAuthorityOfExpenseObligation).HasColumnName("idAuthorityOfExpenseObligation");
			this.Property(t => t.IdTaskCollection).HasColumnName("idTaskCollection");
			this.Property(t => t.IsIndirectCosts).HasColumnName("isIndirectCosts");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.IdValueType).HasColumnName("idValueType");
			this.Property(t => t.Value).HasPrecision(18,2).HasColumnName("Value");
			this.Property(t => t.IdOKATO).HasColumnName("idOKATO");
			this.Property(t => t.IsMeansAUBU).HasColumnName("isMeansAUBU");
			this.Property(t => t.IdRegistrator).HasColumnName("idRegistrator");
			this.Property(t => t.IdRegistratorEntity).HasColumnName("idRegistratorEntity");
			this.Property(t => t.DateCommit).HasColumnName("DateCommit");
			this.Property(t => t.IdApproved).HasColumnName("idApproved");
			this.Property(t => t.IdApprovedEntity).HasColumnName("idApprovedEntity");
			this.Property(t => t.DateCreate).HasColumnName("DateCreate");
			this.Property(t => t.HasAdditionalNeed).HasColumnName("HasAdditionalNeed");
			
            // Relationships
			this.HasOptional(t => t.ExecutedOperation).WithMany().HasForeignKey(d => d.IdExecutedOperation);
			this.HasRequired(t => t.PublicLegalFormation).WithMany().HasForeignKey(d => d.IdPublicLegalFormation);
			this.HasRequired(t => t.Version).WithMany().HasForeignKey(d => d.IdVersion);
			this.HasRequired(t => t.Budget).WithMany().HasForeignKey(d => d.IdBudget);
			this.HasRequired(t => t.EstimatedLine).WithMany().HasForeignKey(d => d.IdEstimatedLine);
			this.HasOptional(t => t.AuthorityOfExpenseObligation).WithMany().HasForeignKey(d => d.IdAuthorityOfExpenseObligation);
			this.HasOptional(t => t.TaskCollection).WithMany().HasForeignKey(d => d.IdTaskCollection);
			this.HasRequired(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			this.HasOptional(t => t.OKATO).WithMany().HasForeignKey(d => d.IdOKATO);
			this.HasRequired(t => t.RegistratorEntity).WithMany().HasForeignKey(d => d.IdRegistratorEntity);
			this.HasOptional(t => t.ApprovedEntity).WithMany().HasForeignKey(d => d.IdApprovedEntity);
			
        }
    }
}
