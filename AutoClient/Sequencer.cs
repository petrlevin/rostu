using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClient
{
    /// <summary>
    /// Класс для последовательного вызова некоторого метода с аргументами из массива.
    /// </summary>
    public class Sequencer
    {
        public static void Do<T, TArgs>(T obj, Action<T, TArgs> action, List<TArgs> argsList)
        {
            foreach (TArgs args in argsList)
            {
                action(obj, args);
            }
        }
    }
}
