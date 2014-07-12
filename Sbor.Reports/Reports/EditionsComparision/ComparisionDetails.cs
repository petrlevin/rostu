using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.EditionsComparision;

namespace Sbor.Reports.EditionsComparision
{
    /// <summary>
    /// ToDo{SBORIII-1525} удалить данный класс
    /// </summary>
    public class ComparisionDetails
    {
        public static List<DSComparision> Get(ResultItem resultItem, TablepartInfo tpInfo)
        {
            var instance = new ComparisionDetails()
                {
                    resultItem = resultItem,
                    tpInfo = tpInfo
                };
            return instance.get();
        }

        private ComparisionDetails()
        {
        }

        private ResultItem resultItem;
        private TablepartInfo tpInfo;

        private List<DSComparision> get()
        {
            var result = new List<DSComparision>();
            var reportItem = new DSComparision()
                {
                    //Id = 
                    AttributeName = getCaption()
                };
            result.Add(reportItem);
            return result;
        }

        private string getCaption()
        {
            return ""; //ToDo{SBORIII-1525} удалить данный класс
            //var row = resultItem.RowA ?? resultItem.RowB;
            //if (row ==  null)
            //    return "(-)";

            //if (tpInfo.HasCaptionField)
            //{
            //    return row[tpInfo.CaptionFieldName].ToString();
            //}

            //string result = "";
            //foreach (var field in tpInfo.TablepartEntity.Fields)
            //{
            //    result += row[field.Name] + ", ";
            //}
            //return result;
        }
    }
}
