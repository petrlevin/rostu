using System.Xml.Linq;
using BaseApp.XmlExchange.Export;
using BaseApp.XmlExchange.Import;
using Platform.BusinessLogic.AppServices;

namespace BaseApp.Service
{
    /// <summary>
    /// Сервис импорта/экспорта xml
    /// </summary>
    [AppService]
    public class XmlExchange
    {
        /// <summary>
        /// Экспорт элементов указанного типа сущности
        /// </summary>
        /// <param name="idEntity">Идентификатор сущности</param>
        /// <returns>Возвращается xml-объект в текстовом виде</returns>
        public string SimpleExport(int idEntity)
		{
			return new SimpleExporter(idEntity).Execute();
		}

        /// <summary>
        /// Импорт элементов указанного типа сущности
        /// </summary>
        /// <param name="idEntity">Идентификатор сущности</param>
        /// <param name="document">Источник-xml</param>
        /// <returns>Возвращается текстовое сообщение о результате импорта</returns>
		public string SimpleImport(int idEntity, XDocument document)
		{
			return new SimpleImporter(idEntity, document).Execute();
		}
	}
}
