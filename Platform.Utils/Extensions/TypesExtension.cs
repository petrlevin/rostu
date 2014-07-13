using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Utils.Extensions;

namespace Platform.Utils
{
    public static class TypesExtension
    {
        public static IEnumerable<Type> WhitchInherit(this IEnumerable<Type> types, Type baseType, TypeOptions options = TypeOptions.All)
        {
            var result = types.Where(t => t.InheritsFrom(baseType));
            return FilterByOptions(options, result);
        }

        private static IEnumerable<Type> FilterByOptions(TypeOptions options, IEnumerable<Type> result)
        {
            if ((options & TypeOptions.Public) == TypeOptions.Public)
                result = result.Where(t => t.IsPublic);
            if ((options & TypeOptions.NotAbstract) == TypeOptions.NotAbstract)
                result = result.Where(t => !t.IsAbstract);
            if ((options & TypeOptions.WithPublicParameterLessConstructor) == TypeOptions.WithPublicParameterLessConstructor)
                result = result.Where(t => t.GetConstructor(new Type[] { }) != null);
            if ((options & TypeOptions.IsClass) == TypeOptions.IsClass)
                result = result.Where(t => !t.IsClass);
            return result;
        }

        public static IEnumerable<Type> WhitchHasAttribute<TAttribute>(this IEnumerable<Type> types,
                                                                       TypeOptions options = TypeOptions.All,
                                                                       bool includeInherits = true)
        {
            return WhitchHasAttribute(types, typeof (TAttribute), options, includeInherits);
        }

        public static IEnumerable<Type> WhitchHasAttribute(this IEnumerable<Type> types, Type attributeType, TypeOptions options = TypeOptions.All, bool includeInherits = true)
        {
            var result = types.Where(t => t.GetCustomAttributes(includeInherits).Any(a=>a.GetType().Equals(attributeType)));
            return FilterByOptions(options, result);

        }

    }
}
