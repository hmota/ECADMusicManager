using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatorioPgtoAberto
    {
        public List<RowRelatorioPgtoAberto> PesquisarSonorizacoes(int unidadeID, int generoID, int mes, int ano,
                                                                  List<Programa> programasSelecionados)
        {
            var uniID = unidadeID;
            var itemsPrograma = programasSelecionados;
            List<Programa> programas = new List<Programa>();
            foreach (var item in itemsPrograma)
            {
                programas.Add(item);
            }

            if (programas.Count <= 0)
            {
                return null;
            }

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
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Month == mes)
                                       .Where(s => s.Sincronizacao.Exibicao.Data.Year == ano)
                                       .Where(s => s.Sincronizacao.Exibicao.UnidadeID == uniID)
                                       .Where(s => s.Sincronizacao.Aberto == false)
                                       .Where(s => s.Sincronizacao.Aprovado)
                                       .Where(s =>
                                              s.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                                              s.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"))
                                       .Where(s => s.Musica.TipoTrilha.Descricao.ToUpper().Contains("COMERCIAL"));

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

                var resultSons = sons.OrderBy(s => s.Sincronizacao.Exibicao.Data)
                                     .ThenBy(s => s.Sincronizacao.Exibicao.Programa.Nome)
                                     .Select(s =>
                                             new RowRelatorioPgtoAberto()
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
                                                     Titulo = s.Musica.Titulo,
                                                     MusicaID = s.MusicaID,
                                                     SonorizacaoID = s.SonorizacaoID,
                                                     ClassificacaoID = s.ClassificacaoID,
                                                     ProgramaID = s.Sincronizacao.Exibicao.ProgramaID
                                                 });

                var groupSons = from s in resultSons
                                group s by new {s.Programa, s.Data, s.Titulo, s.Classificacao}
                                into sg
                                select sg;

                var groupList = groupSons.ToList();

                foreach (var groupSon in groupList)
                {
                    double soma = 0;
                    decimal somaPorcentagemGrupo = 0;
                    decimal somaValorGrupo = 0;
                    bool comercial = false;
                    foreach (var rowRelatorioPgtoAberto in groupSon)
                    {
                        rowRelatorioPgtoAberto.Segundos = rowRelatorioPgtoAberto.Minutagem.TotalSeconds;
                        soma += rowRelatorioPgtoAberto.Segundos;
                        comercial = rowRelatorioPgtoAberto.TipoTrilha.Contains("COMERCIAL");
                        if (repositorio.Autorizacoes.Any(a =>
                                                         a.SonorizacaoID == rowRelatorioPgtoAberto.SonorizacaoID))
                        {
                            somaPorcentagemGrupo += repositorio.Autorizacoes
                                                               .Where(
                                                                   a =>
                                                                   a.SonorizacaoID ==
                                                                   rowRelatorioPgtoAberto.SonorizacaoID)
                                                               .Sum(a => a.Porcentagem);
                            somaValorGrupo += repositorio.Autorizacoes
                                                         .Where(
                                                             a =>
                                                             a.SonorizacaoID ==
                                                             rowRelatorioPgtoAberto.SonorizacaoID)
                                                         .Sum(a => a.Valor);
                        }
                    }
                    //300 segundos = 5 minutos de limite de musicas comerciais
                    if (soma > 300 && comercial)
                        soma = 300;
                    var firstOrDefault = groupSon.FirstOrDefault();

                    if (firstOrDefault != null)
                    {
                        firstOrDefault.Minutagem = TimeSpan.FromSeconds(soma);
                        firstOrDefault.Segundos = soma;
                        firstOrDefault.Porcentagem = 100 - somaPorcentagemGrupo;
                        firstOrDefault.Valor = somaPorcentagemGrupo > 0
                                                   ? ((somaValorGrupo*100)/somaPorcentagemGrupo) - somaValorGrupo
                                                   : ObtemPreco(firstOrDefault, repositorio);
                    }
                }

                groupList.RemoveAll(group => group.FirstOrDefault().Porcentagem <= 0);

                var rowsECAD = from s in groupList
                               select new RowRelatorioPgtoAberto()
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
                                              Porcentagem = s.FirstOrDefault().Porcentagem,
                                              Valor = decimal.Round(s.FirstOrDefault().Valor,2)
                                          };

                return rowsECAD.ToList();
            }
        }

        private decimal ObtemPreco(RowRelatorioPgtoAberto pgto, Context ctx)
        {
            var anoVigencia = pgto.Data.Year;
            var genID = 0;
            if (pgto.GeneroID == 0)
            {
                genID = ctx.Programas.FirstOrDefault(p => p.ProgramaID == pgto.ProgramaID).GeneroID;
            }

            var precos = ctx.Precos
                            .Where(p =>
                                   p.ClassificacaoID == pgto.ClassificacaoID &&
                                   p.GeneroID == genID &&
                                   p.Vigencia == anoVigencia)
                            .OrderByDescending(p => p.Valor);
            return precos.FirstOrDefault().Valor;
        }
    }
}