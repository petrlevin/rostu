using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Microsoft.Practices.Unity;
using System.Data.Common;
using Platform.BusinessLogic.EntityFramework;

using Sbor.Reference;using Sbor.Reference.Mappings;using Sbor.Tablepart;using Sbor.Tablepart.Mappings;using Sbor.Registry;using Sbor.Registry.Mappings;using Sbor.Document;using Sbor.Document.Mappings;using Sbor.Tool;using Sbor.Tool.Mappings;
namespace Sbor
{
	/// <summary>
	/// Дата-контекст
	/// </summary>
	public partial class DataContext : BaseApp.DataContext	{
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
		/// Виды НПА
		/// </summary>
		public DbSet<Sbor.Reference.TypeRegulatoryAct> TypeRegulatoryAct { get; set; }
		/// <summary>
		/// Нормативно-правовые акты
		/// </summary>
		public DbSet<Sbor.Reference.RegulatoryAct> RegulatoryAct { get; set; }
		/// <summary>
		/// КВСР/КАДБ/КАИФ
		/// </summary>
		public DbSet<Sbor.Reference.KVSR> KVSR { get; set; }
		/// <summary>
		/// Субъекты бюджетного планирования
		/// </summary>
		public DbSet<Sbor.Reference.SBP> SBP { get; set; }
		/// <summary>
		/// Полномочия расходных обязательств
		/// </summary>
		public DbSet<Sbor.Reference.AuthorityOfExpenseObligation> AuthorityOfExpenseObligation { get; set; }
		/// <summary>
		/// Структурные единицы НПА
		/// </summary>
		public DbSet<Sbor.Tablepart.RegulatoryAct_StructuralUnit> RegulatoryAct_StructuralUnit { get; set; }
		/// <summary>
		/// Типы документов
		/// </summary>
		public DbSet<Sbor.Reference.DocType> DocType { get; set; }
		/// <summary>
		/// Типы элементов СЦ
		/// </summary>
		public DbSet<Sbor.Reference.ElementTypeSystemGoal> ElementTypeSystemGoal { get; set; }
		/// <summary>
		/// Документы
		/// </summary>
		public DbSet<Sbor.Tablepart.ElementTypeSystemGoal_Document> ElementTypeSystemGoal_Document { get; set; }
		/// <summary>
		/// Модель СЦ
		/// </summary>
		public DbSet<Sbor.Reference.ModelSystemGoal> ModelSystemGoal { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Reference.GoalIndicator> GoalIndicator { get; set; }
		/// <summary>
		/// Система целеполагания
		/// </summary>
		public DbSet<Sbor.Reference.SystemGoal> SystemGoal { get; set; }
		/// <summary>
		/// Элементы СЦ
		/// </summary>
		public DbSet<Sbor.Tablepart.DocumentsOfSED_ItemsSystemGoal> DocumentsOfSED_ItemsSystemGoal { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Tablepart.DocumentsOfSED_GoalIndicator> DocumentsOfSED_GoalIndicator { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Tablepart.SystemGoal_GoalIndicator> SystemGoal_GoalIndicator { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Registry.GoalTarget> GoalTarget { get; set; }
		/// <summary>
		/// Значения целевых показателей
		/// </summary>
		public DbSet<Sbor.Registry.ValuesGoalTarget> ValuesGoalTarget { get; set; }
		/// <summary>
		/// Бланки доведения и формирования
		/// </summary>
		public DbSet<Sbor.Tablepart.SBP_Blank> SBP_Blank { get; set; }
		/// <summary>
		/// Периоды планирования в документах АУ/БУ
		/// </summary>
		public DbSet<Sbor.Tablepart.SBP_PlanningPeriodsInDocumentsAUBU> SBP_PlanningPeriodsInDocumentsAUBU { get; set; }
		/// <summary>
		/// Значения целевых показателей
		/// </summary>
		public DbSet<Sbor.Tablepart.SystemGoal_GoalIndicatorValue> SystemGoal_GoalIndicatorValue { get; set; }
		/// <summary>
		/// Значения целевых показателей
		/// </summary>
		public DbSet<Sbor.Tablepart.DocumentsOfSED_GoalIndicatorValue> DocumentsOfSED_GoalIndicatorValue { get; set; }
		/// <summary>
		/// План деятельности
		/// </summary>
		public DbSet<Sbor.Document.PlanActivity> PlanActivity { get; set; }
		/// <summary>
		/// Периоды финансового обеспечения
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_PeriodsOfFinancialProvision> PlanActivity_PeriodsOfFinancialProvision { get; set; }
		/// <summary>
		/// Мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_Activity> PlanActivity_Activity { get; set; }
		/// <summary>
		/// Объемы мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_ActivityVolume> PlanActivity_ActivityVolume { get; set; }
		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_IndicatorQualityActivity> PlanActivity_IndicatorQualityActivity { get; set; }
		/// <summary>
		/// Значения показателей качества
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_IndicatorQualityActivityValue> PlanActivity_IndicatorQualityActivityValue { get; set; }
		/// <summary>
		/// Мероприятия АУ/БУ
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_ActivityAUBU> PlanActivity_ActivityAUBU { get; set; }
		/// <summary>
		/// Объемы мероприятий АУ/БУ
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_ActivityVolumeAUBU> PlanActivity_ActivityVolumeAUBU { get; set; }
		/// <summary>
		/// КБК финансового обеспечения
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_KBKOfFinancialProvision> PlanActivity_KBKOfFinancialProvision { get; set; }
		/// <summary>
		/// Требования к заданию
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_RequirementsForTheTask> PlanActivity_RequirementsForTheTask { get; set; }
		/// <summary>
		/// Порядок контроля за исполнением задания
		/// </summary>
		public DbSet<Sbor.Tablepart.PlanActivity_OrderOfControlTheExecutionTasks> PlanActivity_OrderOfControlTheExecutionTasks { get; set; }
		/// <summary>
		/// Программы/мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_Program> BalancingIFDB_Program { get; set; }
		/// <summary>
		/// Расходы
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_Expense> BalancingIFDB_Expense { get; set; }
		/// <summary>
		/// Балансировка доходов, расходов и ИФДБ
		/// </summary>
		public DbSet<Sbor.Tool.BalancingIFDB> BalancingIFDB { get; set; }
		/// <summary>
		/// Настройка балансировки
		/// </summary>
		public DbSet<Sbor.Tool.BalanceConfig> BalanceConfig { get; set; }
		/// <summary>
		/// Правила фильтрации
		/// </summary>
		public DbSet<Sbor.Tablepart.BalanceConfig_FilterRule> BalanceConfig_FilterRule { get; set; }
		/// <summary>
		/// Пользователи
		/// </summary>
		public DbSet<Sbor.Tablepart.BalanceConfig_User> BalanceConfig_User { get; set; }
		/// <summary>
		/// Фильтры КБК
		/// </summary>
		public DbSet<Sbor.Tablepart.BalanceConfig_FilterKBK> BalanceConfig_FilterKBK { get; set; }
		/// <summary>
		/// Сметные строки
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_EstimatedLine> BalancingIFDB_EstimatedLine { get; set; }
		/// <summary>
		/// Примененные правила индексации
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_RuleIndex> BalancingIFDB_RuleIndex { get; set; }
		/// <summary>
		/// Настройка отображения КБК
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_SetShowKBK> BalancingIFDB_SetShowKBK { get; set; }
		/// <summary>
		/// История применения правил
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_ChangeHistory> BalancingIFDB_ChangeHistory { get; set; }
		/// <summary>
		/// Набор КБК для правила
		/// </summary>
		public DbSet<Sbor.Tablepart.BalancingIFDB_RuleFilterKBK> BalancingIFDB_RuleFilterKBK { get; set; }
		/// <summary>
		/// Утверждение балансировки расходов, доходов и ИФДБ
		/// </summary>
		public DbSet<Sbor.Tool.ApprovalBalancingIFDB> ApprovalBalancingIFDB { get; set; }
		/// <summary>
		/// Бланки
		/// </summary>
		public DbSet<Sbor.Tablepart.ApprovalBalancingIFDB_Blank> ApprovalBalancingIFDB_Blank { get; set; }
		/// <summary>
		/// План финансово-хозяйственной деятельности
		/// </summary>
		public DbSet<Sbor.Document.FinancialAndBusinessActivities> FinancialAndBusinessActivities { get; set; }
		/// <summary>
		/// Мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_Activity> FBA_Activity { get; set; }
		/// <summary>
		/// Плановые объемы поступлений
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_PlannedVolumeIncome> FBA_PlannedVolumeIncome { get; set; }
		/// <summary>
		/// Плановые объемы поступлений - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_PlannedVolumeIncome_value> FBA_PlannedVolumeIncome_value { get; set; }
		/// <summary>
		/// Расходы по мероприятиям
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_CostActivities> FBA_CostActivities { get; set; }
		/// <summary>
		/// Расходы по мероприятиям - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_CostActivities_value> FBA_CostActivities_value { get; set; }
		/// <summary>
		/// Методы распределения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_DistributionMethods> FBA_DistributionMethods { get; set; }
		/// <summary>
		/// Дополнительный параметр распределения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_DistributionAdditionalParameter> FBA_DistributionAdditionalParameter { get; set; }
		/// <summary>
		/// Мероприятия для распределения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_ActivitiesDistribution> FBA_ActivitiesDistribution { get; set; }
		/// <summary>
		/// Косвенные расходы
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_IndirectCosts> FBA_IndirectCosts { get; set; }
		/// <summary>
		/// Косвенные расходы - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_IndirectCosts_value> FBA_IndirectCosts_value { get; set; }
		/// <summary>
		/// Цели деятельности учреждения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_DepartmentActivityGoal> FBA_DepartmentActivityGoal { get; set; }
		/// <summary>
		/// Показатели финансового состояния учреждения
		/// </summary>
		public DbSet<Sbor.Tablepart.FBA_FinancialIndicatorsInstitutions> FBA_FinancialIndicatorsInstitutions { get; set; }
		/// <summary>
		/// Единицы измерения
		/// </summary>
		public DbSet<Sbor.Reference.UnitDimension> UnitDimension { get; set; }
		/// <summary>
		/// Аналитические коды государственных программ
		/// </summary>
		public DbSet<Sbor.Reference.AnalyticalCodeStateProgram> AnalyticalCodeStateProgram { get; set; }
		/// <summary>
		/// КВР
		/// </summary>
		public DbSet<Sbor.Reference.KVR> KVR { get; set; }
		/// <summary>
		/// Шаблоны обоснований
		/// </summary>
		public DbSet<Sbor.Reference.TemplateJustification> TemplateJustification { get; set; }
		/// <summary>
		/// Расходные обязательства
		/// </summary>
		public DbSet<Sbor.Reference.ExpenditureObligations> ExpenditureObligations { get; set; }
		/// <summary>
		/// ТЧ Нормативные правовые акты (Расходные обязательства)
		/// </summary>
		public DbSet<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct> ExpenditureObligations_RegulatoryAct { get; set; }
		/// <summary>
		/// ТЧ Структурные единицы НПА (Расходные обязательства)
		/// </summary>
		public DbSet<Sbor.Tablepart.ExpenditureObligations_RegulatoryAct_StructuralUnit> ExpenditureObligations_RegulatoryAct_StructuralUnit { get; set; }
		/// <summary>
		/// TestDocument
		/// </summary>
		public DbSet<Sbor.Document.TestDocument> TestDocument { get; set; }
		/// <summary>
		/// TestReg1
		/// </summary>
		public DbSet<Sbor.Registry.TestReg1> TestReg1 { get; set; }
		/// <summary>
		/// TestReg2
		/// </summary>
		public DbSet<Sbor.Registry.TestReg2> TestReg2 { get; set; }
		/// <summary>
		/// TestDocument2
		/// </summary>
		public DbSet<Sbor.Document.TestDocument2> TestDocument2 { get; set; }
		/// <summary>
		/// Таб часть тестового документа
		/// </summary>
		public DbSet<Sbor.Tablepart.TestDocumentTP> TestDocumentTP { get; set; }
		/// <summary>
		/// TestDocumentTP2
		/// </summary>
		public DbSet<Sbor.Tablepart.TestDocumentTP2> TestDocumentTP2 { get; set; }
		/// <summary>
		/// Тестовые значения
		/// </summary>
		public DbSet<Sbor.Reference.TestObject> TestObject { get; set; }
		/// <summary>
		/// Тестовый документ 3
		/// </summary>
		public DbSet<Sbor.Document.TestDocument3> TestDocument3 { get; set; }
		/// <summary>
		/// Элементы СЦ
		/// </summary>
		public DbSet<Sbor.Registry.SystemGoalElement> SystemGoalElement { get; set; }
		/// <summary>
		/// Атрибуты элементов СЦ
		/// </summary>
		public DbSet<Sbor.Registry.AttributeOfSystemGoalElement> AttributeOfSystemGoalElement { get; set; }
		/// <summary>
		/// Документы СЭР
		/// </summary>
		public DbSet<Sbor.Document.DocumentsOfSED> DocumentsOfSED { get; set; }
		/// <summary>
		/// Сметные строки
		/// </summary>
		public DbSet<Sbor.Registry.EstimatedLine> EstimatedLine { get; set; }
		/// <summary>
		/// Объемы финансовых средств
		/// </summary>
		public DbSet<Sbor.Registry.LimitVolumeAppropriations> LimitVolumeAppropriations { get; set; }
		/// <summary>
		/// Показатели качества
		/// </summary>
		public DbSet<Sbor.Tablepart.RegisterActivity_IndicatorActivity> RegisterActivity_IndicatorActivity { get; set; }
		/// <summary>
		/// Исполнители
		/// </summary>
		public DbSet<Sbor.Tablepart.RegisterActivity_Performers> RegisterActivity_Performers { get; set; }
		/// <summary>
		/// Отраслевые коды
		/// </summary>
		public DbSet<Sbor.Reference.BranchCode> BranchCode { get; set; }
		/// <summary>
		/// Разрешения на ввод дополнительных потребностей
		/// </summary>
		public DbSet<Sbor.Reference.PermissionsInputAdditionalRequirements> PermissionsInputAdditionalRequirements { get; set; }
		/// <summary>
		/// Источники финансирования
		/// </summary>
		public DbSet<Sbor.Reference.FinanceSource> FinanceSource { get; set; }
		/// <summary>
		/// Перечень мероприятий
		/// </summary>
		public DbSet<Sbor.Reference.Activity> Activity { get; set; }
		/// <summary>
		/// Показатели мероприятий
		/// </summary>
		public DbSet<Sbor.Reference.IndicatorActivity> IndicatorActivity { get; set; }
		/// <summary>
		/// Категории контингента
		/// </summary>
		public DbSet<Sbor.Reference.CategoryContingent> CategoryContingent { get; set; }
		/// <summary>
		/// Контингент
		/// </summary>
		public DbSet<Sbor.Reference.Contingent> Contingent { get; set; }
		/// <summary>
		/// НПА
		/// </summary>
		public DbSet<Sbor.Tablepart.Activity_RegulatoryAct> Activity_RegulatoryAct { get; set; }
		/// <summary>
		/// Дополнительная информация
		/// </summary>
		public DbSet<Sbor.Tablepart.Activity_ExtInfo> Activity_ExtInfo { get; set; }
		/// <summary>
		/// Реестр мероприятий
		/// </summary>
		public DbSet<Sbor.Document.RegisterActivity> RegisterActivity { get; set; }
		/// <summary>
		/// Коды РО
		/// </summary>
		public DbSet<Sbor.Tablepart.Activity_CodeAuthority> Activity_CodeAuthority { get; set; }
		/// <summary>
		/// Показатели
		/// </summary>
		public DbSet<Sbor.Tablepart.Activity_Indicator> Activity_Indicator { get; set; }
		/// <summary>
		/// Мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.RegisterActivity_Activity> RegisterActivity_Activity { get; set; }
		/// <summary>
		/// Государственная программа
		/// </summary>
		public DbSet<Sbor.Document.StateProgram> StateProgram { get; set; }
		/// <summary>
		/// Типы ответственных исполнителей
		/// </summary>
		public DbSet<Sbor.Reference.ResponsibleExecutantType> ResponsibleExecutantType { get; set; }
		/// <summary>
		/// Элементы СЦ
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_SystemGoalElement> StateProgram_SystemGoalElement { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_GoalIndicator> StateProgram_GoalIndicator { get; set; }
		/// <summary>
		/// Соисполнители
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_CoExecutor> StateProgram_CoExecutor { get; set; }
		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_ResourceMaintenance> StateProgram_ResourceMaintenance { get; set; }
		/// <summary>
		/// Перечень подпрограмм
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_ListSubProgram> StateProgram_ListSubProgram { get; set; }
		/// <summary>
		/// Ресурсное обеспечение подпрограмм
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_SubProgramResourceMaintenance> StateProgram_SubProgramResourceMaintenance { get; set; }
		/// <summary>
		/// ВЦП и основные мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_DepartmentGoalProgramAndKeyActivity> StateProgram_DepartmentGoalProgramAndKeyActivity { get; set; }
		/// <summary>
		/// Ресурсное обеспечение ВЦП и основных мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_DGPKAResourceMaintenance> StateProgram_DGPKAResourceMaintenance { get; set; }
		/// <summary>
		/// Программы
		/// </summary>
		public DbSet<Sbor.Registry.Program> Program { get; set; }
		/// <summary>
		/// Атрибуты программы
		/// </summary>
		public DbSet<Sbor.Registry.AttributeOfProgram> AttributeOfProgram { get; set; }
		/// <summary>
		/// Ресурсное обеспечение программ
		/// </summary>
		public DbSet<Sbor.Registry.Program_ResourceMaintenance> Program_ResourceMaintenance { get; set; }
		/// <summary>
		/// Деятельность ведомства
		/// </summary>
		public DbSet<Sbor.Document.ActivityOfSBP> ActivityOfSBP { get; set; }
		/// <summary>
		/// Долгосрочная целевая программа
		/// </summary>
		public DbSet<Sbor.Document.LongTermGoalProgram> LongTermGoalProgram { get; set; }
		/// <summary>
		/// Элементы СЦ
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_SystemGoalElement> LongTermGoalProgram_SystemGoalElement { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator> LongTermGoalProgram_GoalIndicator { get; set; }
		/// <summary>
		/// Соисполнители
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_CoExecutor> LongTermGoalProgram_CoExecutor { get; set; }
		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance> LongTermGoalProgram_ResourceMaintenance { get; set; }
		/// <summary>
		/// Перечень подпрограмм
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_ListSubProgram> LongTermGoalProgram_ListSubProgram { get; set; }
		/// <summary>
		/// Ресурсное обеспечение подпрограмм
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance> LongTermGoalProgram_SubProgramResourceMaintenance { get; set; }
		/// <summary>
		/// Мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_Activity> LongTermGoalProgram_Activity { get; set; }
		/// <summary>
		/// Ресурсное обеспечение мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance> LongTermGoalProgram_ActivityResourceMaintenance { get; set; }
		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity> LongTermGoalProgram_IndicatorActivity { get; set; }
		/// <summary>
		/// Элементы СЦ
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_SystemGoalElement> ActivityOfSBP_SystemGoalElement { get; set; }
		/// <summary>
		/// Целевые показатели
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_GoalIndicator> ActivityOfSBP_GoalIndicator { get; set; }
		/// <summary>
		/// Ресурсное обеспечение
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance> ActivityOfSBP_ResourceMaintenance { get; set; }
		/// <summary>
		/// Мероприятия
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_Activity> ActivityOfSBP_Activity { get; set; }
		/// <summary>
		/// Ресурсное обеспечение мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance> ActivityOfSBP_ActivityResourceMaintenance { get; set; }
		/// <summary>
		/// Показатели качества мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity> ActivityOfSBP_IndicatorQualityActivity { get; set; }
		/// <summary>
		/// Набор задач
		/// </summary>
		public DbSet<Sbor.Registry.TaskCollection> TaskCollection { get; set; }
		/// <summary>
		/// Объемы задач
		/// </summary>
		public DbSet<Sbor.Registry.TaskVolume> TaskVolume { get; set; }
		/// <summary>
		/// Показатели качества задач
		/// </summary>
		public DbSet<Sbor.Registry.TaskIndicatorQuality> TaskIndicatorQuality { get; set; }
		/// <summary>
		/// Целевые показатели - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_GoalIndicator_Value> StateProgram_GoalIndicator_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_ResourceMaintenance_Value> StateProgram_ResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение подпрограмм - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_SubProgramResourceMaintenance_Value> StateProgram_SubProgramResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Мероприятия - значения объемов
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_Activity_Value> ActivityOfSBP_Activity_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение мероприятий - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_ActivityResourceMaintenance_Value> ActivityOfSBP_ActivityResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Целевые показатели - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_GoalIndicator_Value> ActivityOfSBP_GoalIndicator_Value { get; set; }
		/// <summary>
		/// Показатели качества мероприятий - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_IndicatorQualityActivity_Value> ActivityOfSBP_IndicatorQualityActivity_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_ResourceMaintenance_Value> ActivityOfSBP_ResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Мероприятия - значения объемов
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_Activity_Value> LongTermGoalProgram_Activity_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение мероприятий - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_ActivityResourceMaintenance_Value> LongTermGoalProgram_ActivityResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Целевые показатели - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_GoalIndicator_Value> LongTermGoalProgram_GoalIndicator_Value { get; set; }
		/// <summary>
		/// Показатели качества мероприятий - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_IndicatorActivity_Value> LongTermGoalProgram_IndicatorActivity_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_ResourceMaintenance_Value> LongTermGoalProgram_ResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение подпрограмм - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.LongTermGoalProgram_SubProgramResourceMaintenance_Value> LongTermGoalProgram_SubProgramResourceMaintenance_Value { get; set; }
		/// <summary>
		/// Ресурсное обеспечение ВЦП и основных мероприятий - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.StateProgram_DGPKAResourceMaintenance_Value> StateProgram_DGPKAResourceMaintenance_Value { get; set; }
		/// <summary>
		/// КФО
		/// </summary>
		public DbSet<Sbor.Reference.KFO> KFO { get; set; }
		/// <summary>
		/// Коды субсидий
		/// </summary>
		public DbSet<Sbor.Reference.CodeSubsidy> CodeSubsidy { get; set; }
		/// <summary>
		/// ДФК
		/// </summary>
		public DbSet<Sbor.Reference.DFK> DFK { get; set; }
		/// <summary>
		/// ДЭК
		/// </summary>
		public DbSet<Sbor.Reference.DEK> DEK { get; set; }
		/// <summary>
		/// ДКР
		/// </summary>
		public DbSet<Sbor.Reference.DKR> DKR { get; set; }
		/// <summary>
		/// КОСГУ
		/// </summary>
		public DbSet<Sbor.Reference.KOSGU> KOSGU { get; set; }
		/// <summary>
		/// КЦСР
		/// </summary>
		public DbSet<Sbor.Reference.KCSR> KCSR { get; set; }
		/// <summary>
		/// РзПР
		/// </summary>
		public DbSet<Sbor.Reference.RZPR> RZPR { get; set; }
		/// <summary>
		/// Спрос и мощность мероприятий
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity> ActivityOfSBP_ActivityDemandAndCapacity { get; set; }
		/// <summary>
		/// Спрос и мощность мероприятий - значения
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_ActivityDemandAndCapacity_Value> ActivityOfSBP_ActivityDemandAndCapacity_Value { get; set; }
		/// <summary>
		/// Формулы расчета
		/// </summary>
		public DbSet<Sbor.Reference.CalculationFormula> CalculationFormula { get; set; }
		/// <summary>
		/// Показатели расчета
		/// </summary>
		public DbSet<Sbor.Reference.IndicatorCalculation> IndicatorCalculation { get; set; }
		/// <summary>
		/// История создания бланков доведения и формирования
		/// </summary>
		public DbSet<Sbor.Tablepart.SBP_BlankHistory> SBP_BlankHistory { get; set; }
		/// <summary>
		/// Актуальные бланки формирования
		/// </summary>
		public DbSet<Sbor.Tablepart.ActivityOfSBP_SBPBlankActual> ActivityOfSBP_SBPBlankActual { get; set; }
		/// <summary>
		/// Правила отнесения расходов на код РО по КОСГУ
		/// </summary>
		public DbSet<Sbor.Reference.RuleReferExpenceAsRoByKOSGU> RuleReferExpenceAsRoByKOSGU { get; set; }
		/// <summary>
		/// Тест работы с файлами
		/// </summary>
		public DbSet<Sbor.Reference.TestFile> TestFile { get; set; }
		/// <summary>
		/// Показатели финансового состояния учреждений
		/// </summary>
		public DbSet<Sbor.Reference.FinancialIndicator> FinancialIndicator { get; set; }
		/// <summary>
		/// Показатели вышестоящей цели
		/// </summary>
		public DbSet<Sbor.Tablepart.SystemGoal_GoalIndicatorParent> SystemGoal_GoalIndicatorParent { get; set; }
		/// <summary>
		/// Предельные объемы бюджетных ассигнований
		/// </summary>
		public DbSet<Sbor.Document.LimitBudgetAllocations> LimitBudgetAllocations { get; set; }
		/// <summary>
		/// Предельные объемы бюджетных ассигнований
		/// </summary>
		public DbSet<Sbor.Tablepart.LimitBudgetAllocations_LimitAllocations> LimitBudgetAllocations_LimitAllocations { get; set; }
		/// <summary>
		/// Контрольные соотношения
		/// </summary>
		public DbSet<Sbor.Tablepart.LimitBudgetAllocations_ControlRelation> LimitBudgetAllocations_ControlRelation { get; set; }
		/// <summary>
		/// Просмотр изменений
		/// </summary>
		public DbSet<Sbor.Tablepart.LimitBudgetAllocations_ShowChanges> LimitBudgetAllocations_ShowChanges { get; set; }
		/// <summary>
		/// Смета казенного учреждения
		/// </summary>
		public DbSet<Sbor.Document.PublicInstitutionEstimate> PublicInstitutionEstimate { get; set; }
		/// <summary>
		/// ТЧ «Мероприятия»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_Activity> PublicInstitutionEstimate_Activity { get; set; }
		/// <summary>
		/// ТЧ «Расходы»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_Expense> PublicInstitutionEstimate_Expense { get; set; }
		/// <summary>
		/// ТЧ «Методы распределения»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_DistributionMethod> PublicInstitutionEstimate_DistributionMethod { get; set; }
		/// <summary>
		/// ТЧ «Дополнительный параметр распределения»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_DistributionAdditionalParam> PublicInstitutionEstimate_DistributionAdditionalParam { get; set; }
		/// <summary>
		/// ТЧ «Мероприятия для распределения»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_DistributionActivity> PublicInstitutionEstimate_DistributionActivity { get; set; }
		/// <summary>
		/// ТЧ «Косвенные расходы»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_IndirectExpenses> PublicInstitutionEstimate_IndirectExpenses { get; set; }
		/// <summary>
		/// ТЧ «Мероприятия АУ/БУ»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_ActivityAUBU> PublicInstitutionEstimate_ActivityAUBU { get; set; }
		/// <summary>
		/// ТЧ «Расходы учредителя по мероприятиям АУ/БУ»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_FounderAUBUExpense> PublicInstitutionEstimate_FounderAUBUExpense { get; set; }
		/// <summary>
		/// ТЧ «Расходы автономных и бюджетных учреждений»
		/// </summary>
		public DbSet<Sbor.Tablepart.PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense> PublicInstitutionEstimate_AloneAndBudgetInstitutionExpense { get; set; }
		/// <summary>
		/// Скорость соединения
		/// </summary>
		public DbSet<Sbor.Reference.UserBandWidth> UserBandWidth { get; set; }
		
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
			modelBuilder.Configurations.Add(new TypeRegulatoryActMap());
			modelBuilder.Configurations.Add(new RegulatoryActMap());
			modelBuilder.Configurations.Add(new KVSRMap());
			modelBuilder.Configurations.Add(new SBPMap());
			modelBuilder.Configurations.Add(new AuthorityOfExpenseObligationMap());
			modelBuilder.Configurations.Add(new RegulatoryAct_StructuralUnitMap());
			modelBuilder.Configurations.Add(new DocTypeMap());
			modelBuilder.Configurations.Add(new ElementTypeSystemGoalMap());
			modelBuilder.Configurations.Add(new ElementTypeSystemGoal_DocumentMap());
			modelBuilder.Configurations.Add(new ModelSystemGoalMap());
			modelBuilder.Configurations.Add(new GoalIndicatorMap());
			modelBuilder.Configurations.Add(new SystemGoalMap());
			modelBuilder.Configurations.Add(new DocumentsOfSED_ItemsSystemGoalMap());
			modelBuilder.Configurations.Add(new DocumentsOfSED_GoalIndicatorMap());
			modelBuilder.Configurations.Add(new SystemGoal_GoalIndicatorMap());
			modelBuilder.Configurations.Add(new GoalTargetMap());
			modelBuilder.Configurations.Add(new ValuesGoalTargetMap());
			modelBuilder.Configurations.Add(new SBP_BlankMap());
			modelBuilder.Configurations.Add(new SBP_PlanningPeriodsInDocumentsAUBUMap());
			modelBuilder.Configurations.Add(new SystemGoal_GoalIndicatorValueMap());
			modelBuilder.Configurations.Add(new DocumentsOfSED_GoalIndicatorValueMap());
			modelBuilder.Configurations.Add(new PlanActivityMap());
			modelBuilder.Configurations.Add(new PlanActivity_PeriodsOfFinancialProvisionMap());
			modelBuilder.Configurations.Add(new PlanActivity_ActivityMap());
			modelBuilder.Configurations.Add(new PlanActivity_ActivityVolumeMap());
			modelBuilder.Configurations.Add(new PlanActivity_IndicatorQualityActivityMap());
			modelBuilder.Configurations.Add(new PlanActivity_IndicatorQualityActivityValueMap());
			modelBuilder.Configurations.Add(new PlanActivity_ActivityAUBUMap());
			modelBuilder.Configurations.Add(new PlanActivity_ActivityVolumeAUBUMap());
			modelBuilder.Configurations.Add(new PlanActivity_KBKOfFinancialProvisionMap());
			modelBuilder.Configurations.Add(new PlanActivity_RequirementsForTheTaskMap());
			modelBuilder.Configurations.Add(new PlanActivity_OrderOfControlTheExecutionTasksMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_ProgramMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_ExpenseMap());
			modelBuilder.Configurations.Add(new BalancingIFDBMap());
			modelBuilder.Configurations.Add(new BalanceConfigMap());
			modelBuilder.Configurations.Add(new BalanceConfig_FilterRuleMap());
			modelBuilder.Configurations.Add(new BalanceConfig_UserMap());
			modelBuilder.Configurations.Add(new BalanceConfig_FilterKBKMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_EstimatedLineMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_RuleIndexMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_SetShowKBKMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_ChangeHistoryMap());
			modelBuilder.Configurations.Add(new BalancingIFDB_RuleFilterKBKMap());
			modelBuilder.Configurations.Add(new ApprovalBalancingIFDBMap());
			modelBuilder.Configurations.Add(new ApprovalBalancingIFDB_BlankMap());
			modelBuilder.Configurations.Add(new FinancialAndBusinessActivitiesMap());
			modelBuilder.Configurations.Add(new FBA_ActivityMap());
			modelBuilder.Configurations.Add(new FBA_PlannedVolumeIncomeMap());
			modelBuilder.Configurations.Add(new FBA_PlannedVolumeIncome_valueMap());
			modelBuilder.Configurations.Add(new FBA_CostActivitiesMap());
			modelBuilder.Configurations.Add(new FBA_CostActivities_valueMap());
			modelBuilder.Configurations.Add(new FBA_DistributionMethodsMap());
			modelBuilder.Configurations.Add(new FBA_DistributionAdditionalParameterMap());
			modelBuilder.Configurations.Add(new FBA_ActivitiesDistributionMap());
			modelBuilder.Configurations.Add(new FBA_IndirectCostsMap());
			modelBuilder.Configurations.Add(new FBA_IndirectCosts_valueMap());
			modelBuilder.Configurations.Add(new FBA_DepartmentActivityGoalMap());
			modelBuilder.Configurations.Add(new FBA_FinancialIndicatorsInstitutionsMap());
			modelBuilder.Configurations.Add(new UnitDimensionMap());
			modelBuilder.Configurations.Add(new AnalyticalCodeStateProgramMap());
			modelBuilder.Configurations.Add(new KVRMap());
			modelBuilder.Configurations.Add(new TemplateJustificationMap());
			modelBuilder.Configurations.Add(new ExpenditureObligationsMap());
			modelBuilder.Configurations.Add(new ExpenditureObligations_RegulatoryActMap());
			modelBuilder.Configurations.Add(new ExpenditureObligations_RegulatoryAct_StructuralUnitMap());
			modelBuilder.Configurations.Add(new TestDocumentMap());
			modelBuilder.Configurations.Add(new TestReg1Map());
			modelBuilder.Configurations.Add(new TestReg2Map());
			modelBuilder.Configurations.Add(new TestDocument2Map());
			modelBuilder.Configurations.Add(new TestDocumentTPMap());
			modelBuilder.Configurations.Add(new TestDocumentTP2Map());
			modelBuilder.Configurations.Add(new TestObjectMap());
			modelBuilder.Configurations.Add(new TestDocument3Map());
			modelBuilder.Configurations.Add(new SystemGoalElementMap());
			modelBuilder.Configurations.Add(new AttributeOfSystemGoalElementMap());
			modelBuilder.Configurations.Add(new DocumentsOfSEDMap());
			modelBuilder.Configurations.Add(new EstimatedLineMap());
			modelBuilder.Configurations.Add(new LimitVolumeAppropriationsMap());
			modelBuilder.Configurations.Add(new RegisterActivity_IndicatorActivityMap());
			modelBuilder.Configurations.Add(new RegisterActivity_PerformersMap());
			modelBuilder.Configurations.Add(new BranchCodeMap());
			modelBuilder.Configurations.Add(new PermissionsInputAdditionalRequirementsMap());
			modelBuilder.Configurations.Add(new FinanceSourceMap());
			modelBuilder.Configurations.Add(new ActivityMap());
			modelBuilder.Configurations.Add(new IndicatorActivityMap());
			modelBuilder.Configurations.Add(new CategoryContingentMap());
			modelBuilder.Configurations.Add(new ContingentMap());
			modelBuilder.Configurations.Add(new Activity_RegulatoryActMap());
			modelBuilder.Configurations.Add(new Activity_ExtInfoMap());
			modelBuilder.Configurations.Add(new RegisterActivityMap());
			modelBuilder.Configurations.Add(new Activity_CodeAuthorityMap());
			modelBuilder.Configurations.Add(new Activity_IndicatorMap());
			modelBuilder.Configurations.Add(new RegisterActivity_ActivityMap());
			modelBuilder.Configurations.Add(new StateProgramMap());
			modelBuilder.Configurations.Add(new ResponsibleExecutantTypeMap());
			modelBuilder.Configurations.Add(new StateProgram_SystemGoalElementMap());
			modelBuilder.Configurations.Add(new StateProgram_GoalIndicatorMap());
			modelBuilder.Configurations.Add(new StateProgram_CoExecutorMap());
			modelBuilder.Configurations.Add(new StateProgram_ResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new StateProgram_ListSubProgramMap());
			modelBuilder.Configurations.Add(new StateProgram_SubProgramResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new StateProgram_DepartmentGoalProgramAndKeyActivityMap());
			modelBuilder.Configurations.Add(new StateProgram_DGPKAResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new ProgramMap());
			modelBuilder.Configurations.Add(new AttributeOfProgramMap());
			modelBuilder.Configurations.Add(new Program_ResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new ActivityOfSBPMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgramMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_SystemGoalElementMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_GoalIndicatorMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_CoExecutorMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_ResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_ListSubProgramMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_SubProgramResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_ActivityMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_ActivityResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_IndicatorActivityMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_SystemGoalElementMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_GoalIndicatorMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ActivityMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ActivityResourceMaintenanceMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_IndicatorQualityActivityMap());
			modelBuilder.Configurations.Add(new TaskCollectionMap());
			modelBuilder.Configurations.Add(new TaskVolumeMap());
			modelBuilder.Configurations.Add(new TaskIndicatorQualityMap());
			modelBuilder.Configurations.Add(new StateProgram_GoalIndicator_ValueMap());
			modelBuilder.Configurations.Add(new StateProgram_ResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new StateProgram_SubProgramResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_Activity_ValueMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ActivityResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_GoalIndicator_ValueMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_IndicatorQualityActivity_ValueMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_Activity_ValueMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_ActivityResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_GoalIndicator_ValueMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_IndicatorActivity_ValueMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_ResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new LongTermGoalProgram_SubProgramResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new StateProgram_DGPKAResourceMaintenance_ValueMap());
			modelBuilder.Configurations.Add(new KFOMap());
			modelBuilder.Configurations.Add(new CodeSubsidyMap());
			modelBuilder.Configurations.Add(new DFKMap());
			modelBuilder.Configurations.Add(new DEKMap());
			modelBuilder.Configurations.Add(new DKRMap());
			modelBuilder.Configurations.Add(new KOSGUMap());
			modelBuilder.Configurations.Add(new KCSRMap());
			modelBuilder.Configurations.Add(new RZPRMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ActivityDemandAndCapacityMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_ActivityDemandAndCapacity_ValueMap());
			modelBuilder.Configurations.Add(new CalculationFormulaMap());
			modelBuilder.Configurations.Add(new IndicatorCalculationMap());
			modelBuilder.Configurations.Add(new SBP_BlankHistoryMap());
			modelBuilder.Configurations.Add(new ActivityOfSBP_SBPBlankActualMap());
			modelBuilder.Configurations.Add(new RuleReferExpenceAsRoByKOSGUMap());
			modelBuilder.Configurations.Add(new TestFileMap());
			modelBuilder.Configurations.Add(new FinancialIndicatorMap());
			modelBuilder.Configurations.Add(new SystemGoal_GoalIndicatorParentMap());
			modelBuilder.Configurations.Add(new LimitBudgetAllocationsMap());
			modelBuilder.Configurations.Add(new LimitBudgetAllocations_LimitAllocationsMap());
			modelBuilder.Configurations.Add(new LimitBudgetAllocations_ControlRelationMap());
			modelBuilder.Configurations.Add(new LimitBudgetAllocations_ShowChangesMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimateMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_ActivityMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_ExpenseMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_DistributionMethodMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_DistributionAdditionalParamMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_DistributionActivityMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_IndirectExpensesMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_ActivityAUBUMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_FounderAUBUExpenseMap());
			modelBuilder.Configurations.Add(new PublicInstitutionEstimate_AloneAndBudgetInstitutionExpenseMap());
			modelBuilder.Configurations.Add(new UserBandWidthMap());
			 
			CustomOnModelCreating(modelBuilder);
			base.OnModelCreating(modelBuilder);
		}

		partial void CustomOnModelCreating(DbModelBuilder modelBuilder);
		static partial void OnStaticConstruct();
	}
}