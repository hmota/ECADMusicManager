using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.Data.Repositories
{
    public class SonorizacaoRepository
    {
        public List<Sonorizacao> GetSonorizacoes()
        {
            Context context = SingletonContext.Instance.Context;
            var sonorizacoes = context.Set<Sonorizacao>().ToList();
            return sonorizacoes;
        }

        public List<Sonorizacao> GetSonorizacoesByID(int sonorizacaoID)
        {
            Context context = SingletonContext.Instance.Context;
            var sonorizacoes = (from s in context.Set<Sonorizacao>()
                                where s.SonorizacaoID == sonorizacaoID
                                select s).ToList<Sonorizacao>();
            return sonorizacoes;
        }

        public List<Sonorizacao> GetSonorizacoesByPrograma(int programaID)
        {
            Context context = SingletonContext.Instance.Context;
            var sonorizacoes = (from s in context.Set<Sonorizacao>()
                               where s.Exibicao.ProgramaID == programaID
                                select s).ToList<Sonorizacao>();
            return sonorizacoes;
        }

        public List<Sonorizacao> GetSonorizacoesByPrograma(int programaID, DateTime data)
        {
            Context context = SingletonContext.Instance.Context;
            var sonorizacoes = (from s in context.Set<Sonorizacao>()
                                where s.Exibicao.ProgramaID == programaID
                                && s.Exibicao.Data > data
                                select s).Distinct().ToList<Sonorizacao>();
            return sonorizacoes;
        }
    }
}
