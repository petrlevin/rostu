using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Common.Exceptions;

namespace Sbor.Reports.EditionsComparision
{
    /// <summary>
    /// ToDo{SBORIII-1525} удалить класс
    /// </summary>
    public class ReportResultBuilder
    {
        public ReportResultBuilder()
        {
            data = new List<DSComparision>();
            parents = new LinkedList<string>();
        }

        private List<DSComparision> data;

        private LinkedList<string> parents;

        /// <summary>
        /// Подняться вверх по иерархии
        /// </summary>
        public void LevelUp()
        {
            parents.RemoveLast();
        }

        /// <summary>
        /// Спуститься вниз по иерархии
        /// </summary>
        public void LevelDown()
        {
            string newParentId = data.Last().Id;
            if (string.IsNullOrEmpty(newParentId))
                throw new PlatformException("Последняя созданная строка не имеет идентификатора.");
            parents.AddLast(data.Last().Id);
        }

        public void AddNext(List<DSComparision> items)
        {
            foreach (var item in items)
            {
                item.Parent = parents.Last.Value;
            }
        }

        public List<DSComparision> GetData()
        {
            return data;
        }

        // Private 


    }
}
