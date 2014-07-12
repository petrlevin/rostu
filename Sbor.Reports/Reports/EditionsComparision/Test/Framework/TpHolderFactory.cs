using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.GenericTree;
using Sbor.Reports.EditionsComparision.Test;
using Sbor.Reports.EditionsComparision.Test.Framework;

namespace Sbor.Reports.Reports.EditionsComparision.Test.Framework
{
    public class TpHolderFactory<T>
    {
        public static Node<TpDataHolder> Get(List<T> dataA, List<T> dataB)
        {
            var instance = new TpHolderFactory<T>()
                {
                    dataA = dataA,
                    dataB = dataB
                };
            return instance.get();
        }

        private TpHolderFactory()
        {
            type = typeof (T);
        }

        private Type type;
        private List<T> dataA;
        private List<T> dataB;

        private Node<TpDataHolder> get()
        {
            TablefieldAttribute attribute = type.GetCustomAttribute<TablefieldAttribute>();
            
            var tpInfo = new TablepartInfo()
            {
                MasterFieldName = attribute.MasterField,
                CaptionFieldName = attribute.CaptionField,
                Field = new EntityField()
                {
                    Name = attribute.FieldName,
                    Caption = attribute.FieldName
                },
                Fields = getFields()
            };
            tpInfo.setIgnoredFields();

            var result = new TestTpDataHolder(tpInfo);
            result.SetTables(ClassToDataTable<T>.Get(dataA), ClassToDataTable<T>.Get(dataB));
            return new Node<TpDataHolder>(result);
        }

        private List<IEntityField> getFields()
        {
            var result = new List<IEntityField>();
            
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                var field = new EntityField();
                field.Name = propertyInfo.Name;
                field.Caption = propertyInfo.Name;
                if (propertyInfo.PropertyType == typeof(int))
                {
                    field.IdEntityFieldType = (byte) EntityFieldType.Int;
                }
                else
                {
                    field.IdEntityFieldType = (byte)EntityFieldType.String;
                }
                result.Add(field);
            }
            return result;
        }
    }
}
