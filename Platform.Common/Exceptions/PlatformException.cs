using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Platform.Common.Interfaces;
using Platform.Utils.Common;

namespace Platform.Common.Exceptions
{
    [JsonObject(MemberSerialization.OptOut)]
    public class PlatformException : Exception
    {

        public class ContractResolver : DefaultContractResolver
        {
            

            protected override List<System.Reflection.MemberInfo> GetSerializableMembers(Type objectType)
            {
                if (objectType.Namespace.StartsWith("System.Data.Entity.Dynamic"))
                {
                    return base.GetSerializableMembers(objectType.BaseType).Where(mi => !IsIgnored(mi)).ToList();
                }

                


                return base.GetSerializableMembers(objectType).Where(mi => !IsIgnored(mi)).ToList();

            }

            private bool IsIgnored(MemberInfo memberInfo)
            {
                var result = memberInfo.GetCustomAttributes(typeof (JsonIgnoreForExceptionAttribute), false).Any();
                return result;
            }



            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                
               var result = base.CreateProperties(type, memberSerialization);
               if (typeof(IUserFriendlyException).IsAssignableFrom(type))
                   result.Add(new JsonProperty() { PropertyName = "IsUserFriendly", ShouldSerialize = (o) => true, PropertyType = typeof(bool) ,Readable = true , ValueProvider = new TrueValueProvider()});
               return result;
            }


            
            
            protected override JsonContract CreateContract(Type objectType)
            {
                return base.CreateContract(objectType);
            }

        }




        public PlatformException()
            : this(string.Empty)
        {
        }

        public PlatformException(string message)
            : this(message, null)
        {

        }

        public PlatformException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public string ExceptionType
        {
            get { return this.GetType().Name; }
        }

        [JsonProperty("message")]
        public override string Message
        {
            get
            {
#if DEBUG
                return (InnerException == null) ? base.Message : String.Format("{0}  {1}",base.Message,InnerException.Message) ;
#else
                return base.Message;
#endif
            }
        }

        
        public override string ToString()
        {
            var formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(this, formatting, getSerializerSettings());
        }

        private JsonSerializerSettings getSerializerSettings()
        {
            JsonSerializerSettings settings;
            settings = new JsonSerializerSettings();

            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            
            
            settings.ContractResolver = new ContractResolver();
            settings.Error += (sender, args) =>
                                  {
                                      args.ErrorContext.Handled = true;
                                  };
            return settings;
        }

        class TrueValueProvider :IValueProvider
        {
            public void SetValue(object target, object value)
            {
                throw new NotImplementedException();
            }

            public object GetValue(object target)
            {
                return true;
            }
        }

        public string Url { get; set; }


    
    }

    
}
