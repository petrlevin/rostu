using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Utils.Extensions
{
    public static class DbSetExtension
    {
        public static void RemoveAll<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> data) where TEntity : class
        {
            foreach (var entity in data.ToList())
                set.Remove(entity);
        }

        public static void AddAll<TEntity>(this DbSet<TEntity> set, IEnumerable<TEntity> data) where TEntity : class
        {
            foreach (var entity in data.ToList())
                set.Add(entity);
        }
    }
}
