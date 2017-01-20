using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain.ViewModels;
using System.Data.Objects;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatorioRanking
    {
        public List<RowRelatorioRanking> PesquisarSonorizacoes(int unidadeID, DateTime dataIncial, DateTime dataFinal, int tipoTrilha, int generoID, List<Programa> itemsSelecionados)
        {
            var uniID = unidadeID;
            var items = itemsSelecionados;
            List<Programa> programas = new List<Programa>();
            foreach (var item in items)
            {
                programas.Add(item);
            }

            if (programas.Count <= 0)
            {
                return null;
            }
            TimeSpan diff = dataFinal.Subtract(dataIncial);
            using (Context repositorio = new Context())
            {
                repositorio.Configuration.ProxyCreationEnabled = true;
                var sons = new List<Sonorizacao>();
                var query = repositorio.Sonorizacoes
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
                                      .Where(
                                           ex =>
                                           EntityFunctions.DiffDays(dataIncial, ex.Sincronizacao.Exibicao.Data).Value <= diff.TotalDays &&
                                           EntityFunctions.DiffDays(dataIncial, ex.Sincronizacao.Exibicao.Data).Value >= 0)
                                          .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                          .Where(s => s.Sincronizacao.Aberto == false)
                                          .Where(s => s.Sincronizacao.Aprovado);

                // Tipo de relatório Musical
                //if (cbRankingTipoRel == 1)
                //{
                //    sons = sons
                //        .Where(
                //            s =>
                //            s.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                //            s.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"))
                //            .Where(s => s.Musica.TipoTrilha.Descricao.ToUpper().Contains("COMERCIAL"));
                //}

                if (tipoTrilha != 0)
                {
                    query = query
                            .Where(s => s.Musica.TipoTrilhaID==tipoTrilha);
                }

                if (generoID != 0)
                {
                    query = query
                            .Where(s => s.Sincronizacao.Exibicao.Programa.GeneroID == generoID);
                }

                if (programas.Any(p => p.Nome.Contains("--Todos--")))
                {
                    sons.AddRange(
                        query
                            .Where(ex => ex.Sincronizacao.Exibicao.ProgramaID > 0)
                            .AsQueryable()
                        );
                }
                else
                {
                    foreach (var programa in programas)
                    {
                        sons.AddRange(
                            query
                                .Where(ex => ex.Sincronizacao.Exibicao.ProgramaID == programa.ProgramaID)
                                .AsQueryable()
                            );
                    }
                }

                //Tipo de relatório -- Todos

                var resultSons = sons.OrderBy(s => s.Sincronizacao.Exibicao.Data)
                                     .ThenBy(s => s.Sincronizacao.Exibicao.Programa.Nome)
                                     .Select(s =>
                                             new RowRelatorioRanking()
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
                                group s by new { s.Programa, s.Titulo }
                                    into sg
                                    select sg;

                var groupList = groupSons.ToList().OrderBy(s => s.FirstOrDefault().Programa)
                                     .ThenBy(s => s.FirstOrDefault().Data);

                foreach (var groupson in groupList)
                {
                    double soma = 0;
                    int execucao = 0;
                    //bool comercial = false;
                    foreach (var rowrelatorioranking in groupson)
                    {
                        rowrelatorioranking.Segundos = rowrelatorioranking.Minutagem.TotalSeconds;
                        soma += rowrelatorioranking.Segundos;
                        execucao++;
                        //comercial = rowrelatorioecad.TipoTrilha.Contains("comercial");
                    }

                    ////300 segundos = 5 minutos de limite de musicas comerciais
                    //if (soma > 300 && comercial)
                    //    soma = 300;

                    var firstordefault = groupson.FirstOrDefault();
                    if (firstordefault != null)
                    {
                        firstordefault.Minutagem = TimeSpan.FromSeconds(soma);
                        firstordefault.Segundos = soma;
                        firstordefault.Execucoes = execucao;
                    }
                }

                var rowsRanking = from s in groupList
                               select new RowRelatorioRanking
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
                                              Titulo = s.FirstOrDefault().Titulo,
                                              Execucoes = s.FirstOrDefault().Execucoes
                                          };

                return rowsRanking.ToList();
            }
        }

        public List<RowRelatorioRanking> PesquisarSonorizacoesAnalitico(int unidadeID, DateTime dataIncial, DateTime dataFinal, int tipoTrilha, int generoID, List<Programa> itemsSelecionados)
        {
            var uniID = unidadeID;
            var items = itemsSelecionados;
            List<Programa> programas = new List<Programa>();
            foreach (var item in items)
            {
                programas.Add(item);
            }

            if (programas.Count <= 0)
            {
                return null;
            }
            TimeSpan diff = dataFinal.Subtract(dataIncial);
            using (Context repositorio = new Context())
            {
                repositorio.Configuration.ProxyCreationEnabled = true;
                var sons = new List<Sonorizacao>();
                var query = repositorio.Sonorizacoes
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
                                      .Where(
                                           ex =>
                                           EntityFunctions.DiffDays(dataIncial, ex.Sincronizacao.Exibicao.Data).Value <= diff.TotalDays &&
                                           EntityFunctions.DiffDays(dataIncial, ex.Sincronizacao.Exibicao.Data).Value >= 0)
                                          .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                          .Where(s => s.Sincronizacao.Aberto == false)
                                          .Where(s => s.Sincronizacao.Aprovado);

                if (tipoTrilha != 0)
                {
                    query = query
                            .Where(s => s.Musica.TipoTrilhaID == tipoTrilha);
                }

                if (generoID != 0)
                {
                    query = query
                            .Where(s => s.Sincronizacao.Exibicao.Programa.GeneroID == generoID);
                }

                if (programas.Any(p => p.Nome.Contains("--Todos--")))
                {
                    sons.AddRange(
                        query
                            .Where(ex => ex.Sincronizacao.Exibicao.ProgramaID > 0)
                            .AsQueryable()
                        );
                }
                else
                {
                    foreach (var programa in programas)
                    {
                        sons.AddRange(
                            query
                                .Where(ex => ex.Sincronizacao.Exibicao.ProgramaID == programa.ProgramaID)
                                .AsQueryable()
                            );
                    }
                }

                //Tipo de relatório -- Todos

                var resultSons = sons.OrderBy(s => s.Sincronizacao.Exibicao.Data)
                                     .ThenBy(s => s.Sincronizacao.Exibicao.Programa.Nome)
                                     .Select(s =>
                                             new RowRelatorioRanking()
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
                                group s by new { s.Programa, s.Data, s.Titulo }
                                    into sg
                                    select sg;

                var groupList = groupSons.ToList().OrderBy(s => s.FirstOrDefault().Programa)
                                     .ThenBy(s => s.FirstOrDefault().Data);

                foreach (var groupson in groupList)
                {
                    double soma = 0;
                    int execucao = 0;
                    //bool comercial = false;
                    foreach (var rowrelatorioranking in groupson)
                    {
                        rowrelatorioranking.Segundos = rowrelatorioranking.Minutagem.TotalSeconds;
                        soma += rowrelatorioranking.Segundos;
                        execucao++;
                        //comercial = rowrelatorioecad.TipoTrilha.Contains("comercial");
                    }

                    ////300 segundos = 5 minutos de limite de musicas comerciais
                    //if (soma > 300 && comercial)
                    //    soma = 300;

                    var firstordefault = groupson.FirstOrDefault();
                    if (firstordefault != null)
                    {
                        firstordefault.Minutagem = TimeSpan.FromSeconds(soma);
                        firstordefault.Segundos = soma;
                        firstordefault.Execucoes = execucao;
                    }
                }

                var rowsRanking = from s in groupList
                                  select new RowRelatorioRanking
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
                                      Titulo = s.FirstOrDefault().Titulo,
                                      Execucoes = s.FirstOrDefault().Execucoes
                                  };

                return rowsRanking.ToList();
            }
        }
    }
}

