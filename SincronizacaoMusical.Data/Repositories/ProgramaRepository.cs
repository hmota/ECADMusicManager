using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.Data.Repositories
{
    public class ProgramaRepository
    {
        public List<Programa> GetProgramas()
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var programas = context.Set<Programa>().ToList();
            context.Dispose();

            return programas;
        }

        public List<string> GetNomesProgramas()
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var programas = (from u in context.Set<Programa>()
                           select u.Nome).ToList<string>();
            context.Dispose();

            return programas;
        }

        public List<Programa> GetProgramasByGenero(int GeneroID)
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var programas = (from u in context.Set<Programa>()
                             where u.GeneroID == GeneroID
                             select u).ToList();
            context.Dispose();

            return programas;
        }
    }
}
