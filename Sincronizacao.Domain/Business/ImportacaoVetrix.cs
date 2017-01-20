using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Xml.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.Domain.Business
{
    public class ImportacaoVetrix
    {
        private string sCaminhoDoArquivo;
        private DateTime dataRoteiro;
        private Context context = SingletonContext.Instance.Context;
        public List<RowVetrixErro> Importar(string arquivo, out int totalMusicas, out int totalImportadas, out int totalErros)
        {
            sCaminhoDoArquivo = arquivo;
            ImportacaoMusicas importacao = new ImportacaoMusicas();
            var root = XElement.Load(arquivo);

            ValidacaoXML.ValidarXml(sCaminhoDoArquivo, Environment.CurrentDirectory + @"\Schema\SINCRECORD.xsd");

            List<TrilhaImportada> importadas = ImportByLinq(root);

            dataRoteiro = DateTime.Parse(root.FirstAttribute.Value);

            return importacao.Importar(importadas, dataRoteiro, arquivo, out totalMusicas, out totalImportadas, out totalErros, true);
        }

        public void ExcluirExibicaoExistente(DateTime exibidoEm, string programa, string unidade)
        {
            var exib = (from ex in context.Exibicoes
                             where ex.Programa.Nome == programa &&
                                   EntityFunctions.DiffDays(ex.Data, exibidoEm) == 0
                                   && ex.Unidade.Nome == unidade
                             select ex).FirstOrDefault();

            if (exib != null && exib.ExibicaoID != 0)
            {
                var sinc = (from s in context.Sincronizacoes
                           where s.ExibicaoID == exib.ExibicaoID
                           select s).FirstOrDefault();
                if (sinc != null)
                {
                    if (!sinc.Aprovado)
                    {
                        try
                        {
                            context.Exibicoes.Remove(exib);
                            context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex.Message);
                        }
                    }
                }
            }
            if (context.Database.Connection.State == System.Data.ConnectionState.Open)
                context.Database.Connection.Close();
        }

        public List<TrilhaImportada> ImportByLinq(XElement root)
        {
            Console.WriteLine("Lendo XML by Linq" + Environment.NewLine);
            int nNodes = 0;

            DateTime tInicioExecucao = DateTime.Now;

            TimeSpan tExecucao = new TimeSpan();

            var importadas = new List<TrilhaImportada>();

            foreach (var praca in root.Elements("PRACA"))
            {
                string unidade = praca.Attribute("RAZAOSOCIAL").Value;

                var query = from trilha in praca.Elements("TRILHA")
                            select new
                                       {
                                           Unidade = unidade,
                                           Programa = trilha.Element("PROGRAMA").Value,
                                           ExibidoEm = trilha.Element("DATA_EXIBICAO").Value,
                                           Duracao = trilha.Element("DURACAO").Value,
                                           Trilha = trilha.Element("TRILHA").Value,
                                           Autores = trilha.Element("AUTOR").Value,
                                           CodAlbum = trilha.Element("CODIGO_ALBUM").Value,
                                           Album = trilha.Element("ALBUM").Value,
                                           Interpretes = trilha.Element("INTERPRETE").Value,
                                           ISRC = trilha.Element("ISRC").Value,
                                           VetrixID = trilha.Element("CODIGO_VETRIX").Value,
                                       };

                //Efetuamos um for para varrermos todos os itens  
                //TODO: importar musicas verificando unidade
                try
                {
                    foreach (var trilha in query)
                    {
                        var trilhaImportada = new TrilhaImportada
                                                  {
                                                      Unidade = trilha.Unidade,
                                                      Programa = trilha.Programa,
                                                      ExibidoEm = DateTime.Parse(trilha.ExibidoEm),
                                                      Minutagem = TimeSpan.Parse(trilha.Duracao),
                                                      Musica = trilha.Trilha,
                                                      Autores = trilha.Autores,
                                                      Interpretes = trilha.Interpretes,
                                                      CodAlbum = trilha.CodAlbum,
                                                      Album = trilha.Album,
                                                      ISRC = trilha.ISRC,
                                                      Vetrix = int.Parse(trilha.VetrixID)
                                                  };

                        ExcluirExibicaoExistente(trilhaImportada.ExibidoEm, trilhaImportada.Programa, trilhaImportada.Unidade);

                        importadas.Add(trilhaImportada);
                        nNodes++;
                    }
                }
                catch (Exception ex)
                {
                    //TODO: implementar Logs
                    Console.Write("Roteiro - " + sCaminhoDoArquivo + " Erro: " + ex.Message);
                    throw new Exception("Erro - ImportByLinq()");
                }
            }
            tExecucao = DateTime.Now.Subtract(tInicioExecucao);
            Console.WriteLine("Numero de Nós: " + nNodes);
            Console.WriteLine("Tempo Execução:" + tExecucao.Milliseconds);

            return importadas;
        }

        //TODO: ExisteSincronizacao ?????
    }
}
