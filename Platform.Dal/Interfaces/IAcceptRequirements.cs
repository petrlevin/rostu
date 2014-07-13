using System.Collections.Generic;

namespace Platform.Dal.Interfaces
{
   /// <summary>
   /// Интерфейс для декораторов которые принимают требования
   /// </summary>
    public interface IAcceptRequirements
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns>Возвращает  требования кторые будут переданы следующему декоратору</returns>
        IEnumerable<IRequirement>  Accept(IEnumerable<IRequirement> requirements);
    }
}
