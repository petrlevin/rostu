using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Sbor.Reports.EditionsComparision;
using Platform.PrimaryEntities.Common.Extensions;
using Platform.BusinessLogic.CaptionExpressions.Extensions;

namespace Sbor.Reports.Reports.EditionsComparision
{
    public class DatarowReportItem: IReportItem
    {
        private ResultItem resultItem;
        private ITableInfo tpInfo;

        public DatarowReportItem(ResultItem resultItem, ITableInfo tpInfo, bool hideCaption = false)
        {
            this.resultItem = resultItem;
            this.tpInfo = tpInfo;
            HideCaption = hideCaption;
        }

        /// <summary>
        /// Скрывать заголовок строки.
        /// Строка, представляющая собой наименование записи таблицы и расположенная над строками полей со значениями, является заголовком.
        /// </summary>
        public bool HideCaption { get; set; }

        public string Id
        {
            get
            {
                string result = string.Empty;
                if (resultItem.IdA != null)
                    result += resultItem.IdA.ToString();
                if (resultItem.IdB != null)
                    result += resultItem.IdB.ToString();
                if (string.IsNullOrEmpty(result))
                    result = "-пусто-";
                return result;
            }
        }

        public IEnumerable<DSComparision> GetData(string itemId, string parentId)
        {
            Func<DSComparision, IEntityField, DSComparision> reportRowGetter;

            switch (resultItem.ItemType)
            {
                case ResultItemType.Added:
                    reportRowGetter = getAdded;
                    break;
                case ResultItemType.Deleted:
                    reportRowGetter = getDeleted;
                    break;
                case ResultItemType.Changed:
                case ResultItemType.Unchanged:
                case ResultItemType.Different:
                    reportRowGetter = getSame;
                    break;
                default:
                    throw new PlatformException("Не учтенный тип результирующей строки");
            }

            var fieldsData = new List<DSComparision>();

            //if (!HideCaption)
                fieldsData.Add(new DSComparision()
                {
                    Id = itemId,
                    Parent = parentId,
                    AttributeName = getCaption()
                });

            foreach (IEntityField entityField in tpInfo.Fields)
            {
                var reportItem = getNewFieldReportItem(itemId, entityField);
                fieldsData.Add(reportRowGetter(reportItem, entityField));
            }

            return fieldsData;
        }

        private DSComparision getNewFieldReportItem(string parentId, IEntityField entityField)
        {
            var result = new DSComparision()
            {
                Id = string.Format("{0}-{1}", parentId, entityField.Name),
                Parent = parentId,
                AttributeName = entityField.GetEvaluatedCaption()
            };

            if (resultItem.ItemType == ResultItemType.Changed)
                result.RowType = resultItem.ChangedFields.Contains(entityField.Name)
                    ? resultItem.ItemType.ToString().ToLower()
                    : string.Empty;
            else
                result.RowType = resultItem.ItemType.ToString().ToLower();
            return result;
        }

        private DSComparision getAdded(DSComparision ri, IEntityField field)
        {
            ri.ValueB = getFieldValue(resultItem.RowB, field);
            return ri;
        }

        private DSComparision getDeleted(DSComparision ri, IEntityField field)
        {
            ri.ValueA = getFieldValue(resultItem.RowA, field);
            return ri;
        }

        private DSComparision getSame(DSComparision ri, IEntityField field)
        {
            ri.ValueA = getFieldValue(resultItem.RowA, field);
            ri.ValueB = getFieldValue(resultItem.RowB, field);
            return ri;
        }

        private string getCaption()
        {
            string result = string.Empty;
            DataRow row = resultItem.RowA ?? resultItem.RowB;
            
            if (row == null)
                result = "(-)";
            else if (!string.IsNullOrEmpty(tpInfo.CaptionFieldName))
            {
                result = getFieldValue(row, tpInfo.CaptionFieldName);
            }
            else
            {
                result = tpInfo.Fields.DefaultIfEmpty()
                    .Select(ef => getFieldValue(row, ef))
                    .Aggregate((a, b) => string.Format("{0}, {1}", a, b));
            }

            result += string.Format(" ({0})", getRusItemType(resultItem.ItemType));

            return result;
        }

        private string getFieldValue(DataRow row, string fieldName)
        {
            IEntityField field = tpInfo.Fields.Single(ef => ef.Name == fieldName);
            return getFieldValue(row, field);
        }

        private string getFieldValue(DataRow row, IEntityField entityField)
        {
            if (entityField.EntityFieldType == EntityFieldType.Bool)
            {
                var value = row[entityField.Name].ToString();
                if (value.ToLower() == "true")
                    return "Истина";
                if (value.ToLower() == "false")
                    return "Ложь";
            }

            string name = entityField.IsLinkField() ? entityField.Name + "_Caption" : entityField.Name;
            return row[name].ToString();
        }

        private string getRusItemType(ResultItemType type)
        {
            switch (type)
            {
                case ResultItemType.Added:
                    return "добавлена";
                case ResultItemType.Deleted:
                    return "удалена";
                case ResultItemType.Changed:
                    return "изменена";
                case ResultItemType.Unchanged:
                    return "неизменена";
                default:
                    return type.ToString();
            }
        }
    }
}
