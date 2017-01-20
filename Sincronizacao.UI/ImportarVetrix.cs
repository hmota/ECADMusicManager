using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Xml.Linq;
using Sincronizacao.Domain;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.UI
{
    public class ImportarVetrix
    {
        private XElement root = null;
        private string sCaminhoDoArquivo;
        private DateTime dataRoteiro;
        private DateTime tInicioExecucao = new DateTime();
        private Sonorizacao lastSon = null;
        private Context context = SingletonContext.Instance.Context;
        Usuario user;
        
        public List<Vetrix> Importar(string arquivo, out int totalMusicas, out int totalImportadas, out int totalErros)
        {
            totalMusicas = 0;
            totalImportadas = 0;
            totalErros = 0;
            sCaminhoDoArquivo = arquivo;

            root = XElement.Load(arquivo);

            List<TrilhaImportada> importadas = ImportByLinq(root);
            var naoImportadas = new List<TrilhaImportada>(); 
            
            dataRoteiro = DateTime.Parse(root.FirstAttribute.Value);

            //bool roteiroExiste = (from v in context.Vetrix
            //                          where EntityFunctions.DiffDays(v.ExibidoEm, dataRoteiro) == 0
            //                          select v).Any();
            //if(roteiroExiste)
            //    throw new Exception("Roteiro já foi importado.");

      

            //Verifica se a data da vetrix para importacao nao é uma data futura
            if(dataRoteiro > DateTime.Now)
                throw  new Exception("A data do roteiro é uma data futura. Não pode ser importada.");


            //TODO: Verificar se a sonorizacao já foi aprovada


            var vetrixes = new List<Vetrix>();
            
            try
            {
                totalMusicas = importadas.Count();
                
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
                    catch (Exception)
                    {
                        var vetrix = new Vetrix
                                            {
                                                ExibidoEm = trilha.ExibidoEm,
                                                ImportadoEm = DateTime.Now,
                                                Processado =
                                                    "Musica " + trilha.Musica + " não encontrada na base de dados.",
                                                ProgramaID = 0,
                                                Roteiro = dataRoteiro.ToShortDateString(),
                                                UnidadeID = 0,
                                                UsuarioID = 1,//TODO: Obter Usuario
                                                VetrixID = trilha.Vetrix
                                            };
                        vetrixes.Add(vetrix);
                        context.Vetrix.Add(vetrix);
                        context.SaveChanges();
                        totalErros++;
                        continue;
                    }
                    //importando musicas da sonorizacao
                    var musicaSonorizacao = new MusicaSonorizacao
                                                              {
                                                                  Sonorizacao = son,
                                                                  TipoExibicaoID = (from te in context.TipoExibicao
                                                                                    where
                                                                                        te.Descricao.Contains(
                                                                                            "NÃO DEFIN")
                                                                                    select te.TipoExibicaoID).
                                                                      SingleOrDefault(),
                                                                  ClassificacaoID = (from c in context.Classificacoes
                                                                                     where
                                                                                         c.Descricao.Contains(
                                                                                             "NÃO DEFIN")
                                                                                     select c.ClassificacaoID).
                                                                      SingleOrDefault(),
                                                                  QuadroID = (from q in context.Quadros
                                                                              where q.Descricao.Contains("NÃO DEFIN")
                                                                              select q.QuadroID).SingleOrDefault()
                                                              };

                    Musica musica = (from m in context.Musicas
                                                  where m.Titulo == trilha.Musica
                                                  select m).FirstOrDefault();
                    if (musica != null)
                        musicaSonorizacao.Musica = musica;
                    else
                    {
                        vetrixes.Add(new Vetrix{ExibidoEm = trilha.ExibidoEm, ImportadoEm = DateTime.Now,
                        Processado = "Musica "+trilha.Musica+" não encontrada na base de dados.",
                        ProgramaID = son.Exibicao.ProgramaID, Roteiro = dataRoteiro.ToShortDateString(),
                        UnidadeID = son.Exibicao.UnidadeID, UsuarioID = user.UsuarioID, VetrixID = trilha.Vetrix
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
                throw new Exception("Erro - Importar()");
            }

            return vetrixes;
        }

        public void AddTrilhaToSonorizacao(out Sonorizacao son, TrilhaImportada trilha)
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
                    throw new Exception("Programa não cadastrado na base de dados");
                }

                Unidade uni = (from u in context.Unidades
                             where u.Nome == trilha.Unidade
                             select u).SingleOrDefault();
                //TODO: Importação: Verificar similaridade Unidade
                exib.Unidade = uni;
                if (uni != null)
                {
                    uni.UnidadeID = uni.UnidadeID;
                }
                else
                {
                    throw new Exception("Unidade não cadastrada na base de dados");
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

        public List<TrilhaImportada> ImportTrilhasByLinq(XElement root)
        {
            Console.WriteLine("Lendo XML by Linq" + Environment.NewLine);
            int nNodes = 0;

            tInicioExecucao = DateTime.Now;

            TimeSpan tExecucao = new TimeSpan();

            DateTime dataRoteiro = DateTime.Parse(root.FirstAttribute.Value);

            List<TrilhaImportada> importadas = new List<TrilhaImportada>();

            foreach (var praca in root.Elements("PRACA"))
            {
                var nomePraca = praca.Attribute("RAZAOSOCIAL").Value;

                var query = from xml in praca.Elements("TRILHA")
                            select new
                                       {
                                           Programa = xml.Element("PROGRAMA").Value,
                                           ExibidoEm = xml.Element("DATA_EXIBICAO").Value,
                                           Duracao = xml.Element("DURACAO").Value,
                                           Musica = xml.Element("TRILHA").Value,
                                           Interprete = xml.Element("INTERPRETE").Value,
                                           ISRC = xml.Element("ISRC").Value,
                                           VETRIX = xml.Element("CODIGO_VETRIX").Value
                                       };

                //Efetuamos um for para varrermos todos os itens  
                //TODO: importar musicas verificando unidade
                try
                {
                    foreach (var xml in query)
                    {

                        TrilhaImportada trilha = new TrilhaImportada();
                        //Declaramos variaveis com seus respectivos valores
                        trilha.Arquivo = sCaminhoDoArquivo;
                        trilha.ExibidoEm = DateTime.Parse(xml.ExibidoEm);
                        trilha.Interpretes = xml.Interprete;
                        trilha.ImportadoEm = DateTime.Now;
                        trilha.ISRC = xml.ISRC;
                        trilha.Minutagem = TimeSpan.Parse(xml.Duracao);
                        trilha.Musica = xml.Musica;
                        trilha.Programa = xml.Programa;
                        trilha.Vetrix = int.Parse(xml.VETRIX);
                        trilha.Unidade = nomePraca;
                        importadas.Add(trilha);
                        nNodes++;
                    }
                }
                catch (Exception ex)
                {
                    //TODO: implementar Logs
                    Console.Write("Roteiro - " + sCaminhoDoArquivo + " Erro: " + ex.Message);
                    throw new Exception("Erro - ImportTrilhasByLinq()");
                }
            }
            tExecucao = DateTime.Now.Subtract(tInicioExecucao);
            Console.WriteLine("Numero de Nós: " + nNodes.ToString());
            Console.WriteLine("Tempo Execução:" + tExecucao.Milliseconds.ToString());

            return importadas;
        }

        public List<TrilhaImportada> ImportByLinq(XElement xml)
        {
            Console.WriteLine("Lendo XML by Linq" + Environment.NewLine);
            int nNodes = 0;

            DateTime tInicioExecucao = DateTime.Now;

            TimeSpan tExecucao = new TimeSpan();


            DateTime dataRoteiro = DateTime.Parse(root.FirstAttribute.Value);

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
                                           Interpretes = trilha.Element("INTERPRETE").Value,
                                           ISRC = trilha.Element("ISRC").Value,
                                           VetrixID = trilha.Element("CODIGO_VETRIX").Value
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
                                                      Interpretes = trilha.Interpretes,
                                                      ISRC = trilha.ISRC,
                                                      Vetrix = int.Parse(trilha.VetrixID)
                                                  };

                        importadas.Add(trilhaImportada);
                        nNodes++;
                    }
                }
                catch (Exception ex)
                {
                    //TODO: implementar Logs
                    Console.Write("Roteiro - " + sCaminhoDoArquivo + " Erro: " + ex.Message);
                    throw new Exception("Erro - ImportTrilhasByLinq()");
                }
            }
            tExecucao = DateTime.Now.Subtract(tInicioExecucao);
            Console.WriteLine("Numero de Nós: " + nNodes.ToString());
            Console.WriteLine("Tempo Execução:" + tExecucao.Milliseconds.ToString());

            return importadas;
        }
    }
}
