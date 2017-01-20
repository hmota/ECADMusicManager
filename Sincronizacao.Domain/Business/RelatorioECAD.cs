using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatorioECAD
    {
        public List<RowRelatorioECAD> PesquisarSonorizacoes(int unidadeID, int mes, int ano, int tipoRelatorio)
        {
            var uniID = unidadeID;
            using (Context repositorio = new Context())
            {
                repositorio.Configuration.ProxyCreationEnabled = true;
                var sons = repositorio.Sonorizacoes
                                      .Include("Classificacao")
                                      .Include("Sincronizacao")
                                      .Include("Sincronizacao.Exibicao")
                                      .Include("Sincronizacao.Exibicao.Programa")
                                      .Include("Musica")
                                      .Include("Musica.TipoTrilha")
                                      .Include("Musica.Autor")
                                      .Include("Musica.Interprete")
                                      .Include("TipoExibicao")
                                      .AsNoTracking()
                                      .Where(s => s.Sincronizacao.Exibicao.Data.Month == mes)
                                      .Where(s => s.Sincronizacao.Exibicao.Data.Year == ano)
                                      .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                      .Where(s => s.Sincronizacao.Aberto == false)
                                      .Where(s => s.Sincronizacao.Aprovado);

                if (tipoRelatorio == 2)
                {
                    sons = sons
                        .Where(
                            s =>
                            s.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                            s.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"))
                        .Where(s => s.Musica.TipoTrilha.Descricao.ToUpper().Contains("COMERCIAL"));
                }

                var resultSons = sons.OrderBy(s => s.Sincronizacao.Exibicao.Data)
                                     .ThenBy(s => s.Sincronizacao.Exibicao.Programa.Nome)
                                     .Select(s =>
                                             new RowRelatorioECAD()
                                                 {
                                                     Autor = s.Musica.Autor.Nome,
                                                     Classificacao = s.Classificacao.Descricao,
                                                     Data = s.Sincronizacao.Exibicao.Data,
                                                     ISRC = s.Musica.ISRC,
                                                     Interprete = s.Musica.Interprete.Nome,
                                                     Minutagem = s.Minutagem,
                                                     Programa = s.Sincronizacao.Exibicao.Programa.Nome,
                                                     TipoExibicao = s.TipoExibicao.Descricao,
                                                     TipoTrilha = s.Musica.TipoTrilha.Descricao,
                                                     Titulo = s.Musica.Titulo
                                                 });

                var groupSons = from s in resultSons
                                group s by new {s.Programa, s.Data, s.Titulo, s.Classificacao}
                                into sg
                                select sg;

                var groupList = groupSons.ToList();

                foreach (var groupSon in groupList)
                {
                    double soma = 0;
                    bool comercial = false;
                    foreach (var rowRelatorioECAD in groupSon)
                    {
                        rowRelatorioECAD.Segundos = rowRelatorioECAD.Minutagem.TotalSeconds;
                        soma += rowRelatorioECAD.Segundos;
                        comercial = rowRelatorioECAD.TipoTrilha.Contains("COMERCIAL");
                    }
                    //300 segundos = 5 minutos de limite de musicas comerciais
                    if (soma > 300 && comercial)
                        soma = 300;
                    var firstOrDefault = groupSon.FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        firstOrDefault.Minutagem = TimeSpan.FromSeconds(soma);
                        firstOrDefault.Segundos = soma;
                    }
                }

                var rowsECAD = from s in groupList
                               select new RowRelatorioECAD
                                          {
                                              Autor = s.FirstOrDefault().Autor,
                                              Classificacao = s.FirstOrDefault().Classificacao,
                                              Data = s.FirstOrDefault().Data,
                                              ISRC = s.FirstOrDefault().ISRC,
                                              Interprete = s.FirstOrDefault().Interprete,
                                              Minutagem = s.FirstOrDefault().Minutagem,
                                              Programa = s.FirstOrDefault().Programa,
                                              TipoExibicao = s.FirstOrDefault().TipoExibicao,
                                              TipoTrilha = s.FirstOrDefault().TipoTrilha,
                                              Titulo = s.FirstOrDefault().Titulo
                                          };

                return rowsECAD.ToList();
            }
        }
    }
}