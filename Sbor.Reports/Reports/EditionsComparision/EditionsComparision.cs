using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EditionsComparision;
using Platform.BusinessLogic.EditionsComparision.Extensions;
using Platform.BusinessLogic.ReportingServices.Reports;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Unity;
using Platform.Utils.GenericTree;
using Sbor.Reports.EditionsComparision;
using Sbor.Reports.EditionsComparision.Test;
using Sbor.Reports.Reports.EditionsComparision;

namespace Sbor.Reports.Report
{
    //  отчет "Сравнение редакций"
    [Report]
    public partial class EditionsComparision
    {
        private Tree<IReportItem> ReportTree;

        private List<DSComparision> Report;

        public List<DSComparision> DataSetComparision()
        {
            //IdEditionAEntity = -1207959525; // PublicInstitutionEstimate	Смета казенного учреждения
            //IdEditionA = 153; Смета казенного учреждения № 214 от 24.07.2013
            //IdEditionB = 361; Смета казенного учреждения № 214.1 от 29.07.2013

            EditionsComparator comparator = new EditionsComparator()
            {
                EntityId = IdEditionAEntity.Value,
                EditionA = IdEditionA.Value,
                EditionB = IdEditionB.Value
            };
            //comparator = new TestEditionsComparator();

            comparator.Compare();

            // представляем результат в виде дерева
            ReportTree = new Tree<IReportItem>();
            var root = new Node<IReportItem>(new DatarowReportItem(comparator.MainTableResult, comparator.MainTableInfo));
            root.Children.AddRange(processTree(null, comparator.HoldersTree));
            ReportTree.Add(root);

            // дерево результата преобразуем в дерево данных отчета
            Report = new List<DSComparision>();
            ReportTree.Exec(buildReport);

            return Report;
        }

        #region Алгоритм построения отчета на основе результата работы EditionsComparator

        private List<Node<IReportItem>> processTree(ResultItem parentResultItem, List<Node<TpDataHolder>> nodes)
        {
            var childrenTp = new List<Node<IReportItem>>();

            // цикл по табличным частям
            foreach (Node<TpDataHolder> node in nodes)
            {
                Node<IReportItem> tablepartResultNode = new Node<IReportItem>(new TablepartReportItem(node.Obj.TpInfo));
                
                // цикл по строкам табличной части
                foreach (ResultItem resultItem in node.Obj.ResultByMaster(parentResultItem).OrderBy(orderer))
                {
                    Node<IReportItem> datarowResultNode = new Node<IReportItem>(new DatarowReportItem(resultItem, node.Obj.TpInfo));

                    List<Node<IReportItem>> childrenRecursive = new List<Node<IReportItem>>();
                    if (resultItem.ItemType != ResultItemType.Deleted) // для удаленных элементов не отображаем детализацию по дочерним ТЧ
                        childrenRecursive = processTree(resultItem, node.Children);
                    
                    if (resultItem.ItemType != ResultItemType.Unchanged || childrenRecursive.Any())
                    {
                        if (childrenRecursive.Any())
                            datarowResultNode.Children.AddRange(childrenRecursive);
                        tablepartResultNode.Children.Add(datarowResultNode);
                    }
                }

                if (tablepartResultNode.Children.Any()) // если в ТЧ нет строк, то информацию о ней в результирующее дерево не добавляем
                    childrenTp.Add(tablepartResultNode);
            }
            return childrenTp;
        }

        private void buildReport(IReportItem item, LinkedList<IReportItem> parentsChain)
        {
            string tpl = "{0}-{1}";
            string parentId = parentsChain.Select(repItem => repItem.Id).DefaultIfEmpty().Aggregate((a, b) => string.Format(tpl, a, b));
            string id = parentId == null ? item.Id : string.Format(tpl, parentId, item.Id);
            
            Report.AddRange(item.GetData(id, parentId));
        }

        private int orderer(ResultItem item)
        {
            switch (item.ItemType)
            {
                case ResultItemType.Unchanged:
                    return 1;
                case ResultItemType.Changed:
                    return 2;
                case ResultItemType.Added:
                    return 3;
                case ResultItemType.Deleted:
                    return 4;
                default:
                    return 5;
            }
        }

        #endregion


        #region Тестирование

        private List<DSComparision> getTestData()
        {
            return new List<DSComparision>() 
            {
                                
                new DSComparision()
                {
                    Id = "0",
                    Parent = null,
                    AttributeName = "AttributeName",
                    ValueA = "ValueA",
                    ValueB = "ValueB"
                },
                                
                new DSComparision()
                {
                    Id = "1",
                    Parent = "0",
                    AttributeName = "AttributeName",
                    ValueA = "ValueA",
                    ValueB = "ValueB"
                }

            };
        }

        #endregion
    }
}
