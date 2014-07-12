using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sbor.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности BalancingIFDB_Program
    /// </summary>
	public class BalancingIFDB_ProgramMap : EntityTypeConfiguration<BalancingIFDB_Program>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public BalancingIFDB_ProgramMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BalancingIFDB_Program", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdParent).HasColumnName("idParent");
			this.Property(t => t.OFG).HasPrecision(22,2).HasColumnName("OFG");
			this.Property(t => t.AdditionalOFG).HasPrecision(22,2).HasColumnName("AdditionalOFG");
			this.Property(t => t.ChangeOFG).HasPrecision(22,2).HasColumnName("ChangeOFG");
			this.Property(t => t.ChangeAdditionalOFG).HasPrecision(22,2).HasColumnName("ChangeAdditionalOFG");
			this.Property(t => t.PFG1).HasPrecision(22,2).HasColumnName("PFG1");
			this.Property(t => t.AdditionalPFG1).HasPrecision(22,2).HasColumnName("AdditionalPFG1");
			this.Property(t => t.ChangePFG1).HasPrecision(22,2).HasColumnName("ChangePFG1");
			this.Property(t => t.ChangeAdditionalPFG1).HasPrecision(22,2).HasColumnName("ChangeAdditionalPFG1");
			this.Property(t => t.PFG2).HasPrecision(22,2).HasColumnName("PFG2");
			this.Property(t => t.AdditionalPFG2).HasPrecision(22,2).HasColumnName("AdditionalPFG2");
			this.Property(t => t.ChangePFG2).HasPrecision(22,2).HasColumnName("ChangePFG2");
			this.Property(t => t.ChangeAdditionalPFG2).HasPrecision(22,2).HasColumnName("ChangeAdditionalPFG2");
			this.Property(t => t.IdProgramOrActivity).HasColumnName("idProgramOrActivity");
			this.Property(t => t.IdProgramOrActivityEntity).HasColumnName("idProgramOrActivityEntity");
			this.Property(t => t.IdType).HasColumnName("idType");
			this.Property(t => t.IdTypeEntity).HasColumnName("idTypeEntity");
			this.Property(t => t.DifferenceOFG).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceOFG");
			this.Property(t => t.DifferenceAdditionalOFG).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceAdditionalOFG");
			this.Property(t => t.DifferencePFG1).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferencePFG1");
			this.Property(t => t.DifferenceAdditionalPFG1).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceAdditionalPFG1");
			this.Property(t => t.DifferencePFG2).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferencePFG2");
			this.Property(t => t.DifferenceAdditionalPFG2).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed).HasColumnName("DifferenceAdditionalPFG2");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Programs).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.Parent).WithMany(t => t.ChildrenByidParent).HasForeignKey(d => d.IdParent);
			this.HasRequired(t => t.ProgramOrActivityEntity).WithMany().HasForeignKey(d => d.IdProgramOrActivityEntity);
			this.HasOptional(t => t.TypeEntity).WithMany().HasForeignKey(d => d.IdTypeEntity);
			
        }
    }
}
