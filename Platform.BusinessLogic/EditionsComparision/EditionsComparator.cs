using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision.Extensions;
using Platform.Common;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils.GenericTree;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class EditionsComparator
    {
        // Public Properies

        public int EntityId { get; set; }

        public int EditionA { get; set; }

        public int EditionB { get; set; }

        public ResultItem MainTableResult { get; private set; }

        public ITableInfo MainTableInfo { get; protected set; }

        public Tree<TpDataHolder> HoldersTree { get; protected set; }

        // CTor

        public EditionsComparator()
        {
            db = IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        // Public Methods

        public void Compare()
        {
            loadData();
            MainTableResult = mainDataHolder.Compare();
            HoldersTree.Exec((node, parent) => node.Compare(parent));
        }

        // Private Fields

        private DataContext db { get; set; }

        private MainDataHolder mainDataHolder { get; set; }

        // Private Methods

        protected virtual void loadData()
        {
            // таблица основной сущности
            MainTableInfo = new TableInfo(EntityId);
            mainDataHolder = new MainDataHolder(MainTableInfo, EditionA, EditionB);
            mainDataHolder.Fill();

            // табличные части
            Tree<TablepartInfo> fieldTree = TablepartsDependenciesResolver.GetTree(db.Entity.Single(e => e.Id == EntityId));
            HoldersTree = fieldTree.Cast(tpinfo => new TpDataHolder(tpinfo)
                {
                    OwnerItemIdA = EditionA,
                    OwnerItemIdB = EditionB
                });

            HoldersTree.Exec(node => node.Fill());
        }
    }
}
