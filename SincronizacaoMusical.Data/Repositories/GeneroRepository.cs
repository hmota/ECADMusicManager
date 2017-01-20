using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.Data.Repositories
{
    public class GeneroRepository
    {
        public List<Genero> GetGeneros()
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var generos = context.Set<Genero>().ToList();
            context.Dispose();

            return generos;
        }
    }
}
