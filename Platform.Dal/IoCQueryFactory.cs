using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils.FactoryPattern;

namespace Platform.Dal
{
    public class IoCQueryFactory : QueryFactory
    {
        public IoCQueryFactory(EntityType entityType, IEntity entity) : base(entityType, entity)
        {
        }

        public IoCQueryFactory(IEntity entity) : base(entity)
        {
        }

        protected override void InitFactories()
        {
            selectFactory =
                new IoCFactory<EntityType, ISelectQueryBuilder>();
            insertFactory =
                new IoCFactory<EntityType, IInsertQueryBuilder>();
            updateFactory =
                new IoCFactory<EntityType, IUpdateQueryBuilder>();
            deleteFactory =
                new IoCFactory<EntityType, IDeleteQueryBuilder>();

        }
    }
}
