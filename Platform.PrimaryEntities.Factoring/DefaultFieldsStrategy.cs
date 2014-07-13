using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Application.Common;
using Platform.PrimaryEntities.Reference;

namespace Platform.PrimaryEntities.Factoring
{
    internal class DefaultFieldsStrategy : Entity.Mixed
    {
        [ThreadStatic]
        static private List<Int32> _needRefresh;

        public override IEnumerable<Common.Interfaces.IEntityField> Get(Entity entity)
        {
            if (_needRefresh ==null)
                _needRefresh = new List<int>();
            if (!_needRefresh.Contains(entity.Id))
            {
                base.Set(entity, null);
                _needRefresh.Add(entity.Id);
            }
            return base.Get(entity);
        }

        public DefaultFieldsStrategy()
        {
            Application.Application.BeginRequest += () => { if (_needRefresh != null) _needRefresh.Clear(); };
        }
        public class StartUp : IAfterAplicationStart
        {
            public void Execute()
            {
                Instance = new DefaultFieldsStrategy();
            }
        }

        
    }
}
