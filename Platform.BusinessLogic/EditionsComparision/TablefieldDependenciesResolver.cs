using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.GenericTree;

namespace Platform.BusinessLogic.EditionsComparision
{
    public class TablepartsDependenciesResolver
    {
        public static Tree<TablepartInfo> GetTree(IEntity entity)
        {
            var resolver = new TablepartsDependenciesResolver(entity);
            return resolver.GetTree();
        }

        #region Private

        private IEntity Entity { get; set; }

        private Tree<TablepartInfo> GetTree()
        {
            resolveDependencies();
            var list = tableFields.Values.Where(node => node.Children.Any()).Select(node => node).ToList();
            return new Tree<TablepartInfo>(list);
        }

        private DataContext db { get; set; }

        private TablepartsDependenciesResolver(IEntity entity)
        {
            this.Entity = entity;
            db = IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        /// <summary>
        /// Все поля в одном словаре. Ключ = имя поля.
        /// После выполнения <see cref="resolveDependencies"/> у каждого указаны ссылки на дочерние поля.
        /// </summary>
        private Dictionary<string, Node<TablepartInfo>> tableFields;

        /// <summary>
        /// Создает связи в полях <see cref="tableFields"/> (указывает дочерние поля)
        /// </summary>
        private void resolveDependencies()
        {
            tableFields = getAllTablepartFields();
            foreach (TablepartInfo tableFieldInfo in tableFields.Values.Select(node => node.Obj))
            {
                if (!string.IsNullOrEmpty(tableFieldInfo.MasterTablepartfieldName))
                {
                    tableFields[tableFieldInfo.MasterTablepartfieldName].Children.Add(tableFields[tableFieldInfo.Field.Name]);
                }
            }
        }

        private Dictionary<string, Node<TablepartInfo>> getAllTablepartFields()
        {
            return db.EntityField
                .Where(ef =>
                    ef.IdEntity == Entity.Id
                    && (ef.IdEntityFieldType == (byte)EntityFieldType.Tablepart || ef.IdEntityFieldType == (byte)EntityFieldType.VirtualTablePart))
                .ToList()
                .ToDictionary(ef => ef.Name, ef => new Node<TablepartInfo>(new TablepartInfo(ef)));
        }

        #endregion

    }
}
