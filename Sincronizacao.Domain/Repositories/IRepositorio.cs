using System;
using System.Linq;
using System.Linq.Expressions;

namespace SincronizacaoMusical.Domain.Repositories
{
    public interface IRepositorio
    {
        void Salvar();

        void Adicionar<T>(T entidade) where T : class;

        void Editar<T>(T entidade) where T : class;

        void Remover<T>(T entidade) where T : class;

        IQueryable<T> Obter<T>() where T : class;

        IQueryable<T> Obter<T>(Expression<Func<T, bool>> predicate) where T : class;
    }
}
