using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using BaseApp.Common.Interfaces.DbDependecy;
using Platform.BusinessLogic.AppServices;
using Platform.Common;
using Platform.Dal;

namespace BaseApp.Service
{
    /// <summary>
    /// Сервис получения зависимостей между элементами в БД
    /// </summary>
    [AppService]
    public class DbDependencyService
    {
        private readonly SqlConnection _connection = IoC.Resolve<SqlConnection>("DbConnection");

        /// <summary>
        /// Получить зависимости
        /// </summary>
        /// <param name="id">Идентификатор элемента</param>
        /// <param name="idEntity">Идентификатор сущности</param>
        /// <returns>В текстовом виде возвращается список элементов, зависящих от указанного</returns>
        public string GetDependent(int id, int idEntity)
        {
            return _getFormatedText(id, idEntity);
        }

        private string _getFormatedText(int id, int identity)
        {
            IEnumerable<Result> resultDependences = _execCommand(id, identity);
            StringBuilder result = new StringBuilder();
            foreach (var resultDependence in resultDependences)
            {
                if (resultDependence.HeadId == 0)
                {
                    result.AppendFormat("Элемент '{0}' {1} '{2}'</br>", resultDependence.Caption, resultDependence.TypeName,
                                        resultDependence.EntityCaption);
                }
                else
                {
                    result.AppendFormat("Элемент '{0}' {1} '{2}' принадлежащий злементу '{3}' {4} '{5}'</br>", resultDependence.Caption, resultDependence.TypeName,
                                        resultDependence.EntityCaption, resultDependence.HeadCaption, resultDependence.HeadTypeName, resultDependence.HeadEntityCaption);
                }
            }
            return result.ToString();
        }
        private class Result : IResultDbDependence
        {
            public string HeadTypeName { get; set; }
            public string HeadEntityCaption { get; set; }
            public int HeadId { get; set; }
            public string HeadCaption { get; set; }
            public string TypeName { get; set; }
            public string EntityCaption { get; set; }
            public int Id { get; set; }
            public string Caption { get; set; }
        }
        private IEnumerable<Result> _execCommand(int id, int identity)
        {
			List<Result> result;
			SqlCommandFactory commandFactory = new SqlCommandFactory(string.Format("SELECT * FROM dbo.GetDependence({0}, {1})", id,
                                                                            identity), _connection);
	        using (SqlCommand command = commandFactory.CreateCommand())
	        {
				result = command.Select<Result>();
	        }
	        return result;
        }
	}
}
