using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Utils.GenericTree;

namespace Platform.BusinessLogic.EditionsComparision.Extensions
{
    public static class TreeExtensions
    {
        /// <summary>
        /// Выполнить действие <paramref name="action"/> на всех элементах дерева.
        /// Стратегия обхода: Обход дерева начиная с корневых элементов. Для каждого элемента сразу же идет обход нижестоящих.
        /// </summary>
        /// <param name="action"></param>
        public static void Exec<T>(this Tree<T> tree, Action<T> action) where T : class 
        {
            exec(tree, action);
        }
        private static void exec<T>(List<Node<T>> list, Action<T> action) where T : class
        {
            foreach (Node<T> node in list)
            {
                action(node.Obj);
                exec(node.Children, action);
            }
        }


        public static void Exec<T>(this Tree<T> tree, Action<T, T> action) where T : class 
        {
            exec(tree, action, null);
        }
        private static void exec<T>(List<Node<T>> list, Action<T, T> action, T parent) where T : class
        {
            foreach (Node<T> node in list)
            {
                action(node.Obj, parent);
                exec(node.Children, action, node.Obj);
            }
        }


        public static void Exec<T>(this Tree<T> tree, Action<Node<T>> action) where T : class
        {
            exec(tree, action);
        }
        private static void exec<T>(List<Node<T>> list, Action<Node<T>> action) where T: class 
        {
            foreach (Node<T> node in list)
            {
                action(node);
                exec(node.Children, action);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tree"></param>
        /// <param name="action">узел и цепочка родителей</param>
        public static void Exec<T>(this Tree<T> tree, Action<T, LinkedList<T>> action) where T : class
        {
            exec(tree, action, new LinkedList<T>());
        }
        private static void exec<T>(List<Node<T>> list, Action<T, LinkedList<T>> action, LinkedList<T> parentsChain) where T : class
        {
            foreach (Node<T> node in list)
            {
                action(node.Obj, parentsChain);

                var chain = new LinkedList<T>(parentsChain);
                chain.AddLast(node.Obj);
                exec(node.Children, action, chain);
            }
        }
    }
}
