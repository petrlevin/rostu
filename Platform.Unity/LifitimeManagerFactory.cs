using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.Utils;

namespace Platform.Unity
{
    public class LifiTimeManagerFactory<TStorage>
    {
        public LifetimeManager CreateManager<TObject>(Func<TStorage> getStorage,
                                             Expression<Func<TStorage, TObject>> lambda)
        {

            var prop = GetProperty(lambda);
            var setValue = BuildSetValue(prop);
            var getValue = lambda.Compile();

            return new ExternalStorageLifetimeManager(()=>getValue(getStorage()),o=>setValue(getStorage(),o));

        }

        private PropertyInfo GetProperty<TObject>(Expression<Func<TStorage, TObject>> lambda)
        {
            PropertyInfo result;
            try
            {
                result = Reflection.Property(lambda);
            }
            catch (ArgumentException)
            {
                throw Exception(lambda); 
            }
            if (!result.CanWrite)
                throw Exception(lambda);

            return result;


        }

        public Action<TStorage, Object> BuildSetValue(PropertyInfo prop)
        {
            var st = Expression.Parameter(typeof (TStorage));
            var obj = Expression.Parameter(typeof(Object));
            var call = Expression.Call(st, prop.SetMethod, Expression.Convert(obj, prop.PropertyType));
            return Expression.Lambda<Action<TStorage, Object>>(call, st, obj).Compile();
        }



        private Exception Exception<TObject>(Expression<Func<TStorage, TObject>> lambda)
        {
            return new ArgumentException(String.Format("Для регистрации типа нужно использовать ламбда выражение (storage=>storage.SomeProperty). Свойство должно быть доступно и для чтения и для записи. Передано выражение - {0} ", lambda), "lambda");
        }

    }
}
