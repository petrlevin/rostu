using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.PrimaryEntities.DbEnums 
{
	/// <summary>
	/// Проекты решения
	/// Данное перечисление содержит перечень проектов решения (solution). 
	/// Элементы перечисления должны соответствовать наименования м проектов, точка заменяетя на подчеркивание.
	/// В справочнике Entity есть поле, ссылающееся на данное перечисление, 
	/// что позволяет соотносить сущности с проектами и при экспорте метаданных записывать их в папку DbStructure соответствующего проекта.
	/// </summary>
	public enum SolutionProject
	{
		/// <summary>
		/// Tools.MigrationHelper
		/// Системные сущности и перечисления. Данные таблицы создаются скриптом.
		/// </summary>
		Tools_MigrationHelper_Core = 100,

		/// <summary>
		/// Platform.PrimaryEntities
		/// Первичные сущности - сущности, для которых возможно создать clr-триггеры.
		/// </summary>
		Platform_PrimaryEntities = 200,

		/// <summary>
		/// Platform.PrimaryEntities.Common
		/// Интерфейсы, енумераторы первичных сущностей
		/// </summary>
		Platform_PrimaryEntities_Common = 210,

		/// <summary>
		/// Platform.BusinessLogic
		/// Статусы документов, Операции
		/// </summary>
		Platform_BusinessLogic = 250,
		
		/// <summary>
		/// BaseApp.Common
		/// </summary>
		BaseApp_Common = 300,

		/// <summary>
		/// BaseApp
		/// </summary>
		BaseApp = 400,

		/// <summary>
		/// Sbor
		/// СБОР.
		/// </summary>
		Sbor = 500,

        /// <summary>
        /// Sbor.Reports
        /// Отчеты
        /// </summary>
        Sbor_Reports = 600,

        /// <summary>
        /// Tests
        /// Проект тестовых сущностей.
        /// </summary>
        Tests = 700
	}
}
