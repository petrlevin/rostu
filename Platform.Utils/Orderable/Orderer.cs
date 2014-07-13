using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;

namespace Platform.Utils.Orderable
{
    public  class Orderer
    {
        #region Определение порядка применения декораторов

        /// <summary>
        /// Располагает декораторы в порядке применения
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public List<T> SetOrder<T>(List<T> source)
        {

            Item<T>.Predicate beforePredicate = (f, s) =>
                                                    {
                                                        var thisordered = f as IOrdered;
                                                        return ((thisordered != null) && (thisordered.Before != null) &&
                                                                (thisordered.Before.Contains(s.GetType())));
                                                    };

            Item<T>.Predicate afterPredicate = (f, s) =>
            {
                var thisordered = f as IOrdered;
                return ((thisordered != null) && (thisordered.After != null) &&
                        (thisordered.After.Contains(s.GetType())));
            };




            var first = Item<T>.FromList(source, beforePredicate,afterPredicate);
            
            bool wasMovings = true;

            while (wasMovings)
            {
                wasMovings = false;
                var current = first.First();
                while (current != null)
                {
                    T value = current.Value;
                    var movable = value as IOrdered;
                    if (movable != null)
                    {

                        if (movable.Before != null)
                        {

                            wasMovings =
                                current.MoveBeforeByPredicate() ||
                                wasMovings;
                            if (movable.WantBe == Order.Last)
                            {
                                var next = current.Next;
                                while (next != null)
                                {
                                    if ((!current.Before.Contains(next.Value)) &&(next.WantBe!=Order.Last))
                                    {
                                        var nextNext = next.Next;
                                        wasMovings = next.MoveBefore(current) || wasMovings;
                                        next = nextNext;
                                    }
                                    else
                                    {
                                        next = next.Next;
                                    }
                                }
                            }
                        }

                        if (movable.After != null)
                        {
                            wasMovings =
                                current.MoveAfterByPredicate() ||
                                wasMovings;
                            if (movable.WantBe == Order.First)
                            {
                                var previous = current.Previous;
                                while (previous != null)
                                {
                                    if ((!current.After.Contains(previous.Value)&&(previous.WantBe!=Order.First)))
                                    {
                                        var previousPrevious = previous.Previous;
                                        wasMovings = previous.MoveAfter(current) || wasMovings;
                                        previous = previousPrevious;
                                    }
                                    else
                                    {
                                        previous = previous.Previous;
                                    }
                                }
                            }
                        }
                    }
                    current = current.Next;
                }

            }

            return first.First().ToList();
        }


        #endregion


    }
}
