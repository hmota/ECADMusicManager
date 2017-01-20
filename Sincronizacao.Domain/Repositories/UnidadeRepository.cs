using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Data.Repositories
{
    public class UnidadeRepository
    {
        public List<Unidade> GetUnidades()
        {
            Context context = new Context();
            var unidades = context.Set<Unidade>().ToList();
            context.Dispose();

            return unidades;
        }

        public List<string> GetNomesUnidades()
        {
            Context context = new Context();
            var unidades = (from u in context.Set<Unidade>()
                           select u.Nome).ToList<string>();
            context.Dispose();

            return unidades;
        }
    }
}
