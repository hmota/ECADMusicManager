using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Data.Repositories
{
    public class ProgramaRepository
    {
        public List<Programa> GetProgramas()
        {
            Context context = new Context();
            var programas = context.Set<Programa>().ToList();
            context.Dispose();

            return programas;
        }

        public List<string> GetNomesProgramas()
        {
            Context context = new Context();
            var programas = (from u in context.Set<Programa>()
                           select u.Nome).ToList<string>();
            context.Dispose();

            return programas;
        }

        public List<Programa> GetProgramasByGenero(int GeneroID)
        {
            using (Context context = new Context())
            {
                var programas = (from u in context.Set<Programa>()
                                 where u.GeneroID == GeneroID
                                 select u).ToList();
                return programas;
            }
        }
    }
}
