//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Practices.Unity;

//namespace Platform.Unity
//{
//    public class PerRequestLifetimeManager : LifetimeManager
//    {
//        private readonly object key = new object();

//        public override object GetValue()
//        {
//            if (HttpContext.Current != null &&
//                HttpContext.Current.Items.Contains(key))
//                return HttpContext.Current.Items[key];
//            else
//                return null;
//        }

//        public override void RemoveValue()
//        {
//            if (HttpContext.Current != null)
//                HttpContext.Current.Items.Remove(key);
//        }

//        public override void SetValue(object newValue)
//        {
//            if (HttpContext.Current != null)
//                HttpContext.Current.Items[key] = newValue;
//        }
//    }
//}
