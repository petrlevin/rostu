using System.Collections.Specialized;

namespace Platform.Web.Common
{
    public static class NameValueCollectionExtensions
    {
        public static int? GetNullableInt(this NameValueCollection values, string paramName)
        {
            return !string.IsNullOrEmpty(values[paramName]) ? int.Parse(values[paramName]) : (int?)null;
        }

        public static int GetInt(this NameValueCollection values, string paramName)
        {
            return int.Parse(values[paramName]);
        }
    }
}