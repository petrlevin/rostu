using System;

namespace Platform.Utils.LazyProperties
{
    public class LazyPropertyAnalyzer<T> : ILazyPropertyAnalyzer where T : class
    {
        private bool isFetched = false;
        private T value;
        
        public LazyPropertyAnalyzer()
        {
        }

        public LazyPropertyAnalyzer(Func<T> getter)
        {
            this.Getter = getter;
        }

        public Func<T> Getter { get; set; }
        public T Value
        {
            get
            {
                if (!isFetched)
                {
                    value = Getter();
                    isFetched = true;
					if (IsRequired && value == null)
						throw new Exception("Обязательное свойство не получило значение");
                }
                return value;                
            }
        }

        public object GetValue()
        {
            return Value;
        }

        public bool HasValue()
        {
            return !Equals(Value, default(T));
        }

        bool ILazyPropertyAnalyzer.IsRequired()
        {
            return IsRequired;
        }

        public bool IsRequired { get; set; }
    }
}
