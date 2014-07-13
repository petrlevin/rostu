using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Extensions
{
    public static class TypeExtension
    {
        public static bool InheritsFrom(this Type t, Type baseType, bool includeInterfaces = true)
        {
            if (!baseType.IsGenericTypeDefinition)
            {
                if ((includeInterfaces) && (t.GetInterfaces().Any(i => baseType.Equals(i))))
                    return true;

                Type cur = t.BaseType;

                while (cur != null)
                {
                    if (cur.Equals(baseType))
                    {
                        return true;
                    }

                    cur = cur.BaseType;
                }
            }
            else
            {
                if ((includeInterfaces) && (t.GetInterfaces().Any(i => i.IsGenericType && baseType.Equals(i.GetGenericTypeDefinition()))))
                    return true;
                Type cur = t.BaseType;

                while (cur != null)
                {
                    if (cur.IsGenericType && (cur.GetGenericTypeDefinition().Equals(baseType)))
                    {
                        return true;
                    }

                    cur = cur.BaseType;
                }


            }

            return false;
        }

        public static bool InheritsFrom<TBaseType>(this Type t, bool includeInterfaces = true)
        {
            return InheritsFrom(t, typeof(TBaseType), includeInterfaces);
        }

        public static IEnumerable<Type> GetAllParents(this Type t, bool includeInterfaces = true)
        {
            List<Type> result = new List<Type>();
            Type cur = t.BaseType;

            while (cur != null)
            {
                result.Add(cur);

                cur = cur.BaseType;
            }

            if (includeInterfaces)
                result.AddRange(t.GetInterfaces());

            return result;

        }

        public static IEnumerable<Type> GetAllParents(this Type type, Type genericTypeDefinition, bool includeInterfaces = true)
        {
            var result = type.GetAllParents(includeInterfaces);
            return result.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition);
        }

        public static IEnumerable<Type> GetAllParents(this Type type, Type genericTypeDefinition,params Type[] genericTypeArguments)
        {
            var result = type.GetAllParents(genericTypeDefinition,true);
            return result.Where(t =>
                                    {
                                        for (int i = 0; i < genericTypeArguments.Count(); i++)
                                        {
                                            if (t.GetGenericArguments()[i] != genericTypeArguments[i])
                                                return false;

                                        }
                                        return true;
                                    });
        }


    }
}
