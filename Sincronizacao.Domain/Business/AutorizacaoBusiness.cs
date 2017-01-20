using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain.ViewModels;

namespace SincronizacaoMusical.Domain.Business
{
    public class AutorizacaoBusiness
    {
        public List<RowExibicaoAutorizacao> Obter(Context context, int exibicaoID)
        {
            int sincAtual = (from s in context.Sincronizacoes
                             where s.ExibicaoID == exibicaoID
                             select s.SincronizacaoID).SingleOrDefault();

            var musicas = from musica in context.Musicas
                          where musica.TipoTrilha.Descricao.Contains("Comercial")
                          select musica;

            var sons = from son in context.Sonorizacoes
                       where
                           son.SincronizacaoID == sincAtual &&
                           (son.TipoExibicao.Descricao.Contains("VT") ||
                            son.TipoExibicao.Descricao.Contains("REPRISE"))
                       select son;


            var musson = from s in sons
                         join m in musicas on s.MusicaID equals m.MusicaID
                         group s by new { s.Musica.MusicaID, s.ClassificacaoID }
                             into sg
                             select new { sg };

            var autmusson = from ms in musson
                            join aut in context.Autorizacoes on ms.sg.FirstOrDefault().SonorizacaoID equals
                                aut.SonorizacaoID
                                into joinEmptAut
                            from a in joinEmptAut.DefaultIfEmpty()
                            select new
                            {
                                a,
                                ms,
                                sum = a != null
                                          ? joinEmptAut.Sum(x => x.Porcentagem)
                                          : 0
                            };


            var rowsAut = (from ams in autmusson
                           select new RowExibicaoAutorizacao
                           {
                               AutID = ams.a != null ? ams.a.AutorizacaoID : 0,
                               SonID = ams.ms.sg.FirstOrDefault().SonorizacaoID,
                               SincID = ams.ms.sg.FirstOrDefault().SincronizacaoID,
                               MusicaID = ams.ms.sg.FirstOrDefault().MusicaID,
                               Musica = ams.ms.sg.FirstOrDefault().Musica.Titulo,
                               Autor = ams.ms.sg.FirstOrDefault().Musica.Autor,
                               Interprete = ams.ms.sg.FirstOrDefault().Musica.Interprete,
                               Classificacao = ams.ms.sg.FirstOrDefault().Classificacao.Descricao,
                               Minutagem = ams.ms.sg.FirstOrDefault().Minutagem,
                               Porcentagem = ams.sum,
                               Incidental = false,
                               PoutPourri = false
                           })
                .ToList();

            var result = rowsAut
                .GroupBy(p => new { p.Musica, p.Classificacao });

            IList<RowExibicaoAutorizacao> result2 = new List<RowExibicaoAutorizacao>();
            foreach (var group in result)
            {
                result2.Add(group.FirstOrDefault());
            }
            return result2.ToList();
        }
    }
}
