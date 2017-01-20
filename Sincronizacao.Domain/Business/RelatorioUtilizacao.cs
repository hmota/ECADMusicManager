using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatorioUtilizacao
    {
        public List<RowRelatorioUtilizacao> PesquisarSonorizacoes(int unidadeID, int generoID, DateTime dataInicial,
                                                                  DateTime dataFinal, List<Programa> itemsSelecionados)
        {
            var items = itemsSelecionados;
            //((ListBox)cbRelProvPrograma.Template.FindName("lstBox", cbRelProvPrograma)).SelectedItems;
            List<Programa> programas = new List<Programa>();
            foreach (var item in items)
            {
                programas.Add(item);
            }

            if (programas.Count <= 0)
            {
                return null;
            }
            TimeSpan diff = dataFinal.Subtract(dataInicial);
            using (Context repositorio = new Context())
            {
                var uniID = unidadeID;
                var genID = generoID;

                repositorio.Configuration.ProxyCreationEnabled = true;
                var sons = new List<Sonorizacao>();
                var query = repositorio.Sonorizacoes
                                       .AsNoTracking()
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
                                           EntityFunctions.DiffDays(dataInicial, ex.Sincronizacao.Exibicao.Data).Value <= diff.TotalDays &&
                                           EntityFunctions.DiffDays(dataInicial, ex.Sincronizacao.Exibicao.Data).Value >= 0)
                                       .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                       .Where(s => s.Sincronizacao.Aberto == false)
                                       .Where(s => s.Sincronizacao.Aprovado)
                                       .Where(
                                           s =>
                                           s.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                                           s.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"));

                if (genID != 0)
                {
                    query = query.Where(s => s.Sincronizacao.Exibicao.Programa.GeneroID == genID);
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

                var resultSons = sons
                    .OrderBy(s => s.Sincronizacao.Exibicao.Data)
                    .ThenBy(s => s.Sincronizacao.Exibicao.Programa.Nome)
                    .Select(s => s);

                var groupSons = from s in resultSons
                                group s by new
                                               {
                                                   s.Sincronizacao.Exibicao.Programa.Nome,
                                                   TipoTrilhaID = s.Musica.TipoTrilhaID,
                                                   s.Musica.Titulo,
                                                   s.Classificacao.Descricao,
                                                   groupKey =
                                    s.Sincronizacao.Exibicao.ProgramaID + "_" + s.Musica.TipoTrilhaID
                                               }
                                into sg
                                select sg;

                var groupList = groupSons.ToList();

                Dictionary<string, RowClassificacoesQuant> dicClass = new Dictionary<string, RowClassificacoesQuant>();
                Dictionary<string, RowClassificacoesQuant> dicQntd = new Dictionary<string, RowClassificacoesQuant>();

                //dicionario chave do grupo nomeprograma/ rowclassificacoes
                foreach (var groupSon in groupList)
                {
                    var groupKey = groupSon.Key.groupKey;
                    var classif = groupSon.FirstOrDefault().Classificacao;
                    if (classif != null)
                    {
                        RowClassificacoesQuant rClass;
                        RowClassificacoesQuant rQntd;

                        if (dicClass.ContainsKey(groupKey))
                        {
                            rClass = dicClass[groupKey];
                            rQntd = dicQntd[groupKey];
                        }
                        else
                        {
                            rClass = new RowClassificacoesQuant();
                            rQntd = new RowClassificacoesQuant();
                        }

                        switch (classif.Descricao)
                        {
                            case "PERFORMANCE":
                                rQntd.performance++;
                                break;
                            case "ADORNO":
                                rQntd.adorno++;
                                break;
                            case "FUNDO":
                                rQntd.fundo++;
                                break;
                            case "TEMA DE BLOCO":
                                rQntd.tema++;
                                break;
                            case "ABERTURA":
                                rQntd.abertura++;
                                break;
                            case "FUNDO JORNALISTICO":
                                rQntd.fundoJornalistico++;
                                break;
                        }

                        if (dicClass.ContainsKey(groupKey))
                        {
                            dicClass[groupKey] = rClass;
                            dicQntd[groupKey] = rQntd;
                        }
                        else
                        {
                            dicClass.Add(groupKey, rClass);
                            dicQntd.Add(groupKey, rQntd);
                        }
                    }
                }

                var rowsProvisao = from s in groupList

                                   group s by
                                       new
                                           {
                                               s.FirstOrDefault().Sincronizacao.Exibicao.Programa.ProgramaID,
                                               tipoTrilha = s.FirstOrDefault().Musica.TipoTrilha.Descricao,
                                               groupKey = s.Key.groupKey
                                           }
                                   into sg
                                   orderby sg.Key.ProgramaID
                                       select new RowRelatorioUtilizacao
                                              {
                                                  Programa =
                                                      sg.FirstOrDefault().FirstOrDefault()
                                                        .Sincronizacao.Exibicao.Programa.Nome,
                                                  Ordem =
                                                      sg.FirstOrDefault().FirstOrDefault()
                                                        .Sincronizacao.Exibicao.Programa.Ordem,
                                                  TipoTrilha = sg.Key.tipoTrilha,
                                                  Performance = dicQntd[sg.Key.groupKey].performance,
                                                  Adorno = dicQntd[sg.Key.groupKey].adorno,
                                                  Fundo = dicQntd[sg.Key.groupKey].fundo,
                                                  Tema = dicQntd[sg.Key.groupKey].tema,
                                                  Abertura = dicQntd[sg.Key.groupKey].abertura,
                                                  FundoJornalistico = dicQntd[sg.Key.groupKey].fundoJornalistico,
                                                  Total = dicQntd[sg.Key.groupKey].performance +
                                                              dicQntd[sg.Key.groupKey].adorno +
                                                              dicQntd[sg.Key.groupKey].fundo +
                                                              dicQntd[sg.Key.groupKey].tema +
                                                              dicQntd[sg.Key.groupKey].abertura +
                                                              dicQntd[sg.Key.groupKey].fundoJornalistico
                                              };

                return rowsProvisao.Distinct().ToList();
            }
        }
    }
}
