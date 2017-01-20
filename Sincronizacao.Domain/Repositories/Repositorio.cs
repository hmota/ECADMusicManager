using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace SincronizacaoMusical.Domain.Repositories
{
    public class Repositorio : IRepositorio, IDisposable
    {
        private Context context;

        public Repositorio()
        {
            context = new Context();
            context.Configuration.LazyLoadingEnabled = true;
        }

        /// <summary>
        /// Cria repositorio generico
        /// </summary>
        /// <param name="lazyLoad">Para habilitar ou desabilitar o lazyload</param>
        public Repositorio(bool lazyLoad)
        {
            context = new Context();
            context.Configuration.LazyLoadingEnabled = lazyLoad;
        }

        public Repositorio(Context ctx)
        {
            context = ctx;
        }

        public void Adicionar<T>(T entidade) where T : class
        {
            context.Set<T>().Add(entidade);
            Salvar();
        }

        public void Editar<T>(T entidade) where T : class
        {
            context.Entry<T>(entidade).State = EntityState.Modified;
                Salvar();
        }

        public void Remover<T>(T entidade) where T : class
        {
            context.Set<T>().Remove(entidade);
            Salvar();
        }

        public IQueryable<T> Obter<T>() where T : class
        {
            return context.Set<T>().AsQueryable<T>();
        }

        public IQueryable<T> Obter<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return context.Set<T>().Where(predicate);
        }

        public IQueryable<T> Obter<T>(string IncludeProperty) where T : class
        {
            return context.Set<T>().Include(IncludeProperty);
        }

        public IQueryable<T> Obter<T>(Expression<Func<T, bool>> predicate, string IncludeProperty) where T : class
        {
            return context.Set<T>().Include(IncludeProperty).Where(predicate);
        }

        public void Salvar()
        {
            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the EF
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether or not to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (context != null)
                {
                    context.Dispose();
                    context = null;
                }
            }
        }
    }
}
