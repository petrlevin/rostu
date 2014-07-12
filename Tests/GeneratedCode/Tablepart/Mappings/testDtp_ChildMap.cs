using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Tablepart.Mappings
{
    /// <summary>
    /// Класс описывающий маппинг для сущности testDtp_Child
    /// </summary>
	public class testDtp_ChildMap : EntityTypeConfiguration<testDtp_Child>
    {
        /// <summary>
        /// Дефолтный конструктор
        /// </summary>
		public testDtp_ChildMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("testDtp_Child", "tp");
			this.Property(t => t.Id).HasColumnName("id");
			this.Property(t => t.IdOwner).HasColumnName("idOwner");
			this.Property(t => t.IdMaster).HasColumnName("idMaster");
			this.Property(t => t.IdHierarchyPeriod).HasColumnName("idHierarchyPeriod");
			this.Property(t => t.Value1).HasPrecision(15,2).HasColumnName("Value1");
			this.Property(t => t.Value2).HasColumnName("Value2");
			this.Property(t => t.Value3).HasPrecision(15,2).HasColumnName("Value3");
			
            // Relationships
			this.HasRequired(t => t.Owner).WithMany(t => t.Child).HasForeignKey(d => d.IdOwner);
			this.HasOptional(t => t.HierarchyPeriod).WithMany().HasForeignKey(d => d.IdHierarchyPeriod);
			
        }
    }
}
