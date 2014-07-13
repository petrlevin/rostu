using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Platform.Utils
{
    public static class Reflection<TTarget>
    {
        /// <summary>
        /// Получить информацию о свойстве (PropertyInfo) по лямда выражению (target=>target.SomeProperty)
        /// 
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static  PropertyInfo Property<TProperty>(Expression<Func<TTarget,TProperty>> lambda)
        {
            if ((lambda.NodeType != ExpressionType.Lambda))
                throw Exception(lambda);

            var body = lambda.Body;
            var propExpr = body as MemberExpression;
            if (propExpr == null)
                throw Exception(lambda);
            if (!(propExpr.Expression is ParameterExpression))
                throw Exception(lambda);
            var prop = propExpr.Member as PropertyInfo;
            if (prop == null)
                throw Exception(lambda);
            return prop;

        }

        private static Exception Exception<TProperty>(Expression<Func<TTarget, TProperty>> lambda)
        {
            return new ArgumentException(String.Format("Для получения информации о свойстве (PropertyInfo) нужно использовать лямбда выражение  (target=>target.SomeProperty).  Передано выражение - {0} ", lambda), "lambda");
        }

    }

    public static class Reflection
    {
        /// <summary>
        /// Получить информацию о свойстве (PropertyInfo) по лямда выражению (target=>target.SomeProperty)
        /// 
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="lambda"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static PropertyInfo Property<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> lambda)
        {
            return Reflection<TTarget>.Property(lambda);
        }



    }
}
