using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.GenericTree
{
    public class Node<T>
    {
        public Node()
        {
            Children = new List<Node<T>>();
        }

        public Node(T obj): this()
        {
            Obj = obj;
        }

        public T Obj { get; set; }
        public List<Node<T>> Children { get; set; } 

        public void AddChild(T item)
        {
            this.Children.Add(new Node<T>(item));
        }
    }
}
