using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.GenericTree
{

    public static class ListNodeExtension
    {
        public static void AddItem<T>(this List<Node<T>> list, T item)
        {
            list.Add(new Node<T>(item));
        }
    }

    public class Tree<T>: List<Node<T>>
    {
        public Tree()
        {
        }

        public Tree(List<Node<T>> items): base(items)
        {
        }

        public Tree<TResult> Cast<TResult>(Func<T,TResult> converter)
        {
            return new Tree<TResult>(this.Select(node => cast(node, converter)).ToList());
        }

        public void AddItem(T item)
        {
            ListNodeExtension.AddItem(this, item);
        }

        #region Private Methods

        private Node<TResult> cast<TResult>(Node<T> sourceNode, Func<T, TResult> converter)
        {
            var newNode = new Node<TResult>(converter(sourceNode.Obj));
            newNode.Children = new Tree<TResult>(sourceNode.Children.Select(node => cast(node, converter)).ToList());
            return newNode;
        }

        #endregion
    }
}
