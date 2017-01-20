using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.OleDb;
using System.Linq;
using System.Text.RegularExpressions;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;
using Sincronizacao.Util;

namespace Sincronizacao.UI
{
    internal class ImportarFiliais
    {
        private string sCaminhoDoArquivo;
        private DateTime dataRoteiro;
        private Sonorizacao lastSon = null;
        private List<TrilhaImportada> importadas = null;
        private Context context = SingletonContext.Instance.Context;
        private Usuario user = null;

        public static DataSet ImportExcelXLS(string FileName)
        {
            string strConn = "";
            string fileExtension = FileName.Substring(FileName.LastIndexOf('.')).ToLower();
            if (fileExtension == ".xlsx" || fileExtension == ".xls" )
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName +
                          ";Extended Properties=Excel 12.0;";

            var output = new DataSet();

            using (var conn = new OleDbConnection(strConn))
            {
                conn.Open();

                DataTable schemaTable = conn.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables, new object[] {null, null, null, "TABLE"});

                foreach (DataRow schemaRow in schemaTable.Rows)
                {
                    string sheet = schemaRow["TABLE_NAME"].ToString();

                    if (!sheet.EndsWith("_") && !sheet.EndsWith("Print_Area"))
                    {
                        try
                        {
                            OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                            cmd.CommandType = CommandType.Text;

                            DataTable outputTable = new DataTable(sheet);
                            output.Tables.Add(outputTable);
                            new OleDbDataAdapter(cmd).Fill(outputTable);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, FileName), ex);
                        }
                    }
                }
            }
            return output;
        }

        public List<Vetrix> Importar(string arquivo, out int totalMusicas, out int totalImportadas, out int totalErros)
        {
            totalMusicas = 0;
            totalImportadas = 0;
            totalErros = 0;

            sCaminhoDoArquivo = arquivo;

            DataSet ds = ImportExcelXLS(sCaminhoDoArquivo);
            importadas = new List<TrilhaImportada>();
            TrilhaImportada ti = null;
            var trata = new Tratamento();

            List<Vetrix> vetrixes = new List<Vetrix>();



            for (int i = 0; i < ds.Tables.Count; i++)
            {
                foreach (DataRow dr in ds.Tables[i].Rows)
                {
                    if(dr.IsNull("PROGRAMA"))
                        continue;
                    string[] s = dr["MINUTAGEM"].ToString().Split('\'');
                    ti = new TrilhaImportada();
                    if (DateTime.TryParse(dr["LEVADO_AO_AR_EM"].ToString(), out dataRoteiro))
                    {
                        ti.Arquivo = arquivo;
                        ti.ImportadoEm = DateTime.Now;
                        ti.Programa =
                            Regex.Replace(
                                trata.RemoveCaracteresEspeciais(dr["PROGRAMA"].ToString().ToUpper(), true, false),
                                @"\s+", " ");
                        ti.TipoExibicao =
                            Regex.Replace(
                                trata.RemoveCaracteresEspeciais(dr["GRAVADO_EM"].ToString().ToUpper(), true, false),
                                @"\s+", " ");
                        ti.ExibidoEm = Convert.ToDateTime(dr["LEVADO_AO_AR_EM"].ToString());

                        ti.Unidade =
                            Regex.Replace(
                                trata.RemoveCaracteresEspeciais(dr["FILIAL"].ToString().ToUpper(), true, false),
                                @"\s+", " ");
                        ti.Musica =
                            Regex.Replace(
                                trata.RemoveCaracteresEspeciais(dr["NOME_DA_MUSICA"].ToString().ToUpper(), true,
                                                                false),
                                @"\s+"," ");
                        ti.Autores =
                            Regex.Replace(
                                trata.RemoveCaracteresEspeciais(dr["AUTOR(ES)"].ToString().ToUpper(), true, false),
                                @"\s+", " ");
                        ti.Interpretes =
                            Regex.Replace(
                                trata.RemoveCaracteresEspeciais(dr["INTERPRETES"].ToString().ToUpper(), true, false),
                                @"\s+", " ");
                        if (!string.IsNullOrEmpty(dr["FUNDO"].ToString()))
                            ti.Classificacao = "FUNDO";
                        else if (!string.IsNullOrEmpty(dr["ABERT"].ToString()))
                            ti.Classificacao = "ABERTURA";
                        else if (!string.IsNullOrEmpty(dr["TEMA"].ToString()))
                            ti.Classificacao = "TEMA";
                        else if (!string.IsNullOrEmpty(dr["PERFO"].ToString()))
                            ti.Classificacao = "PERFOMANCE";
                        else if (!string.IsNullOrEmpty(dr["ADOR"].ToString()))
                            ti.Classificacao = "ADORNO";
                        else if (!string.IsNullOrEmpty(dr["F_JORNAL"].ToString()))
                            ti.Classificacao = "FUNDO JORNALISTICO";
                        ti.Minutagem = new TimeSpan(0, Convert.ToInt32(s[0]), Convert.ToInt32(s[1]));

                        importadas.Add(ti);
                    }
                    else
                    {
                        //TODO: implementar log importacao filiais
                        throw new Exception("Musica: " +
                            Regex.Replace(trata.RemoveCaracteresEspeciais(dr["NOME_DA_MUSICA"].ToString().ToUpper(), true,false),@"\s+"," ")
                            + " - Não existe data de roteiro na planilha.");
                    }
                }
            }

            //TODO: Refatorar importacao excel
            try
            {
                totalMusicas = importadas.Count;
                foreach (var trilha in importadas)
                {
                    Sonorizacao son;
                    try
                    {
                        if (lastSon == null)
                        {
                            AddTrilhaToSonorizacao(out son, trilha);
                        }
                        if (lastSon.Exibicao.Data.ToShortDateString() != trilha.ExibidoEm.ToShortDateString()
                            || lastSon.Exibicao.Programa.Nome != trilha.Programa
                            || lastSon.Exibicao.Unidade.Nome != trilha.Unidade)
                        {
                            AddTrilhaToSonorizacao(out son, trilha);
                            if (son.Exibicao.ProgramaID == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            son = lastSon;
                        }
                    }
                    catch (Exception ex)
                    {
                        Vetrix vetrix = new Vetrix
                                            {
                                                ExibidoEm = trilha.ExibidoEm,
                                                ImportadoEm = DateTime.Now,
                                                Processado =
                                                    ex.Message,
                                                ProgramaID = 0,
                                                Roteiro = dataRoteiro.ToShortDateString(),
                                                UnidadeID = 0,
                                                UsuarioID = 1, //TODO: Obter Usuario
                                                VetrixID = trilha.Vetrix
                                            };
                        vetrixes.Add(vetrix);
                        context.Vetrix.Add(vetrix);
                        context.SaveChanges();
                        totalErros++;
                        continue;
                    }
                    //importando musicas da sonorizacao
                    MusicaSonorizacao musicaSonorizacao = new MusicaSonorizacao();

                    musicaSonorizacao.Sonorizacao = son;

                    //tipo de exibicao
                    musicaSonorizacao.TipoExibicaoID = (from te in context.TipoExibicao
                                                        where te.Descricao.Contains("NÃO DEFIN")
                                                        select te.TipoExibicaoID).SingleOrDefault();

                    //classificacao
                    musicaSonorizacao.ClassificacaoID = (from c in context.Classificacoes
                                                         where c.Descricao.Contains("NÃO DEFIN")
                                                         select c.ClassificacaoID).SingleOrDefault();

                    //quadro
                    musicaSonorizacao.QuadroID = (from q in context.Quadros
                                                  where q.Descricao.Contains("NÃO DEFIN")
                                                  select q.QuadroID).SingleOrDefault();

                    Musica musica = (from m in context.Musicas
                                     where m.Titulo == trilha.Musica
                                     select m).FirstOrDefault();
                    if (musica != null)
                    {
                        //verifica se musica ja foi sonorizada
                        if (!ExistsSonorizacao(musica, trilha.ExibidoEm))
                        {
                            musicaSonorizacao.Musica = musica;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        vetrixes.Add(new Vetrix
                                         {
                                             ExibidoEm = trilha.ExibidoEm,
                                             ImportadoEm = DateTime.Now,
                                             Processado =
                                                 "Musica " + trilha.Musica + " não encontrada na base de dados.",
                                             ProgramaID = son.Exibicao.ProgramaID,
                                             Roteiro = dataRoteiro.ToShortDateString(),
                                             UnidadeID = son.Exibicao.UnidadeID,
                                             UsuarioID = user.UsuarioID,
                                             VetrixID = trilha.Vetrix
                                         });
                        totalErros++;
                        continue;
                    }

                    //TODO: Importar todos os itens;
                    //TODO: Verificar se musica de importacao existe no banco
                    context.MusicaSonorizacoes.Add(musicaSonorizacao);
                    context.SaveChanges();
                    totalImportadas++;
                }
            }
            catch (Exception ex)
            {
                //TODO: implementar log importacao vetrix
                Console.WriteLine("Erro importacao vetrix: " + ex.Message);
                throw new Exception("Erro - Importar()");
            }
            return vetrixes;
        }

        /// <summary>
        /// Verifica se existe sonoriação com a musica passada por parametro
        /// </summary>
        /// <param name="musica">Musica que sera procurada nas sonorizações</param>
        /// <param name="dataExibicao">Indica que dia da exibição em que a musica foi tocada. </param>
        /// <returns></returns>
        private bool ExistsSonorizacao(Musica musica,DateTime dataExibicao)
        {
            return (from ms in context.MusicaSonorizacoes
                    where ms.MusicaID == musica.MusicaID
                    && EntityFunctions.DiffDays(dataExibicao, ms.Sonorizacao.Exibicao.Data) == 0 
                    select ms
                   ).Any();
        }

        //TODO: Refatorar AddTrilhaToSonorizacao
        protected void AddTrilhaToSonorizacao(out Sonorizacao son, TrilhaImportada trilha)
        {
            //Preenchendo sonorizacao
            son = new Sonorizacao();
            //TODO: Excluir first e verificar porque gera mais de uma exibicao do mesmo programa provavelmente na importacao

            Exibicao exib = (from ex in context.Exibicoes
                             where ex.Data == trilha.ExibidoEm
                                   && ex.Programa.Nome == trilha.Programa
                             select ex).FirstOrDefault();

            if (exib == null || exib.ExibicaoID == 0)
            {
                exib = new Exibicao();

                exib.Data = trilha.ExibidoEm;


                Programa prog = (from p in context.Programas
                                 where p.Nome == trilha.Programa
                                 select p).SingleOrDefault();
                //TODO: Importação: Verificar similaridade programa
                if (prog != null)
                {
                    exib.Programa = prog;
                }
                else
                {
                    throw new Exception("Programa " + trilha.Programa + " não cadastrado na base de dados");
                }

                Unidade uni = (from u in context.Unidades
                               where u.Nome == trilha.Unidade
                               select u).SingleOrDefault();
                //TODO: Importação: Verificar similaridade Unidade
                if (uni != null)
                {
                    exib.Unidade = uni;
                }
                else
                {
                    throw new Exception("Unidade " + trilha.Unidade + " não cadastrada na base de dados");
                }

                context.Exibicoes.Add(exib);
                context.SaveChanges();
            }

            son.Exibicao = exib;

            user = new Usuario {Analista = true, Login = Environment.UserName};

            son.Usuario = user;

            //atualiza ultima sonorizacao
            if (son.Exibicao.ProgramaID != 0)
            {
                lastSon = son;
            }
        }
    }
}
