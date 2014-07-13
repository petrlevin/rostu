using System.Collections.Generic;

namespace Platform.BusinessLogic.Denormalizer.Interfaces
{
	/// <summary>
	/// Интерфейс, предназначенный для реализации сущностью-владельцем (!а не самой ТЧ), в которой содержится одна или несколько табличных частей, подлежащих денормализации
	/// </summary>
	public interface IColumnFactoryForDenormalizedTablepart
	{
        /// <summary>
        /// Получить коллекцию идентификаторов периодов (сущность "Иерархия периодов"), для каждого из которого в денормализованной ТЧ будет создана колонка.
        /// </summary>
        /// <param name="tablepartEntityName">Имя родительской сущности денормализованной ТЧ</param>
        /// <returns></returns>
		ColumnsInfo GetColumns(string tablepartEntityName);
	}
}
