using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.Data.Repositories
{
    public class UnidadeRepository
    {
        public List<Unidade> GetUnidades()
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var unidades = context.Set<Unidade>().ToList();
            context.Dispose();

            return unidades;
        }

        public List<string> GetNomesUnidades()
        {
            Context context = new Context("connSincronizacaoLocal", false);
            var unidades = (from u in context.Set<Unidade>()
                           select u.Nome).ToList<string>();
            context.Dispose();

            return unidades;
        }
    }
}
