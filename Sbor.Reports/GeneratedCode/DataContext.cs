using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;
using System.Data.Common;
using Platform.BusinessLogic.EntityFramework;

using Sbor.Reports.Report;using Sbor.Reports.Report.Mappings;using Sbor.Reports.Tablepart;using Sbor.Reports.Tablepart.Mappings;using Sbor.Reports.Reference;using Sbor.Reports.Reference.Mappings;using Sbor.Reports.Tool;using Sbor.Reports.Tool.Mappings;
namespace Sbor.Reports
{
	/// <summary>
	/// Дата-контекст
	/// </summary>
	public partial class DataContext : Sbor.DataContext	{
		static DataContext()
		{
			Database.SetInitializer<DataContext>(null);
			OnStaticConstruct();
		}
		
		/// <summary>
		/// Конструктор по-умолчанию
		/// </summary>
		public DataContext()
			: this("PlatformDBConnectionString")
		{
			((IObjectContextAdapter) this).ObjectContext.ContextOptions.UseCSharpNullComparisonBehavior = true;
		}

		/// <summary>
		/// Конструктор, с возможностью указания строки соединения
		/// </summary>
		public DataContext([Dependency("ConnectionString")] string connectionString)
			: base(DbContextInitializer.CreateConnection(connectionString), true)
		{
				DbContextInitializer.EnableTracing(this);

		}
		
		/// <summary>
		/// Конструктор, с возможностью указания существующего соединения
		/// </summary>
		public DataContext(DbConnection dbConnection, bool contextOwnsConnection) :
			base(dbConnection, contextOwnsConnection)
		{
			
		}

		/// <summary>
		/// Ресурсное обеспечение реализации государственной программы
		/// </summary>
		public DbSet<Sbor.Reports.Report.ResourceMaintenanceOfTheStateProgram> ResourceMaintenanceOfTheStateProgram { get; set; }
		/// <summary>
		/// Направления и объемы финансирования деятельности ведомства
		/// </summary>
		public DbSet<Sbor.Reports.Report.DirectionAndFundingOfDepartment> DirectionAndFundingOfDepartment { get; set; }
		/// <summary>
		/// Анализ объема бюджетных ассигнований в разрезе прямых и косвенных расходов
		/// </summary>
		public DbSet<Sbor.Reports.Report.AnalysisBAofDirectAndIndirectCost> AnalysisBAofDirectAndIndirectCost { get; set; }
		/// <summary>
		/// Сравнение редакций
		/// </summary>
		public DbSet<Sbor.Reports.Report.EditionsComparision> EditionsComparision { get; set; }
		/// <summary>
		/// Отчет №1
		/// </summary>
		public DbSet<Sbor.Reports.Report.Report1> Report1 { get; set; }
		/// <summary>
		/// ТЧ для отчета №1
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.Report1_Tp> Report1_Tp { get; set; }
		/// <summary>
		/// Тестовый отчет по аудиту
		/// </summary>
		public DbSet<Sbor.Reports.Report.TestAuditReport> TestAuditReport { get; set; }
		/// <summary>
		/// Консолидированные расходы
		/// </summary>
		public DbSet<Sbor.Reports.Report.ConsolidatedExpenditure> ConsolidatedExpenditure { get; set; }
		/// <summary>
		/// ТЧ отчета Публично-правовые образования
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.ConsolidatedExpenditure_PPO> ConsolidatedExpenditure_PPO { get; set; }
		/// <summary>
		/// Правила фильтрации
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter> ConsolidatedExpenditure_BaseFilter { get; set; }
		/// <summary>
		/// Типы РО
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.ConsolidatedExpenditure_BaseFilter_ExpenseObligationType> ConsolidatedExpenditure_BaseFilter_ExpenseObligationType { get; set; }
		/// <summary>
		/// Целевые показатели ведомственной целевой программы
		/// </summary>
		public DbSet<Sbor.Reports.Report.GoalTargetsOfProgramSbp> GoalTargetsOfProgramSbp { get; set; }
		/// <summary>
		/// Потребность в оказании услуг (выполнении работ) учреждениями в рамках государственной программы
		/// </summary>
		public DbSet<Sbor.Reports.Report.NeedForTheProvisionOfActivitySbpWithinTheStateProgram> NeedForTheProvisionOfActivitySbpWithinTheStateProgram { get; set; }
		/// <summary>
		/// Обоснование бюджетных ассигнований
		/// </summary>
		public DbSet<Sbor.Reports.Report.JustificationOfBudget> JustificationOfBudget { get; set; }
		/// <summary>
		/// Стоимость целей
		/// </summary>
		public DbSet<Sbor.Reports.Report.CostGoals> CostGoals { get; set; }
		/// <summary>
		/// Межбюджетные трансферты
		/// </summary>
		public DbSet<Sbor.Reports.Report.InterBudgetaryTransfers> InterBudgetaryTransfers { get; set; }
		/// <summary>
		/// Настраиваемые колонки
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.InterBudgetaryTransfers_CustomizableColumns> InterBudgetaryTransfers_CustomizableColumns { get; set; }
		/// <summary>
		/// Правила фильтрации колонок
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.InterBudgetaryTransfers_RuleFilterKBK> InterBudgetaryTransfers_RuleFilterKBK { get; set; }
		/// <summary>
		/// Межбюджетные трансферты ТЧ Правила фильтрации колонок - Типы РО (ТЧ)
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.IBT_RuleFilterKBK_ExpenseObligationTypeT> IBT_RuleFilterKBK_ExpenseObligationTypeT { get; set; }
		/// <summary>
		/// Сводный отчет отдельных показателей
		/// </summary>
		public DbSet<Sbor.Reports.Report.SummaryReportOfSelectedIndicators> SummaryReportOfSelectedIndicators { get; set; }
		/// <summary>
		/// Универсальный отчет по шаблонам в WORD
		/// </summary>
		public DbSet<Sbor.Reports.Report.WordCommonReport> WordCommonReport { get; set; }
		/// <summary>
		/// Переменные отчетов
		/// </summary>
		public DbSet<Sbor.Reports.Reference.WordCommonReportParams> WordCommonReportParams { get; set; }
		/// <summary>
		/// Входные параметры
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.WordCommonReportParams_InputParam> WordCommonReportParams_InputParam { get; set; }
		/// <summary>
		/// Реестр целей (задач)
		/// </summary>
		public DbSet<Sbor.Reports.Report.RegistryGoal> RegistryGoal { get; set; }
		/// <summary>
		/// Действия пользователя
		/// </summary>
		public DbSet<Sbor.Reports.Report.UserActivityReport> UserActivityReport { get; set; }
		/// <summary>
		/// Структура расходов бюджета
		/// </summary>
		public DbSet<Sbor.Reports.Report.BudgetExpenseStructure> BudgetExpenseStructure { get; set; }
		/// <summary>
		/// Перечень выводимых колонок
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.BudgetExpenseStructure_Columns> BudgetExpenseStructure_Columns { get; set; }
		/// <summary>
		/// Правила фильтрации
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.BudgetExpenseStructure_BaseFilter> BudgetExpenseStructure_BaseFilter { get; set; }
		/// <summary>
		/// Типы РО
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.BudgetExpenseStructure_BaseFilter_ExpenseObligationType> BudgetExpenseStructure_BaseFilter_ExpenseObligationType { get; set; }
		/// <summary>
		/// Наименование колонок
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomColumn> BudgetExpenseStructure_CustomColumn { get; set; }
		/// <summary>
		/// Правила фильтрации колонок
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter> BudgetExpenseStructure_CustomFilter { get; set; }
		/// <summary>
		/// Типы РО
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.BudgetExpenseStructure_CustomFilter_ExpenseObligationType> BudgetExpenseStructure_CustomFilter_ExpenseObligationType { get; set; }
		/// <summary>
		/// Пакетная отчетность
		/// </summary>
		public DbSet<Sbor.Reports.Tool.ReportBatch> ReportBatch { get; set; }
		/// <summary>
		/// Состав пакета отчетов
		/// </summary>
		public DbSet<Sbor.Reports.Tablepart.ReportBatch_Reports> ReportBatch_Reports { get; set; }
		
		/// <summary>
		/// Получение типизированного набора сущностей
		/// </summary>
		public new IDbSet<TEntity> Set<TEntity>() where TEntity : class
		{
			return base.Set<TEntity>();
		}

		/// <summary>
		/// Событие при создании моделей
		/// </summary>
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new ResourceMaintenanceOfTheStateProgramMap());
			modelBuilder.Configurations.Add(new DirectionAndFundingOfDepartmentMap());
			modelBuilder.Configurations.Add(new AnalysisBAofDirectAndIndirectCostMap());
			modelBuilder.Configurations.Add(new EditionsComparisionMap());
			modelBuilder.Configurations.Add(new Report1Map());
			modelBuilder.Configurations.Add(new Report1_TpMap());
			modelBuilder.Configurations.Add(new TestAuditReportMap());
			modelBuilder.Configurations.Add(new ConsolidatedExpenditureMap());
			modelBuilder.Configurations.Add(new ConsolidatedExpenditure_PPOMap());
			modelBuilder.Configurations.Add(new ConsolidatedExpenditure_BaseFilterMap());
			modelBuilder.Configurations.Add(new ConsolidatedExpenditure_BaseFilter_ExpenseObligationTypeMap());
			modelBuilder.Configurations.Add(new GoalTargetsOfProgramSbpMap());
			modelBuilder.Configurations.Add(new NeedForTheProvisionOfActivitySbpWithinTheStateProgramMap());
			modelBuilder.Configurations.Add(new JustificationOfBudgetMap());
			modelBuilder.Configurations.Add(new CostGoalsMap());
			modelBuilder.Configurations.Add(new InterBudgetaryTransfersMap());
			modelBuilder.Configurations.Add(new InterBudgetaryTransfers_CustomizableColumnsMap());
			modelBuilder.Configurations.Add(new InterBudgetaryTransfers_RuleFilterKBKMap());
			modelBuilder.Configurations.Add(new IBT_RuleFilterKBK_ExpenseObligationTypeTMap());
			modelBuilder.Configurations.Add(new SummaryReportOfSelectedIndicatorsMap());
			modelBuilder.Configurations.Add(new WordCommonReportMap());
			modelBuilder.Configurations.Add(new WordCommonReportParamsMap());
			modelBuilder.Configurations.Add(new WordCommonReportParams_InputParamMap());
			modelBuilder.Configurations.Add(new RegistryGoalMap());
			modelBuilder.Configurations.Add(new UserActivityReportMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructureMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructure_ColumnsMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructure_BaseFilterMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructure_BaseFilter_ExpenseObligationTypeMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructure_CustomColumnMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructure_CustomFilterMap());
			modelBuilder.Configurations.Add(new BudgetExpenseStructure_CustomFilter_ExpenseObligationTypeMap());
			modelBuilder.Configurations.Add(new ReportBatchMap());
			modelBuilder.Configurations.Add(new ReportBatch_ReportsMap());
			 
			CustomOnModelCreating(modelBuilder);
			base.OnModelCreating(modelBuilder);
		}

		partial void CustomOnModelCreating(DbModelBuilder modelBuilder);
		static partial void OnStaticConstruct();
	}
}