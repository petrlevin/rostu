using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Utils.Common;
using Platform.Utils.Common.Interfaces;

namespace Platform.Utils.Orderable
{
    public class Item<T>
    {
        public Item<T> Next { get; private set; }
        public Item<T> Previous { get; private set; }
        public T Value { get; private set; }

        public Order WantBe
        {
            get
            {
                var ordered = Value as IOrdered;
                if (ordered==null)
                    return Order.DoesNotMatter;
                ;
                return ordered.WantBe;
            }

        }

        private  List<T>  _before;
        private  List<T> _after;

        public  List<T> Before
        {
            get
            {
                if (_before==null)
                    First().SetBefore();
                return _before;

            }
        }


        public List<T> After
        {
            get
            {
                if (_after == null)
                    First().SetAfter();
                return _after;

            }
        }
        

        public bool MoveAfter(Item<T> item)
        {
            if (Previous == item)
                return false;
            var wasPrevious = Previous;
            var wasNext = Next;
            Previous = item;
            Next = item.Next;
            if (Next!=null)
                Next.Previous = this;
            item.Next = this;

            if (wasPrevious != null) wasPrevious.Next = wasNext;
            if (wasNext != null) wasNext.Previous = wasPrevious;

            return true;
        }

        public bool MoveBefore(Item<T> item)
        {
            if (Next == item)
                return false;
            var wasPrevious = Previous;
            var wasNext = Next;
            Next = item;
            Previous = item.Previous;
            if (Previous!=null)
                Previous.Next = this;
            item.Previous = this;
            if (wasPrevious != null) wasPrevious.Next = wasNext;
            if (wasNext != null) wasNext.Previous = wasPrevious;

            return true;
        }

        public bool MoveBeforeByPredicate()
        {
            var current = Previous;
            while (current != null)
            {
                if (_beforePredicate(Value,current.Value))
                {
                    return MoveBefore(current);

                }
                current = current.Previous;
            }
            return false;

        }


        public bool MoveAfterByPredicate()
        {
            var current = Next;
            while (current != null)
            {
                if (_afterPredicate(Value,current.Value))
                {
                    return MoveAfter(current);
                    
                }
                current = current.Next;
            }
            return false;

        }






        private Item(List<T> source, Predicate beforePredicate, Predicate afterPredicate)
        {
            Value = source[0];
            Previous = null;
            _beforePredicate = beforePredicate;
            _afterPredicate = afterPredicate;
            Next = NextItem(source, this, 1,beforePredicate, afterPredicate);
            
        }

        private void SetBefore()
        {
            var current =this;
            while (current != null)
            {
                current._before = current.GetBefore();
                current = current.Next;
            }
            

        }

        private void SetAfter()
        {
            var current = this;
            while (current != null)
            {
                current._after = current.GetAfter();
                current = current.Next;
            }


        }


        private List<T> GetBefore()
        {
            List<T> result = new List<T>();
            var current = First();
            while (current != null)
            {
                if (current != this)
                {
                    if (_beforePredicate(Value, current.Value))
                    {
                        result.Add(current.Value);
                        result.AddRange(current.GetBefore());
                    }
                    else if (_afterPredicate(current.Value, Value))
                    {

                        result.Add(current.Value);
                        result.AddRange(current.GetBefore());

                    }
                }
                current = current.Next;
            }

            return result.Distinct().ToList();
        }

        private List<T> GetAfter()
        {
            List<T> result = new List<T>();
            var current = First();
            while (current != null)
            {
                if (current != this)
                {

                    if (_afterPredicate(Value, current.Value))
                    {
                        result.Add(current.Value);
                        result.AddRange(current.GetAfter());
                    }
                    else if (_beforePredicate(current.Value, Value))
                    {
                        result.Add(current.Value);
                        result.AddRange(current.GetAfter());
                    }
                }
                current = current.Next;
            }

            return result.Distinct().ToList();
        }



        private Item()
        {

        }

        private Predicate _beforePredicate;
        private Predicate _afterPredicate;

        static private Item<T> NextItem(List<T> source, Item<T> item, int i , Predicate beforePredicate , Predicate afterPredicate)
        {
            if (source.Count <= i)
                return null;
            var result = new Item<T>
                             {
                                 Value = source[i],
                                 Previous = item,
                                 _afterPredicate = afterPredicate,
                                 _beforePredicate = beforePredicate
                             };


            result.Next = NextItem(source, result, i + 1 ,beforePredicate , afterPredicate);
            
            return result;
        }

        public static Item<T> FromList(List<T> list, Predicate beforePredicate , Predicate afterPredicate=null)
        {
            if (list.Count == 0)
                return null;
            if (afterPredicate == null)
                afterPredicate = (f, s) => false;
            if (beforePredicate == null)
                beforePredicate = (f, s) => false;

            return new Item<T>(list, beforePredicate , afterPredicate);
        }

        public List<T> ToList()
        {
            var result = new List<T>();
            result.Add(Value);
            var next = Next;
            while (next != null)
            {
                result.Add(next.Value);
                next = next.Next;
            }
            return result;
        }

        public delegate bool Predicate(T first, T second);


        public Item<T> First()
        {
            var result = this;
            while (result.Previous != null)
                result = result.Previous;
            return result;
        }
    }



}
