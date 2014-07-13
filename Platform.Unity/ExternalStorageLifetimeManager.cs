using Microsoft.Practices.Unity;
using System;

namespace Platform.Unity
{
    public class ExternalStorageLifetimeManager:LifetimeManager
    {


        private readonly Func<Object> _getValueHandler;
        private readonly Action<object> _setValueHandler;

        public override object GetValue()
        {
            return _getValueHandler();
        }

        public override void SetValue(object newValue)
        {
            if (_setValueHandler != null)
                _setValueHandler(newValue);
        }

        public override void RemoveValue()
        {
            
        }

        public ExternalStorageLifetimeManager(Func<Object> getValueHandler, Action<Object> setValueHandler=null)
        {
            if (getValueHandler==null)
                throw new ArgumentNullException("getValueHandler");
            _getValueHandler = getValueHandler;
            _setValueHandler = setValueHandler;
        }

        
    }
    
}
