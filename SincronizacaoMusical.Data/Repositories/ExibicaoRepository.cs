using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.Data.Repositories
{
    public class ExibicaoRepository
    {
        public List<Exibicao> GetExibicoes()
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var exibicoes = context.Set<Exibicao>().ToList();
            context.Dispose();

            return exibicoes;
        }

        public List<Exibicao> GetExibicoesByData(DateTime data)
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var exibicoes = (from e in context.Set<Exibicao>()
                             where e.Data > data
                             select e).ToList();
            context.Dispose();

            return exibicoes;
        }

        public List<Exibicao> GetExibicoesByPrograma(int programaID)
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var exibicoes = (from e in context.Set<Exibicao>()
                             where e.ProgramaID == programaID
                             select e).ToList();
            context.Dispose();

            return exibicoes;
        }

        public List<Exibicao> GetExibicoesByPrograma(int programaID, DateTime data)
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var exibicoes = (from e in context.Set<Exibicao>()
                             where e.ProgramaID == programaID && e.Data > data
                             select e).ToList();
            context.Dispose();

            return exibicoes;
        }
    }
}
