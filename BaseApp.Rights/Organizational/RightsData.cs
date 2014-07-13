using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp.Common.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;

namespace BaseApp.Rights.Organizational
{
    /// <summary>
    /// Данные необходимые для декоратора по организационным правам
    /// </summary>
    public class RightsData : IOrganizationRightData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rights">Права для основной сущности </param>
        public RightsData(IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> rights)
        {
            Rights = rights;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allRights">Права для всех сущностей необходимых для  декоратора (в том числе определяемых расширениями)</param>
        /// <param name="source">Главная сущность запроса</param>
        /// <param name="extensions">Расширения организационных прав</param>
        public RightsData(IEnumerable<IGrouping<IEntity, IGrouping<IEntityField, IOrganizationRightInfo>>> allRights, IEntity source, IEnumerable<IOrganizationRightExtension> extensions)
        {
            Rights = allRights.SingleOrDefault(g => g.Key.Id == source.Id);
            Rights = Rights ?? new List<IGrouping<IEntityField, IOrganizationRightInfo>>();

            _ofExtendedRights = new Dictionary<int, IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>>>();
            foreach (IGrouping<IEntity, IGrouping<IEntityField, IOrganizationRightInfo>> grouping in allRights)
            {
                _ofExtendedRights.Add(grouping.Key.Id, grouping);
            }
            Extensions = extensions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allRights">Права для всех сущностей необходимых для  декоратора (в том числе определяемых расширениями)</param>
        /// <param name="mainRights">Права для основной сущности </param>
        /// <param name="extensions">Расширения организационных прав</param>
        public RightsData(IEnumerable<IGrouping<IEntity, IGrouping<IEntityField, IOrganizationRightInfo>>> allRights, IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> mainRights, IEnumerable<IOrganizationRightExtension> extensions)
        {
            Rights = mainRights;
            Rights = Rights ?? new List<IGrouping<IEntityField, IOrganizationRightInfo>>();

            _ofExtendedRights = new Dictionary<int, IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>>>();
            foreach (IGrouping<IEntity, IGrouping<IEntityField, IOrganizationRightInfo>> grouping in allRights)
            {
                _ofExtendedRights.Add(grouping.Key.Id, grouping);
            }
            Extensions = extensions;
        }



        /// <summary>
        /// Права для основной сущности 
        /// </summary>
        public IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> Rights { get; private set; }
        /// <summary>
        /// Расширения организационных прав
        /// </summary>
        public IEnumerable<IOrganizationRightExtension> Extensions { get; private set; }

        /// <summary>
        /// Права для сущности <paramref name="entityId"></paramref>
        /// </summary>
        /// <param name="entityId"></param>
        public IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>> this[int entityId]
        {
            get
            {
                if (_ofExtendedRights.ContainsKey(entityId))
                    return _ofExtendedRights[entityId];
                else
                {
                    return new List<IGrouping<IEntityField, IOrganizationRightInfo>>();
                }
            }
        }

        private readonly Dictionary<int, IEnumerable<IGrouping<IEntityField, IOrganizationRightInfo>>> _ofExtendedRights;
    }
}
