using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Sincronizacao.Domain.Repositories.Interfaces;

namespace Sincronizacao.Domain.Repositories
{
    public class BaseRepository<T> 
       : IDisposable, IBaseRepository<T> where T : class
   {
       private Context _context;
   
       #region Ctor
       //public BaseRepository(IUnitOfWork unitOfWork)
       //{
       //    if (unitOfWork == null)
       //        throw new ArgumentNullException("unitOfWork");
   
       //    _context = unitOfWork as SampleDataContext;
       //}
       //#endregion

       public BaseRepository()
       {
           if (_context == null)
               _context = new Context();
       }
       #endregion

       public T Find(int id)
       {
           return _context.Set<T>().Find(id);
       }
   
       public IQueryable<T> List()
       {
           return _context.Set<T>();
       }
   
       public void Add(T item)
       {
           _context.Set<T>().Add(item);
           Save();
       }
   
       public void Remove(T item)
       {
           _context.Set<T>().Remove(item);
           Save();
       }
   
       public void Edit(T item)
       {
           _context.Set<T>().Attach(item);
           _context.Entry(item).State = EntityState.Modified;
           Save();
       }

       public void Save()
       {
           _context.SaveChanges();
       }
   
       public void Dispose()
       {
           _context.Dispose();
       }
   }
}
