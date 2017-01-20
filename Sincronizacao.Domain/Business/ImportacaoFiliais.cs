using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.Domain.Business
{
    public class ImportacaoFiliais
    {
        private string _sCaminhoDoArquivo;
        private DateTime _dataRoteiro;

        /// <summary>
        /// Importa musicas a partir de um arquivo Excel
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="totalMusicas"></param>
        /// <param name="totalImportadas"></param>
        /// <param name="totalErros"></param>
        /// <returns></returns>
        public List<RowVetrixErro> Importar(string arquivo, out int totalMusicas, out int totalImportadas,
                                         out int totalErros)
        {
            ImportacaoMusicas importacao = new ImportacaoMusicas();
            _sCaminhoDoArquivo = arquivo;

            if (importacao.ArquivoImportado(_sCaminhoDoArquivo))
                throw new Exception("Este arquivo já foi importado anteriormente.");

            DataSet ds = ImportExcelXLS(_sCaminhoDoArquivo);
            var importadas = PopularTrilhasImportadas(ds);

            _dataRoteiro = DateTime.Parse(importadas[0].ExibidoEm.ToShortDateString());

            return new ImportacaoMusicas().Importar(importadas, _dataRoteiro, arquivo, out totalMusicas,
                                                    out totalImportadas, out totalErros, false);
        }

        /// <summary>
        /// Importa musicas de Novelas a partir de um arquivo Excel
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="totalMusicas"></param>
        /// <param name="totalImportadas"></param>
        /// <param name="totalErros"></param>
        /// <returns></returns>
        public List<RowVetrixErroNovela> ImportarNovela(string arquivo, out int totalMusicas, out int totalImportadas,
                                               out int totalErros)
        {
            ImportacaoMusicas importacao = new ImportacaoMusicas();
            _sCaminhoDoArquivo = arquivo;

            if (importacao.ArquivoImportado(_sCaminhoDoArquivo))
                throw new Exception("Este arquivo já foi importado anteriormente.");

            DataSet ds = ImportExcelXLS(_sCaminhoDoArquivo);
            var importadas = PopularTrilhasImportadasNovelas(ds);

            _dataRoteiro = DateTime.Parse(importadas[0].ExibidoEm.ToShortDateString());

            return new ImportacaoMusicas().ImportarNovelas(importadas, _dataRoteiro, arquivo, out totalMusicas,
                                                    out totalImportadas, out totalErros, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static DataSet ImportExcelXLS(string fileName)
        {
            string strConn = "";
            var extension = Path.GetExtension(fileName);
            if (extension != null)
            {
                string fileExtension = extension.ToLower();
                if (fileExtension == ".xlsx" || fileExtension == ".xls")
                    strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName +
                              ";Extended Properties=Excel 12.0;";
            }

            var output = new DataSet();

            using (var conn = new OleDbConnection(strConn))
            {
                conn.Open();

                DataTable schemaTable = conn.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables, new object[] {null, null, null, "TABLE"});

                if (schemaTable != null)
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
                                throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, fileName),
                                                    ex);
                            }
                        }
                    }
            }
            return output;
        }


        /// <summary>
        /// Popula a lista de trilhas a partir de um dataset
        /// </summary>
        /// <param name="dataSetExcel">dataset que teve carga obtida pelo excel </param>
        /// <returns></returns>
        private List<TrilhaImportada> PopularTrilhasImportadas(DataSet dataSetExcel)
        {
            List<TrilhaImportada> trilhas = new List<TrilhaImportada>();

            for (int i = 0; i < dataSetExcel.Tables.Count; i++)
            {
                int linhaAtual = 1; //valor inicial referente ao cabeçalho
                foreach (DataRow dr in dataSetExcel.Tables[i].Rows)
                {
                    linhaAtual++;

                    if (VerificaLinhaVazia(dr))
                        continue;
                    VerificaColunaVazia(dr, linhaAtual);


                    var ti = new TrilhaImportada();

                    if (!DateTime.TryParse(dr["EXIBICAO"].ToString(), out _dataRoteiro))
                        throw new Exception("Musica: "
                                            + dr["NOME_DA_MUSICA"].ToString().Normalizar()
                                            + " - Não existe data de roteiro na planilha.");

                    ti.Arquivo = _sCaminhoDoArquivo;
                    ti.ImportadoEm = DateTime.Now;

                    ti.Programa = dr["PROGRAMA"].ToString().Normalizar();

                    ti.TipoExibicao = dr["TIPO_DE_EXIBICAO"].ToString().Normalizar();

                    ti.ExibidoEm = Convert.ToDateTime(dr["EXIBICAO"].ToString());

                    ti.Unidade = dr["UNIDADE"].ToString().Normalizar();

                    ti.Musica = dr["NOME_DA_MUSICA"].ToString().Normalizar();

                    ti.Autores = dr["AUTORES"].ToString().Normalizar();

                    ti.Interpretes = dr["INTERPRETES"].ToString().Normalizar();

                    ti.Classificacao = dr["CLASSIFICACAO"].ToString().Normalizar();

                    ti.Minutagem = new TimeSpan(0, 0, 0);

                    string[] s = dr["MINUTAGEM"].ToString().Split(':');
                    if (s.Count() == 3)
                        ti.Minutagem = new TimeSpan(0, Convert.ToInt32(s[1]), Convert.ToInt32(s[2]));
                    else if (s.Count() == 2)
                        ti.Minutagem = new TimeSpan(0, Convert.ToInt32(s[0]), Convert.ToInt32(s[1]));
                    else
                        throw new Exception("A MINUTAGEM da musica " + ti.Musica +
                                            " está com valor ou formatação invalido.");

                    ti.Quadro = dr["QUADRO"].ToString().Normalizar();

                    trilhas.Add(ti);
                }
            }
            return trilhas;
        }

        /// <summary>
        /// Popula a lista de trilhas de novela a partir de um dataset
        /// </summary>
        /// <param name="dataSetExcel">dataset que teve carga obtida pelo excel </param>
        /// <returns></returns>
        private List<TrilhaImportada> PopularTrilhasImportadasNovelas(DataSet dataSetExcel)
        {
            List<TrilhaImportada> trilhas = new List<TrilhaImportada>();

            for (int i = 0; i < dataSetExcel.Tables.Count; i++)
            {
                int linhaAtual = 1; //valor inicial referente ao cabeçalho
                foreach (DataRow dr in dataSetExcel.Tables[i].Rows)
                {
                    linhaAtual++;

                    if (VerificaLinhaVazia(dr))
                        continue;
                    VerificaColunaVaziaNovelas(dr, linhaAtual);


                    var ti = new TrilhaImportada();

                    if (!DateTime.TryParse(dr["DATA EXIBICAO"].ToString(), out _dataRoteiro))
                        throw new Exception("Musica: "
                                            + dr["TITULO"].ToString().Normalizar()
                                            + " - Não existe data de roteiro na planilha.");

                    ti.Arquivo = _sCaminhoDoArquivo;
                    ti.ImportadoEm = DateTime.Now;

                    ti.TituloNacional = dr["TITULO NACIONAL"].ToString().Normalizar();

                    //ti.TipoExibicao = dr["CARACTERISTICAS"].ToString().Normalizar();

                    ti.ExibidoEm = Convert.ToDateTime(dr["DATA EXIBICAO"].ToString());

                    ti.Unidade = dr["UNIDADE"].ToString().Normalizar();

                    ti.Musica = dr["TITULO"].ToString().Normalizar();

                    ti.Autores = dr["AUTOR"].ToString().Normalizar();

                    ti.Interpretes = dr["INTERPRETE"].ToString().Normalizar();

                    ti.Classificacao = dr["CARACTERISTICAS"].ToString().Normalizar();

                    ti.Capitulo = int.Parse(dr["CAPITULO"].ToString().Normalizar());
                    ti.Diretor = dr["DIRETOR"].ToString().Normalizar();
                    ti.Produtor = dr["PRODUTOR"].ToString().Normalizar();
                    ti.Destinacao = dr["DESTINACAO"].ToString().Normalizar();
                    ti.Segundos = int.Parse(dr["SEGUNDOS"].ToString().Normalizar());
                    ti.Minutagem = new TimeSpan(0, 0, ti.Segundos);
                    ti.Ordem = int.Parse(dr["ORDEM"].ToString().Normalizar());
                    ti.Editora = dr["EDITORA"].ToString().Normalizar();
                    ti.Gravadora = dr["GRAVADORA"].ToString().Normalizar();
                    ti.Categoria = dr["CATEGORIA"].ToString().Normalizar();
                    ti.Duracao = Convert.ToDateTime(dr["DURACAO"].ToString()).TimeOfDay;
                    trilhas.Add(ti);
                }
            }
            return trilhas;
        }

        private bool VerificaLinhaVazia(DataRow dr)
        {
            //verifica se a linha toda esta vazia
            int countVazia = dr.ItemArray.Count(item => string.IsNullOrWhiteSpace(item.ToString()));
            return countVazia >= 10;
        }

        private void VerificaColunaVazia(DataRow dr, int linhaAtual)
        {
            //verifica cada campo e ataca uma exception
            if (dr.IsNull("EXIBICAO"))
                throw new Exception("Linha " + linhaAtual + ": EXIBICAO esta vazio.");
            if (dr.IsNull("PROGRAMA"))
                throw new Exception("Linha " + linhaAtual + ": PROGRAMA esta vazio.");
            if (dr.IsNull("TIPO_DE_EXIBICAO"))
                throw new Exception("Linha " + linhaAtual + ": TIPO_DE_EXIBICAO esta vazio.");
            if (dr.IsNull("UNIDADE"))
                throw new Exception("Linha " + linhaAtual + ": UNIDADE esta vazio.");
            if (dr.IsNull("NOME_DA_MUSICA"))
                throw new Exception("Linha " + linhaAtual + ": NOME_DA_MUSICA esta vazio.");
            if (dr.IsNull("AUTORES"))
                throw new Exception("Linha " + linhaAtual + ": AUTORES esta vazio.");
            if (dr.IsNull("INTERPRETES"))
                throw new Exception("Linha " + linhaAtual + ": INTERPRETES esta vazio.");
            if (dr.IsNull("CLASSIFICACAO"))
                throw new Exception("Linha " + linhaAtual + ": CLASSIFICACAO esta vazio.");
            if (dr.IsNull("MINUTAGEM"))
                throw new Exception("Linha " + linhaAtual + ": MINUTAGEM esta vazio.");
            if (dr.IsNull("QUADRO"))
                throw new Exception("Linha " + linhaAtual + ": QUADRO esta vazio.");
        }

        private void VerificaColunaVaziaNovelas(DataRow dr, int linhaAtual)
        {
            //verifica cada campo e ataca uma exception
            if (dr.IsNull("UNIDADE"))
                throw new Exception("Linha " + linhaAtual + ": UNIDADE esta vazio.");
            if (dr.IsNull("TITULO NACIONAL"))
                throw new Exception("Linha " + linhaAtual + ": TITULO NACIONAL esta vazio.");
            if (dr.IsNull("CAPITULO"))
                throw new Exception("Linha " + linhaAtual + ": CAPITULO esta vazio.");
            if (dr.IsNull("DATA EXIBICAO"))
                throw new Exception("Linha " + linhaAtual + ": DATA EXIBICAO esta vazio.");
            if (dr.IsNull("PRODUTOR"))
                throw new Exception("Linha " + linhaAtual + ": PRODUTOR esta vazio.");
            if (dr.IsNull("DIRETOR"))
                throw new Exception("Linha " + linhaAtual + ": DIRETOR esta vazio.");
            if (dr.IsNull("CATEGORIA"))
                throw new Exception("Linha " + linhaAtual + ": CATEGORIA esta vazio.");
            if (dr.IsNull("DESTINACAO"))
                throw new Exception("Linha " + linhaAtual + ": DESTINACAO esta vazio.");
            if (dr.IsNull("ORDEM"))
                throw new Exception("Linha " + linhaAtual + ": ORDEM esta vazio.");
            if (dr.IsNull("TITULO"))
                throw new Exception("Linha " + linhaAtual + ": TITULO esta vazio.");
            if (dr.IsNull("SEGUNDOS"))
                throw new Exception("Linha " + linhaAtual + ": SEGUNDOS esta vazio.");
            if (dr.IsNull("CARACTERISTICAS"))
                throw new Exception("Linha " + linhaAtual + ": CARACTERISTICAS esta vazio.");
            if (dr.IsNull("INTERPRETE"))
                throw new Exception("Linha " + linhaAtual + ": INTERPRETE esta vazio.");
            if (dr.IsNull("EDITORA"))
                throw new Exception("Linha " + linhaAtual + ": EDITORA esta vazio.");
            if (dr.IsNull("AUTOR"))
                throw new Exception("Linha " + linhaAtual + ": AUTOR esta vazio.");
            if (dr.IsNull("GRAVADORA"))
                throw new Exception("Linha " + linhaAtual + ": GRAVADORA esta vazio.");
        }
    }
}