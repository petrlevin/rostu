using BaseApp.Common.Exceptions;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace BaseApp.Rights.Functional
{
    public static class RightsManagerExtension
    {
        /// <summary>
        /// Проверка на редактирование
        /// </summary>
        /// <param name="rightHolderId"></param>
        /// <param name="itemId"></param>
        /// <param name="rightsManager"></param>
        /// <exception cref="FunctionalRightsException"></exception>
        public static void ValidateWrite(this RightsManager rightsManager, int rightHolderId, int? itemId=null)
        {
            rightsManager.ValidateWrite(Objects.ById<Entity>(rightHolderId), itemId);
        }

    }
}
