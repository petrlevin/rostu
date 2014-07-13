using Platform.ClientInteraction;
using Platform.Web.Services;

namespace AutoClient.Helpers
{
    /// <summary>
    /// Хелпер для работы с мультиссылками
    /// </summary>
    public class MultilinkHelper
    {
        public Redirector Redirector { get; set; }

        /// <summary>
        /// id сущности открытого элемента
        /// </summary>
        public int OwnerEntityId;
        
        /// <summary>
        /// id сущности мультиссылки
        /// </summary>
        public int EntityId;

        /// <summary>
        /// id открытого элемента сущности, в которой находится поле мультиссылки
        /// </summary>
        public int? IdEntityItem;

        public void Add(int itemId)
        {
            Redirector.DoRequest<CommunicationDataService>(service => service.CreateMultilink(null, EntityId, OwnerEntityId, IdEntityItem, new int[] { itemId }));
        }

        public void Delete(int itemId)
        {
            Redirector.DoRequest<CommunicationDataService>(service => service.DeleteMultilink(new CommunicationContext(), EntityId, OwnerEntityId, (int)IdEntityItem, new int[] { itemId }));
        }
    }
}
