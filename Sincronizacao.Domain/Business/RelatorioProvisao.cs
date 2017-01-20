using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatorioProvisao
    {
        public List<RowRelatorioProvisao> PesquisarSonorizacoes(int unidadeID, int generoID, int mesRelatorio,
                                                                int anoRelatorio, List<Programa> itemsSelecionados)
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

            using (Context repositorio = new Context())
            {
                var uniID = unidadeID;
                var genID = generoID;
                var mes = mesRelatorio;
                var ano = anoRelatorio;
                //var classID=
                var precos = (from p in repositorio.Precos
                              where
                                  genID == 0 ? p.Vigencia == ano : p.Vigencia == ano && p.GeneroID == genID
                              select p).Distinct().ToList();

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
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Month == mes)
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Year == ano)
                                       .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                       .Where(s => s.Sincronizacao.Aberto == false)
                                       .Where(s => s.Sincronizacao.Aprovado)
                                       .Where(
                                           s =>
                                           s.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                                           s.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"))
                                       .Where(s => s.Musica.TipoTrilha.Descricao.ToUpper().Contains("COMERCIAL"));
                //IQueryable<>
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
                                                   s.Sincronizacao.Exibicao.Data,
                                                   s.Musica.Titulo,
                                                   s.Classificacao.Descricao
                                               }
                                into sg
                                select sg;

                var groupList = groupSons.ToList();

                Dictionary<int, RowClassificacoes> dicClass = new Dictionary<int, RowClassificacoes>();
                Dictionary<int, RowClassificacoes> dicQntd = new Dictionary<int, RowClassificacoes>();

                //dicionario chave do grupo nomeprograma/ rowclassificacoes
                foreach (var groupSon in groupList)
                {
                    int programaID = groupSon.FirstOrDefault().Sincronizacao.Exibicao.ProgramaID;
                    var classif = groupSon.FirstOrDefault().Classificacao;
                    if (classif != null)
                    {
                        var genIDGroup = groupSon.FirstOrDefault().Sincronizacao.Exibicao.Programa.GeneroID;
                        var precoTabela = precos
                            .Where(p => p.Abrangencia == "BRASIL")
                            .Where(p => p.Vigencia == ano)
                            .Where(p => p.GeneroID == genIDGroup)
                            .FirstOrDefault(p => p.ClassificacaoID == classif.ClassificacaoID);
                        RowClassificacoes rClass;
                        RowClassificacoes rQntd;

                        decimal preco;
                        var eReprise = groupSon.FirstOrDefault().TipoExibicao.Descricao.ToUpper().Contains("REPRISE");
                        if (!eReprise)
                        {
                            preco = precoTabela.Valor;
                        }
                        else
                        {
                            preco = precoTabela.Valor/2;
                        }

                        if (dicClass.ContainsKey(programaID))
                        {
                            rClass = dicClass[programaID];
                            rQntd = dicQntd[programaID];
                        }
                        else
                        {
                            rClass = new RowClassificacoes();
                            rQntd = new RowClassificacoes();
                        }

                        switch (classif.Descricao)
                        {
                            case "PERFORMANCE":
                                rClass.performance += preco;
                                rQntd.performance++;
                                break;
                            case "ADORNO":
                                rClass.adorno += preco;
                                rQntd.adorno++;
                                break;
                            case "FUNDO":
                                rClass.fundo += preco;
                                rQntd.fundo++;
                                break;
                            case "TEMA DE BLOCO":
                                rClass.tema += preco;
                                rQntd.tema++;
                                break;
                            case "ABERTURA":
                                rClass.abertura += preco;
                                rQntd.abertura++;
                                break;
                            case "FUNDO JORNALISTICO":
                                rClass.fundoJornalistico += preco;
                                rQntd.fundoJornalistico++;
                                break;
                        }

                        if (dicClass.ContainsKey(programaID))
                        {
                            dicClass[programaID] = rClass;
                            dicQntd[programaID] = rQntd;
                        }
                        else
                        {
                            dicClass.Add(programaID, rClass);
                            dicQntd.Add(programaID, rQntd);
                        }
                    }
                }

                var rowsProvisao = from s in groupList
                                   group s by
                                       new
                                           {
                                               s.FirstOrDefault().Sincronizacao.Exibicao.Programa.ProgramaID
                                           }
                                   into sg
                                   select new RowRelatorioProvisao
                                              {
                                                  Programa =
                                                      sg.FirstOrDefault().FirstOrDefault()
                                                        .Sincronizacao.Exibicao.Programa.Nome,
                                                  Data =
                                                      sg.FirstOrDefault().FirstOrDefault().Sincronizacao.Exibicao.Data,
                                                  Ordem =
                                                      sg.FirstOrDefault().FirstOrDefault()
                                                        .Sincronizacao.Exibicao.Programa.Ordem,
                                                  Performance = dicClass[sg.Key.ProgramaID].performance,
                                                  Adorno = dicClass[sg.Key.ProgramaID].adorno,
                                                  Fundo = dicClass[sg.Key.ProgramaID].fundo,
                                                  Tema = dicClass[sg.Key.ProgramaID].tema,
                                                  Abertura = dicClass[sg.Key.ProgramaID].abertura,
                                                  FundoJornalistico = dicClass[sg.Key.ProgramaID].fundoJornalistico,
                                                  Total = dicClass[sg.Key.ProgramaID].performance +
                                                          dicClass[sg.Key.ProgramaID].adorno +
                                                          dicClass[sg.Key.ProgramaID].fundo +
                                                          dicClass[sg.Key.ProgramaID].tema +
                                                          dicClass[sg.Key.ProgramaID].abertura +
                                                          dicClass[sg.Key.ProgramaID].fundoJornalistico,
                                                  PerformanceQntd = dicQntd[sg.Key.ProgramaID].performance,
                                                  AdornoQntd = dicQntd[sg.Key.ProgramaID].adorno,
                                                  FundoQntd = dicQntd[sg.Key.ProgramaID].fundo,
                                                  TemaQntd = dicQntd[sg.Key.ProgramaID].tema,
                                                  AberturaQntd = dicQntd[sg.Key.ProgramaID].abertura,
                                                  FundoJornalisticoQntd = dicQntd[sg.Key.ProgramaID].fundoJornalistico,
                                                  TotalQntd = dicQntd[sg.Key.ProgramaID].performance +
                                                              dicQntd[sg.Key.ProgramaID].adorno +
                                                              dicQntd[sg.Key.ProgramaID].fundo +
                                                              dicQntd[sg.Key.ProgramaID].tema +
                                                              dicQntd[sg.Key.ProgramaID].abertura +
                                                              dicQntd[sg.Key.ProgramaID].fundoJornalistico
                                              };
                
                return rowsProvisao.Distinct().ToList();
            }
        }


        public List<RowRelatorioProvisao> PesquisarSonorizacoesAnalitico(int unidadeID, int generoID, int mesRelatorio,
                                                                int anoRelatorio, List<Programa> itemsSelecionados)
        {
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

            using (Context repositorio = new Context())
            {
                var uniID = unidadeID;
                var genID = generoID;
                var mes = mesRelatorio;
                var ano = anoRelatorio;
                //var classID=
                var precos = (from p in repositorio.Precos
                              where
                                  genID == 0 ? p.Vigencia == ano : p.Vigencia == ano && p.GeneroID == genID
                              select p).Distinct().ToList();

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
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Month == mes)
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Year == ano)
                                       .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                       .Where(s => s.Sincronizacao.Aberto == false)
                                       .Where(s => s.Sincronizacao.Aprovado)
                                       .Where(
                                           s =>
                                           s.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                                           s.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"))
                                       .Where(s => s.Musica.TipoTrilha.Descricao.ToUpper().Contains("COMERCIAL"));
                //IQueryable<>
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
                                    s.Sincronizacao.Exibicao.Data,
                                    s.Musica.Titulo,
                                    s.Classificacao.Descricao,
                                    groupKey =
                                    s.Sincronizacao.Exibicao.ProgramaID + "_" + s.Sincronizacao.Exibicao.Data
                                }
                                    into sg
                                    select sg;

                var groupList = groupSons.ToList();

                Dictionary<string, RowClassificacoes> dicClass = new Dictionary<string, RowClassificacoes>();
                Dictionary<string, RowClassificacoes> dicQntd = new Dictionary<string, RowClassificacoes>();

                //dicionario chave do grupo nomeprograma/ rowclassificacoes
                foreach (var groupSon in groupList)
                {
                    var groupKey = groupSon.Key.groupKey;
                    int programaID = groupSon.FirstOrDefault().Sincronizacao.Exibicao.ProgramaID;
                    var classif = groupSon.FirstOrDefault().Classificacao;
                    if (classif != null)
                    {
                        var genIDGroup = groupSon.FirstOrDefault().Sincronizacao.Exibicao.Programa.GeneroID;
                        var precoTabela = precos
                            .Where(p => p.Abrangencia == "BRASIL")
                            .Where(p => p.Vigencia == ano)
                            .Where(p => p.GeneroID == genIDGroup)
                            .FirstOrDefault(p => p.ClassificacaoID == classif.ClassificacaoID);
                        RowClassificacoes rClass;
                        RowClassificacoes rQntd;

                        decimal preco;
                        var eReprise = groupSon.FirstOrDefault().TipoExibicao.Descricao.ToUpper().Contains("REPRISE");
                        if (!eReprise)
                        {
                            preco = precoTabela.Valor;
                        }
                        else
                        {
                            preco = precoTabela.Valor / 2;
                        }

                        if (dicClass.ContainsKey(groupKey))
                        {
                            rClass = dicClass[groupKey];
                            rQntd = dicQntd[groupKey];
                        }
                        else
                        {
                            rClass = new RowClassificacoes();
                            rQntd = new RowClassificacoes();
                        }

                        switch (classif.Descricao)
                        {
                            case "PERFORMANCE":
                                rClass.performance += preco;
                                rQntd.performance++;
                                break;
                            case "ADORNO":
                                rClass.adorno += preco;
                                rQntd.adorno++;
                                break;
                            case "FUNDO":
                                rClass.fundo += preco;
                                rQntd.fundo++;
                                break;
                            case "TEMA DE BLOCO":
                                rClass.tema += preco;
                                rQntd.tema++;
                                break;
                            case "ABERTURA":
                                rClass.abertura += preco;
                                rQntd.abertura++;
                                break;
                            case "FUNDO JORNALISTICO":
                                rClass.fundoJornalistico += preco;
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
                                           data = s.FirstOrDefault().Sincronizacao.Exibicao.Data,
                                           groupKey = s.Key.groupKey
                                       }
                                       into sg
                                       orderby sg.Key.ProgramaID
                                       select new RowRelatorioProvisao
                                       {
                                           Programa =
                                               sg.FirstOrDefault().FirstOrDefault()
                                                 .Sincronizacao.Exibicao.Programa.Nome,
                                           Data =
                                               sg.FirstOrDefault().FirstOrDefault().Sincronizacao.Exibicao.Data,
                                           Ordem =
                                               sg.FirstOrDefault().FirstOrDefault()
                                                 .Sincronizacao.Exibicao.Programa.Ordem,
                                           Performance = dicClass[sg.Key.groupKey].performance,
                                           Adorno = dicClass[sg.Key.groupKey].adorno,
                                           Fundo = dicClass[sg.Key.groupKey].fundo,
                                           Tema = dicClass[sg.Key.groupKey].tema,
                                           Abertura = dicClass[sg.Key.groupKey].abertura,
                                           FundoJornalistico = dicClass[sg.Key.groupKey].fundoJornalistico,
                                           Total = dicClass[sg.Key.groupKey].performance +
                                                   dicClass[sg.Key.groupKey].adorno +
                                                   dicClass[sg.Key.groupKey].fundo +
                                                   dicClass[sg.Key.groupKey].tema +
                                                   dicClass[sg.Key.groupKey].abertura +
                                                   dicClass[sg.Key.groupKey].fundoJornalistico,
                                           PerformanceQntd = dicQntd[sg.Key.groupKey].performance,
                                           AdornoQntd = dicQntd[sg.Key.groupKey].adorno,
                                           FundoQntd = dicQntd[sg.Key.groupKey].fundo,
                                           TemaQntd = dicQntd[sg.Key.groupKey].tema,
                                           AberturaQntd = dicQntd[sg.Key.groupKey].abertura,
                                           FundoJornalisticoQntd = dicQntd[sg.Key.groupKey].fundoJornalistico,
                                           TotalQntd = dicQntd[sg.Key.groupKey].performance +
                                                       dicQntd[sg.Key.groupKey].adorno +
                                                       dicQntd[sg.Key.groupKey].fundo +
                                                       dicQntd[sg.Key.groupKey].tema +
                                                       dicQntd[sg.Key.groupKey].abertura +
                                                       dicQntd[sg.Key.groupKey].fundoJornalistico
                                       };

                return rowsProvisao.Distinct().ToList();
            }
        }

        [Obsolete]
        public List<RowRelatorioProvisao> PesquisarSonorizacoesAnaliticoObsolete(int unidadeID, int generoID, int mesRelatorio,
                                                                int anoRelatorio, List<Programa> itemsSelecionados)
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

            using (Context repositorio = new Context())
            {
                var uniID = unidadeID;
                var genID = generoID;
                var mes = mesRelatorio;
                var ano = anoRelatorio;

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
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Month == mes)
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Year == ano)
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
                                    groupKey = s.Sincronizacao.Exibicao.ProgramaID + "_" + s.Musica.TipoTrilhaID
                                }
                                    into sg
                                    select sg;

                var groupList = groupSons.ToList();

                Dictionary<string, RowClassificacoes> dicClass = new Dictionary<string, RowClassificacoes>();
                Dictionary<string, RowClassificacoes> dicQntd = new Dictionary<string, RowClassificacoes>();

                //dicionario chave do grupo nomeprograma/ rowclassificacoes
                foreach (var groupSon in groupList)
                {
                    var groupKey = groupSon.Key.groupKey;
                    var classif = groupSon.FirstOrDefault().Classificacao;
                    if (classif != null)
                    {
                        RowClassificacoes rClass;
                        RowClassificacoes rQntd;

                        if (dicClass.ContainsKey(groupKey))
                        {
                            rClass = dicClass[groupKey];
                            rQntd = dicQntd[groupKey];
                        }
                        else
                        {
                            rClass = new RowClassificacoes();
                            rQntd = new RowClassificacoes();
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
                                       select new RowRelatorioProvisao
                                       {
                                           Programa =
                                               sg.FirstOrDefault().FirstOrDefault()
                                                 .Sincronizacao.Exibicao.Programa.Nome,
                                           Ordem =
                                               sg.FirstOrDefault().FirstOrDefault()
                                                 .Sincronizacao.Exibicao.Programa.Ordem,
                                           PerformanceQntd = dicQntd[sg.Key.groupKey].performance,
                                           AdornoQntd = dicQntd[sg.Key.groupKey].adorno,
                                           FundoQntd = dicQntd[sg.Key.groupKey].fundo,
                                           TemaQntd = dicQntd[sg.Key.groupKey].tema,
                                           AberturaQntd = dicQntd[sg.Key.groupKey].abertura,
                                           FundoJornalisticoQntd = dicQntd[sg.Key.groupKey].fundoJornalistico,
                                           TotalQntd = dicQntd[sg.Key.groupKey].performance +
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
