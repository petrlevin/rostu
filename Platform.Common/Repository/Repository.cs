using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Platform.Common.Repository.Interfaces;


namespace Platform.Common.Repository
{
    //public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IBaseEntity
    //{
    //    private IDbContext _context;
    //    public Repository(IDbContext context)
    //    {
    //        _context = context;
    //    }
    //    private IDbSet<TEntity> DbSet
    //    {
    //        get
    //        {
    //            return _context.Set<TEntity>();
    //        }
    //    }
    //    public IQueryable<TEntity> GetAll()
    //    {
    //        return DbSet.AsQueryable();
    //    }
    //    public void Delete(TEntity entity)
    //    {
    //        DbSet.Remove(entity);
    //    }
    //    public void Add(TEntity entity)
    //    {
    //        DbSet.Add(entity);
    //    }

    //    public void Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }
    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (disposing)
    //        {
    //            if (_context != null)
    //            {
    //                _context.Dispose();
    //                _context = null;
    //            }
    //        }
    //    }
    //}
}
