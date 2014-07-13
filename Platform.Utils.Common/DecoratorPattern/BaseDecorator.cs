using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Utils.DecoratorPattern
{
    abstract public class BaseDecorator<T> where T:class
    {
        public T Inner { get; protected set; }
        protected BaseDecorator(T inner)
        {
            if (inner == null) throw new ArgumentNullException("inner" ,"В конструктор декоратора не был передан внутренний объект (декорируемый) ");
            Inner = inner;
        }
    }
}
