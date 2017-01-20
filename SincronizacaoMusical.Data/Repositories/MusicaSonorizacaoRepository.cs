using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.Data.Repositories
{
    public class MusicaSonorizacaoRepository
    {
        public List<Sonorizacao> GetSonorizacoes()
        {
            Context context = SingletonContext.Instance.Context;
            var sonorizacoes = context.Set<Sonorizacao>().ToList();
            return sonorizacoes;
        }

        //public List<Sonorizacao> GetMusicasBySonorizacao(int sonorizacaoID)
        //{
        //    Context context = SingletonContext.Instance.Context;
        //    var musicas = (from m in context.Set<Musica>()
        //                   join
        //                       where s.Exibicao.ProgramaID == programaID
        //                        select s).ToList<Sonorizacao>();
        //    return sonorizacoes;
        //}
    }
}
