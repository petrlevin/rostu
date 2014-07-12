using BaseApp.DataAccess;
using BaseApp.Service;
using Platform.BusinessLogic.DataAccess;

namespace Platform.Web.Services
{
    /// <summary>
    /// Сервис для получения системных измерений
    /// </summary>
    /// <remarks>
    /// Вынесен отдельно от <see cref="DataService"/> поскольку при любых обращениях к <see cref="DataService"/> необходимы установленные системные измерения.
    /// </remarks>
    public class SysDimensionsService : DataService
    {
        /// <summary>
        /// Получить список элементов типа системного измерения
        /// </summary>
        /// <param name="param">Параметры списка</param>
        /// <returns></returns>
        public new GridResult GridSource(GridParams param)
        {
            param.Limit = 0;
            param.Page = 0;

            DecoratorsManager.RegisterSysDimensionDecorators(param);

            return base.GridSource(param);
        }
    }
}