using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Domain.Entities;
using System.Data.Objects;

namespace SincronizacaoMusical.Domain.Business
{
    public class RelatorioCanhoto
    {
        public List<RowRelatorioCanhoto> PesquisarCanhoto(int unidadeID, DateTime dataInicial, DateTime dataFinal,
                                                          List<Editora> editorasSelecionadas, int generoID,
                                                          List<Programa> programasSelecionados)
        {
            var uniID = unidadeID;
            var itemsPrograma = programasSelecionados;
            var itemsEditora = editorasSelecionadas;

            List<Programa> programas = new List<Programa>();
            foreach (var item in itemsPrograma)
            {
                programas.Add(item);
            }

            if (programas.Count <= 0)
            {
                return null;
            }
            List<Editora> editoras = new List<Editora>();
            foreach (var item in itemsEditora)
            {
                editoras.Add(item);
            }
            if (editoras.Count <= 0)
            {
                return null;
            }
            TimeSpan diff = dataFinal.Subtract(dataInicial);
            using (Context repositorio = new Context())
            {
                repositorio.Configuration.ProxyCreationEnabled = true;
                var sonsFiltroProgramas = new List<Autorizacao>();
                var sons = new List<Autorizacao>();
                var query = repositorio.Autorizacoes
                                       .Include("Editora")
                                       .Include("Sonorizacao")
                                       .Include("Sonorizacao.Sincronizacao")
                                       .Include("Sonorizacao.Sincronizacao.Exibicao")
                                       .Include("Sonorizacao.Sincronizacao.Exibicao.Programa")
                                       .AsNoTracking()
                                      .Where(
                                           aut =>
                                           EntityFunctions.DiffDays(dataInicial, aut.Vencimento).Value <= diff.TotalDays &&
                                           EntityFunctions.DiffDays(dataInicial, aut.Vencimento).Value >= 0)
                                       .Where(s => s.Sonorizacao.Sincronizacao.Exibicao.UnidadeID == uniID)
                                       .Where(s => s.Sonorizacao.Sincronizacao.Aberto == false)
                                       .Where(s => s.Sonorizacao.Sincronizacao.Aprovado)
                                       .Where(
                                           s =>
                                           s.Sonorizacao.TipoExibicao.Descricao.ToUpper().Contains("VT") ||
                                           s.Sonorizacao.TipoExibicao.Descricao.ToUpper().Contains("REPRISE"))
                                       .Where(
                                           s =>
                                           s.Sonorizacao.Musica.TipoTrilha.Descricao.ToUpper().Contains("COMERCIAL"));

                if (generoID != 0)
                {
                    query = query
                            .Where(s => s.Sonorizacao.Sincronizacao.Exibicao.Programa.GeneroID == generoID);
                }

                if (editoras.Any(e => e.Nome.Contains("--Todos--")))
                {
                    sonsFiltroProgramas.AddRange(
                        query
                            .Where(ex => ex.EditoraID > 0)
                            .AsQueryable()
                        );
                }
                else
                {
                    foreach (var editora in editoras)
                    {
                        sonsFiltroProgramas.AddRange(
                            query
                                .Where(ex => ex.EditoraID == editora.EditoraID)
                                .AsQueryable()
                            );
                    }
                }

                if (programas.Any(p => p.Nome.Contains("--Todos--")))
                {
                    sons.AddRange(
                        sonsFiltroProgramas
                            .Where(ex => ex.Sonorizacao.Sincronizacao.Exibicao.ProgramaID > 0)
                            .AsQueryable()
                        );
                }
                else
                {
                    foreach (var programa in programas)
                    {
                        sons.AddRange(
                            sonsFiltroProgramas
                                .Where(ex => ex.Sonorizacao.Sincronizacao.Exibicao.ProgramaID == programa.ProgramaID)
                                .AsQueryable()
                            );
                    }
                }

                var resultSons = sons
                    .OrderBy(s => s.Editora.Nome)
                    .ThenBy(s => s.Vencimento)
                    .ThenBy(s => s.Sonorizacao.Sincronizacao.Exibicao.Programa.Nome)
                    .Select(s => s);

                var groupSons = from s in resultSons
                                group s by new
                                               {
                                                   CHAVE =
                                    s.EditoraID.ToString() + "/" + s.Vencimento + "/" +
                                    s.Sonorizacao.Sincronizacao.Exibicao.ProgramaID,
                                                   EDITORA = s.Editora.Nome,
                                                   s.Vencimento,
                                                   s.Sonorizacao.Sincronizacao.Exibicao.Programa.Nome
                                               }
                                into sg
                                select sg;

                var groupList = groupSons.ToList();

                var rowsProvisao = from s in groupList
                                   //group s by
                                   //    new
                                   //        {
                                   //            s.Key
                                   //        }
                                   //    into sg
                                   select new RowRelatorioCanhoto
                                              {
                                                  Editora = s.FirstOrDefault().Editora.Nome,
                                                  Vencimento = s.FirstOrDefault().Vencimento,
                                                  Programa =
                                                      s.FirstOrDefault()
                                                       .Sonorizacao.Sincronizacao.Exibicao.Programa.Nome,
                                                  Ordem =
                                                      s.FirstOrDefault()
                                                       .Sonorizacao.Sincronizacao.Exibicao.Programa.Ordem,
                                                  Total = s.Sum(v => v.Valor)
                                              };

                var result = rowsProvisao.Distinct().ToList();
                result.Add(
                    new RowRelatorioCanhoto()
                    {
                        Total = rowsProvisao.Sum(s => s.Total)
                    });
                return result;
            }
        }
    }
}