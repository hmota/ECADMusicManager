using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.IO;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.Domain.Business
{
    public class ImportacaoMusicas
    {
        private Context context = SingletonContext.Instance.Context;
        private Importacao _importacao;
        private Usuario _user = new VerificarUsuario().ObterUsuario(ConfigurationManager.AppSettings["Login"]);
        private Sincronizacao _lastSinc;
        private DateTime _dataRoteiro;
        private int _totalErros;
        private List<RowVetrixErro> _vetrixes = new List<RowVetrixErro>();
        private List<RowVetrixErroNovela> _vetrixesNovelas = new List<RowVetrixErroNovela>();
        private bool _importacaoVetrix = false;
        private List<Classificacao> _classificacoes;
        private List<Quadro> _quadros;
        private List<TipoExibicao> _tipoExibicoes;

        public ImportacaoMusicas()
        {
            using (Repositorio repositorio = new Repositorio())
            {
                _classificacoes = repositorio.Obter<Classificacao>().ToList();
                _quadros = repositorio.Obter<Quadro>().ToList();
                _tipoExibicoes = repositorio.Obter<TipoExibicao>().ToList();
            }
        }
        /// <summary>
        /// Verifica se o arquivo foi importado anteriormente
        /// </summary>
        /// <returns></returns>
        public bool ArquivoImportado(string nomeArquivo)
        {
            var existe = (from i in context.Importacoes
                          where i.Arquivo == nomeArquivo
                          select i).Any();

            return existe;
        }

        /// <summary>
        /// Gera Sincronizacao pela lista de musicas recebida
        /// </summary>
        /// <param name="importadas"></param>
        /// <param name="dataRoteiro"></param>
        /// <param name="nomeArquivo"></param>
        /// <param name="totalMusicas"></param>
        /// <param name="totalImportadas"></param>
        /// <param name="totalErros"></param>
        /// <param name="importacaoVetrix"></param>
        /// <returns></returns>
        public List<RowVetrixErro> Importar(List<TrilhaImportada> importadas, DateTime dataRoteiro, string nomeArquivo,
                                         out int totalMusicas, out int totalImportadas, out int totalErros,
                                         bool importacaoVetrix)
        {
            ConfigurationManager.AppSettings["Login"] = Environment.UserName;
            _user = new VerificarUsuario().ObterUsuario(ConfigurationManager.AppSettings["Login"]);
            _importacaoVetrix = importacaoVetrix;
            totalMusicas = 0;
            totalImportadas = 0;
            _totalErros = totalErros = 0;
            _dataRoteiro = dataRoteiro;
            //Verifica se a data da vetrix para importacao nao é uma data futura
            if (dataRoteiro > DateTime.Now)
                throw new Exception("A data do roteiro é uma data futura. Não pode ser importada.");

            var arquivo = Path.GetFileName(nomeArquivo);
            //TODO: Obter Usuario importacao filiais
            //context.SaveChanges();
            _importacao = new Importacao
                              {
                                  Arquivo = arquivo,
                                  ImportadoEm = DateTime.Parse(DateTime.Now.ToShortDateString()),
                                  Processado = true,
                                  UsuarioID = _user.UsuarioID,
                                  ImportadoVetrix = importacaoVetrix
                              };

            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(_importacao);
            }
            //TODO: Verificar se a Sincronizacao já foi aprovada

            try
            {
                totalMusicas = importadas.Count();

                foreach (var trilha in importadas)
                {
                    if (!string.IsNullOrWhiteSpace(trilha.TituloNacional))
                    {
                        var firstOrDefault =
                            context.Novela.AsNoTracking().FirstOrDefault(n => n.TituloNacional == trilha.TituloNacional);
                        if (firstOrDefault != null)
                            trilha.Programa = firstOrDefault.Programa.Nome;
                    }

                    Sincronizacao sinc = GerarSincronizacao(trilha);
                    if (sinc == null)
                    {
                        continue;
                    }
                    //importando musicas da Sincronizacao
                    var sonorizacao = GerarSonorizacao(dataRoteiro, sinc, trilha, _vetrixes);

                    //TODO: Importar todos os itens;
                    //TODO: Verificar se musica de importacao existe no banco
                    if (sonorizacao == null)
                        continue;

                    totalImportadas++;
                }
            }
            catch (Exception ex)
            {
                //TODO: implementar log importacao vetrix
                throw new Exception("Erro - Importar(): " + ex.Message);
            }
            totalErros = _totalErros;
            return _vetrixes;
        }

        /// <summary>
        /// Gera Sincronizacao pela lista de musicas recebida
        /// </summary>
        /// <param name="importadas"></param>
        /// <param name="dataRoteiro"></param>
        /// <param name="nomeArquivo"></param>
        /// <param name="totalMusicas"></param>
        /// <param name="totalImportadas"></param>
        /// <param name="totalErros"></param>
        /// <param name="importacaoVetrix"></param>
        /// <returns></returns>
        public List<RowVetrixErroNovela> ImportarNovelas(List<TrilhaImportada> importadas, DateTime dataRoteiro, string nomeArquivo,
                                         out int totalMusicas, out int totalImportadas, out int totalErros,
                                         bool importacaoVetrix)
        {
            ConfigurationManager.AppSettings["Login"] = Environment.UserName;
            _user = new VerificarUsuario().ObterUsuario(ConfigurationManager.AppSettings["Login"]);
            _importacaoVetrix = importacaoVetrix;
            totalMusicas = 0;
            totalImportadas = 0;
            _totalErros = totalErros = 0;
            _dataRoteiro = dataRoteiro;
            //Verifica se a data da vetrix para importacao nao é uma data futura
            if (dataRoteiro > DateTime.Now)
                throw new Exception("A data do roteiro é uma data futura. Não pode ser importada.");

            var arquivo = Path.GetFileName(nomeArquivo);
            //TODO: Obter Usuario importacao filiais
            //context.SaveChanges();
            _importacao = new Importacao
            {
                Arquivo = arquivo,
                ImportadoEm = DateTime.Parse(DateTime.Now.ToShortDateString()),
                Processado = true,
                UsuarioID = _user.UsuarioID,
                ImportadoVetrix = importacaoVetrix
            };

            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(_importacao);
            }
            //TODO: Verificar se a Sincronizacao já foi aprovada

            try
            {
                totalMusicas = importadas.Count();

                foreach (var trilha in importadas)
                {
                    if (!string.IsNullOrWhiteSpace(trilha.TituloNacional))
                    {
                        var firstOrDefault =
                            context.Novela.AsNoTracking().FirstOrDefault(n => n.TituloNacional == trilha.TituloNacional);
                        if (firstOrDefault != null)
                            trilha.Programa = firstOrDefault.Programa.Nome;
                    }

                    Sincronizacao sinc = GerarSincronizacao(trilha, _vetrixesNovelas);
                    if (sinc == null)
                    {
                        continue;
                    }
                    //importando musicas da Sincronizacao
                    var sonorizacao = GerarSonorizacao(dataRoteiro, sinc, trilha, _vetrixesNovelas);

                    //TODO: Importar todos os itens;
                    //TODO: Verificar se musica de importacao existe no banco
                    if (sonorizacao == null)
                        continue;

                    totalImportadas++;
                }
            }
            catch (Exception ex)
            {
                //TODO: implementar log importacao vetrix
                throw new Exception("Erro - Importar(): " + ex.Message);
            }
            totalErros = _totalErros;
            return _vetrixesNovelas;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataRoteiro"></param>
        /// <param name="sinc"></param>
        /// <param name="trilha"></param>
        /// <param name="listaVetrix"></param>
        /// <returns></returns>
        private Sonorizacao GerarSonorizacao(DateTime dataRoteiro, Sincronizacao sinc,
                                             TrilhaImportada trilha, List<RowVetrixErro> listaVetrix)
        {
            Sonorizacao sonorizacao;


            sonorizacao = new Sonorizacao
                              {
                                  SincronizacaoID = sinc.SincronizacaoID
                              };

            sonorizacao.ClassificacaoID = !string.IsNullOrWhiteSpace(trilha.Classificacao)
                                              ? _classificacoes.Where(
                                                  c => c.Descricao.Contains(trilha.Classificacao))
                                                               .Select(c => c.ClassificacaoID)
                                                               .First()
                                              : _classificacoes.Where(
                                                  c => c.Descricao.Contains("INDEFINIDO"))
                                                               .Select(c => c.ClassificacaoID)
                                                               .First();

            sonorizacao.TipoExibicaoID = !string.IsNullOrWhiteSpace(trilha.TipoExibicao)
                                             ? _tipoExibicoes.Where(
                                                 te => te.Descricao.Contains(trilha.TipoExibicao))
                                                             .Select(te => te.TipoExibicaoID)
                                                             .First()
                                             : _tipoExibicoes.Where(
                                                 te => te.Descricao.Contains("INDEFINIDO"))
                                                             .Select(te => te.TipoExibicaoID)
                                                             .First();

            sonorizacao.QuadroID = !string.IsNullOrWhiteSpace(trilha.Quadro)
                                       ? _quadros.Where(
                                           q => q.Descricao.Contains(trilha.Quadro))
                                                 .Select(q => q.QuadroID)
                                                 .First()
                                       : _quadros.Where(
                                           q => q.Descricao.Contains("GERAL"))
                                                 .Select(q => q.QuadroID)
                                                 .First();

            sonorizacao.Captacao = trilha.ExibidoEm.TimeOfDay;

            Musica musica = null;

            musica = _importacaoVetrix ? BuscarMusicaVetrix(trilha) : BuscarMusicaPlanilha(trilha);


            if (musica != null)
                sonorizacao.Musica = musica;
            else
            {
                listaVetrix.Add(
                    new RowVetrixErro
                        {
                            Exibicao = trilha.ExibidoEm,
                            ImportadoEm = DateTime.Now,
                            Processado =
                                "Musica " + trilha.Musica + " não encontrada na base de dados.",
                            Programa = trilha.Programa,
                            Roteiro = dataRoteiro,
                            Unidade = trilha.Unidade,
                            VetrixID = trilha.Vetrix,
                            Autores = trilha.Autores,
                            Classificacao = trilha.Classificacao,
                            Minutagem = trilha.Minutagem,
                            Quadro = trilha.Quadro,
                            Interpretes = trilha.Interpretes,
                            Nome_Da_Musica = trilha.Musica,
                            Tipo_De_Exibicao = trilha.TipoExibicao
                        });
                _totalErros++;
                return null;
            }

            sonorizacao.Minutagem = trilha.Minutagem;

            sonorizacao.ImportacaoID = _importacao.ImportacaoID;
            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(sonorizacao);
            }

            return sonorizacao;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataRoteiro"></param>
        /// <param name="sinc"></param>
        /// <param name="trilha"></param>
        /// <param name="listaVetrix"></param>
        /// <returns></returns>
        private Sonorizacao GerarSonorizacao(DateTime dataRoteiro, Sincronizacao sinc,
                                             TrilhaImportada trilha, List<RowVetrixErroNovela> listaVetrix)
        {
            Sonorizacao sonorizacao;

            using (Repositorio repositorio = new Repositorio())
            {
                sonorizacao = new Sonorizacao
                {
                    SincronizacaoID = sinc.SincronizacaoID
                };

                sonorizacao.ClassificacaoID = !string.IsNullOrWhiteSpace(trilha.Classificacao)
                                                  ? repositorio.Obter<Classificacao>(
                                                      c => c.Descricao.Contains(trilha.Classificacao))
                                                               .Select(c => c.ClassificacaoID)
                                                               .First()
                                                  : repositorio.Obter<Classificacao>(
                                                      c => c.Descricao.Contains("INDEFINIDO"))
                                                               .Select(c => c.ClassificacaoID)
                                                               .First();

                sonorizacao.TipoExibicaoID = !string.IsNullOrWhiteSpace(trilha.TipoExibicao)
                                                 ? repositorio.Obter<TipoExibicao>(
                                                     te => te.Descricao.Contains(trilha.TipoExibicao))
                                                              .Select(te => te.TipoExibicaoID)
                                                              .First()
                                                 : repositorio.Obter<TipoExibicao>(
                                                     te => te.Descricao.Contains("INDEFINIDO"))
                                                              .Select(te => te.TipoExibicaoID)
                                                              .First();

                sonorizacao.QuadroID = !string.IsNullOrWhiteSpace(trilha.Quadro)
                                           ? repositorio.Obter<Quadro>(
                                               q => q.Descricao.Contains(trilha.Quadro))
                                                        .Select(q => q.QuadroID)
                                                        .First()
                                           : repositorio.Obter<Quadro>(q => q.Descricao.Contains("GERAL"))
                                                        .Select(q => q.QuadroID)
                                                        .First();

                sonorizacao.Captacao = trilha.ExibidoEm.TimeOfDay;

                Musica musica = null;

                musica = _importacaoVetrix ? BuscarMusicaVetrix(trilha) : BuscarMusicaPlanilha(trilha);


                if (musica != null)
                    sonorizacao.Musica = musica;
                else
                {
                    listaVetrix.Add(
                        new RowVetrixErroNovela()
                        {
                            Exibicao = trilha.ExibidoEm,
                            ImportadoEm = DateTime.Now,
                            Processado =
                                "Musica " + trilha.Musica + " não encontrada na base de dados.",
                            Titulo_Nacional = trilha.TituloNacional,
                            Roteiro = dataRoteiro,
                            Unidade = trilha.Unidade,
                            Capitulo = trilha.Capitulo,
                            Produtor = trilha.Produtor,
                            Diretor = trilha.Diretor,
                            Categoria = trilha.Categoria,
                            Destinacao = trilha.Destinacao,
                            Duracao = trilha.Duracao,
                            Ordem = trilha.Ordem,
                            Titulo = trilha.Musica,
                            Segundos = trilha.Segundos,
                            Caracteristicas = trilha.Caracteristicas,
                            Interpretes = trilha.Interpretes,
                            Editora = trilha.Editora,
                            Gravadora = trilha.Gravadora,
                            Autores = trilha.Autores
                        });
                    _totalErros++;
                    return null;
                }

                sonorizacao.Minutagem = trilha.Minutagem;

                sonorizacao.ImportacaoID = _importacao.ImportacaoID;

                repositorio.Adicionar(sonorizacao);
            }

            return sonorizacao;
        }

        private Musica BuscarMusicaPlanilha(TrilhaImportada trilhaImportada)
        {
            var trilha = trilhaImportada;

            IQueryable<Musica> query;

            var nomeMusica = trilha.Musica.Normalizar();
            var nomeInterprete = trilha.Interpretes.Normalizar();
            var nomeAutor = trilha.Autores.Normalizar();

            //TODO: Verifica duplicidade com dtExibicao, codVetrix e Exibidora
            using (Context repositorio = new Context())
            {
                query = repositorio.Musicas
                                   .Include("Album")
                                   .Include("Autor")
                                   .Include("Interprete")
                                   .AsNoTracking()
                                   .Where(m => m.Titulo == nomeMusica);
                //encontrou titulo
                if (query.Any())
                {
                    var count = 0;
                    foreach (Musica musica in query)
                    {
                        try
                        {
                            #region autor no xml

                            //existe autor no xml
                            if (!string.IsNullOrWhiteSpace(nomeAutor) && musica.Autor != null)
                            {
                                //autor encontrado
                                if (musica.Autor.Nome == nomeAutor)
                                {
                                    #region interprete no xml

                                    //existe interprete no xml
                                    if (!string.IsNullOrWhiteSpace(nomeInterprete) & musica.Interprete != null)
                                    {
                                        //interprete encontrado
                                        if (musica.Interprete.Nome == nomeInterprete)
                                        {
                                            return musica;
                                        }
                                            //interprete nao encontrado
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                        //nao existe interprete no xml
                                    else
                                    {
                                        return musica;
                                    }

                                    #endregion interprete
                                }
                                    //autor nao encontrado
                                else
                                {
                                    #region interprete no xml

                                    //existe interprete no xml
                                    if (!string.IsNullOrWhiteSpace(nomeInterprete))
                                    {
                                        //interprete encontrado
                                        if (musica.Interprete.Nome == nomeInterprete)
                                        {
                                            return musica;
                                        }
                                            //interprete nao encontrado
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                        //nao existe interprete no xml
                                    else
                                    {
                                        return musica;
                                    }

                                    #endregion interprete
                                }

                            }
                                //nao existe autor no xml
                            else
                            {
                                #region interprete no xml

                                //existe interprete no xml
                                if (!string.IsNullOrWhiteSpace(nomeInterprete))
                                {
                                    //interprete encontrado
                                    if (musica.Interprete.Nome == nomeInterprete)
                                    {
                                        return musica;
                                    }
                                        //interprete nao encontrado
                                    else
                                    {
                                        continue;
                                    }
                                }
                                    //nao existe interprete no xml
                                else
                                {
                                    return musica;
                                }

                                #endregion interprete
                            }

                            #endregion autor no xml
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Problema ao procurar musica: " + musica.Titulo, ex);
                        }
                    }
                    //se for o ultimo item da lista, retorna o ultimo
                    return query.FirstOrDefault();
                }
            }
            //nao encontrou titulo nem ISRC
            return null;
        }

        private Musica BuscarMusicaVetrix(TrilhaImportada trilhaImportada)
        {
            var trilha = trilhaImportada;
            //var repositorio = contexto;

            IQueryable<Musica> query;
            var isrc = trilha.ISRC;
            var nomeMusica = trilha.Musica.Normalizar();
            var nomeInterprete = trilha.Interpretes.Normalizar();
            var nomeAutor = trilha.Autores.Normalizar();
            var codAlbum = trilha.CodAlbum;
            var nomeAlbum = trilha.Album.Normalizar();

            //TODO: Verifica duplicidade com dtExibicao, codVetrix e Exibidora
            using (Context repositorio = new Context())
            {

                if (!string.IsNullOrWhiteSpace(isrc))
                {
                    query = repositorio.Musicas
                                       .Where(m => m.ISRC == trilha.ISRC);
                    //encontrou ISRC
                    if (query.Any() && query.Count() == 1)
                    {
                        return query.Single();
                    }
                }
                //nao encontrou ISRC
                query = repositorio.Musicas
                                   .Include("Album")
                                   .Include("Autor")
                                   .Include("Interprete")
                                   .AsNoTracking()
                                   .Where(m => m.Titulo == nomeMusica);
                //encontrou titulo
                if (query.Any())
                {
                    var count = 0;
                    foreach (Musica musica in query)
                    {
                        try
                        {
                            //Biblioteca Musical
                            if (musica.TipoTrilhaID == 1)
                            {
                                //existe codigo do album no xml
                                if (!string.IsNullOrWhiteSpace(codAlbum) && musica.Album != null)
                                {
                                    //encontrou codigo do album
                                    if (musica.Album.CodigoAlbum == codAlbum)
                                    {
                                        return musica;
                                    }
                                        //nao encontrou codigo do album
                                    else
                                    {
                                        #region nome do album no xml

                                        //existe nome do album no xml
                                        if (!string.IsNullOrWhiteSpace(nomeAlbum))
                                        {
                                            //encontrou codigo do album
                                            if (musica.Album.NomeAlbum == nomeAlbum)
                                            {
                                                return musica;
                                            }
                                                //nao encontrou codigo do album
                                            else
                                            {
                                                #region autor no xml

                                                //existe autor no xml
                                                if (!string.IsNullOrWhiteSpace(nomeAutor) & musica.Autor != null)
                                                {
                                                    //encontrou codigo do album
                                                    if (musica.Autor.Nome == nomeAutor)
                                                    {
                                                        return musica;
                                                    }
                                                        //nao encontrou codigo do album
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                    //nao existe autor no xml
                                                else
                                                {
                                                    continue;
                                                }

                                                #endregion autor no xml
                                            }
                                        }
                                            //nao existe nome do album no xml
                                        else
                                        {
                                            #region autor no xml

                                            //existe autor no xml
                                            if (!string.IsNullOrWhiteSpace(nomeAutor) & musica.Autor != null)
                                            {
                                                //encontrou codigo do album
                                                if (musica.Autor.Nome == nomeAutor)
                                                {
                                                    return musica;
                                                }
                                                    //nao encontrou codigo do album
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                                //nao existe autor no xml
                                            else
                                            {
                                                continue;
                                            }

                                            #endregion autor no xml
                                        }

                                        #endregion nome do album no xml
                                    }
                                }
                                    //nao existe codigo do album no xml
                                else
                                {
                                    #region nome do album no xml

                                    //existe nome do album no xml
                                    if (!string.IsNullOrWhiteSpace(nomeAlbum) & musica.Album != null)
                                    {
                                        //encontrou codigo do album
                                        if (musica.Album.NomeAlbum == nomeAlbum)
                                        {
                                            return musica;
                                        }
                                            //nao encontrou codigo do album
                                        else
                                        {
                                            #region autor no xml

                                            //existe autor no xml
                                            if (!string.IsNullOrWhiteSpace(nomeAutor) & musica.Autor != null)
                                            {
                                                //encontrou codigo do album
                                                if (musica.Autor.Nome == nomeAutor)
                                                {
                                                    return musica;
                                                }
                                                    //nao encontrou codigo do album
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                                //nao existe autor no xml
                                            else
                                            {
                                                continue;
                                            }

                                            #endregion autor no xml
                                        }
                                    }
                                        //nao existe nome do album no xml
                                    else
                                    {
                                        #region autor no xml

                                        //existe autor no xml
                                        if (!string.IsNullOrWhiteSpace(nomeAutor) & musica.Autor != null)
                                        {
                                            //encontrou codigo do album
                                            if (musica.Autor.Nome == nomeAutor)
                                            {
                                                return musica;
                                            }
                                                //nao encontrou codigo do album
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                            //nao existe autor no xml
                                        else
                                        {
                                            continue;
                                        }

                                        #endregion autor no xml
                                    }

                                    #endregion nome do album no xml
                                }
                            }
                                //comercial/record
                            else
                            {
                                #region autor no xml

                                //existe autor no xml
                                if (!string.IsNullOrWhiteSpace(nomeAutor) && musica.Autor != null)
                                {
                                    //autor encontrado
                                    if (musica.Autor.Nome == nomeAutor)
                                    {
                                        #region interprete no xml

                                        //existe interprete no xml
                                        if (!string.IsNullOrWhiteSpace(nomeInterprete) & musica.Interprete != null)
                                        {
                                            //interprete encontrado
                                            if (musica.Interprete.Nome == nomeInterprete)
                                            {
                                                return musica;
                                            }
                                                //interprete nao encontrado
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                            //nao existe interprete no xml
                                        else
                                        {
                                            return musica;
                                        }

                                        #endregion interprete
                                    }
                                        //autor nao encontrado
                                    else
                                    {
                                        #region interprete no xml

                                        //existe interprete no xml
                                        if (!string.IsNullOrWhiteSpace(nomeInterprete))
                                        {
                                            //interprete encontrado
                                            if (musica.Interprete.Nome == nomeInterprete)
                                            {
                                                return musica;
                                            }
                                                //interprete nao encontrado
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                            //nao existe interprete no xml
                                        else
                                        {
                                            return musica;
                                        }

                                        #endregion interprete
                                    }

                                }
                                    //nao existe autor no xml
                                else
                                {
                                    #region interprete no xml

                                    //existe interprete no xml
                                    if (!string.IsNullOrWhiteSpace(nomeInterprete))
                                    {
                                        //interprete encontrado
                                        if (musica.Interprete.Nome == nomeInterprete)
                                        {
                                            return musica;
                                        }
                                            //interprete nao encontrado
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                        //nao existe interprete no xml
                                    else
                                    {
                                        return musica;
                                    }

                                    #endregion interprete
                                }

                                #endregion autor no xml
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Problema ao procurar musica: " + musica.Titulo, ex);
                        }
                    }
                    //se for o ultimo item da lista, retorna o ultimo
                    return query.FirstOrDefault();
                }
            }
            //nao encontrou titulo nem ISRC
            return null;
        }


        /// <summary>
        /// Gera Sincronizacao a partir de dados da trilha
        /// </summary>
        /// <param name="trilha"></param>
        /// <returns></returns>
        private Sincronizacao GerarSincronizacao(TrilhaImportada trilha)
        {
            Sincronizacao sinc;

            using (Repositorio repositorio = new Repositorio())
            {
                try
                {
                    if (_lastSinc == null)
                    {
                        AddExibicaoToSincronizacao(out sinc, trilha, repositorio);
                    }
                    else
                    {
                        _lastSinc =
                            repositorio.Obter<Sincronizacao>(s => s.SincronizacaoID == _lastSinc.SincronizacaoID)
                                       .SingleOrDefault();

                        if (_lastSinc != null &&
                            (_lastSinc.Exibicao.Data.ToShortDateString() != trilha.ExibidoEm.ToShortDateString()
                             || _lastSinc.Exibicao.Programa.Nome != trilha.Programa
                             || _lastSinc.Exibicao.Unidade.Nome != trilha.Unidade))
                        {
                            AddExibicaoToSincronizacao(out sinc, trilha, repositorio);
                            if (sinc.Exibicao.ProgramaID == 0)
                            {
                                return sinc;
                            }
                        }
                        else
                        {
                            sinc = _lastSinc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var vetrix = new RowVetrixErro
                                     {
                                         Autores = trilha.Autores,
                                         Classificacao = trilha.Classificacao,
                                         Exibicao = trilha.ExibidoEm,
                                         ImportadoEm = DateTime.Now,
                                         Interpretes = trilha.Interpretes,
                                         Minutagem = trilha.Minutagem,
                                         Nome_Da_Musica = trilha.Musica,
                                         Processado =
                                             "Musica " + trilha.Musica + " não importada. Erro: " + ex.Message,
                                         Programa = trilha.Programa,
                                         Quadro = trilha.Quadro,
                                         Tipo_De_Exibicao = trilha.TipoExibicao,
                                         Unidade = trilha.Unidade,
                                         VetrixID = trilha.Vetrix
                                     };
                    _vetrixes.Add(vetrix);
                    _totalErros++;
                    return null;
                }
            }
            return sinc;
        }

        /// <summary>
        /// Gera Sincronizacao a partir de dados da trilha
        /// </summary>
        /// <param name="trilha"></param>
        /// <returns></returns>
        private Sincronizacao GerarSincronizacao(TrilhaImportada trilha, List<RowVetrixErroNovela> vetrixesNovelas)
        {
            Sincronizacao sinc;

            using (Repositorio repositorio = new Repositorio())
            {
                try
                {
                    if (_lastSinc == null)
                    {
                        AddExibicaoToSincronizacao(out sinc, trilha, repositorio);
                    }
                    else
                    {
                        _lastSinc =
                            repositorio.Obter<Sincronizacao>(s => s.SincronizacaoID == _lastSinc.SincronizacaoID)
                                       .SingleOrDefault();

                        if (_lastSinc != null &&
                            (_lastSinc.Exibicao.Data.ToShortDateString() != trilha.ExibidoEm.ToShortDateString()
                             || _lastSinc.Exibicao.Programa.Nome != trilha.Programa
                             || _lastSinc.Exibicao.Unidade.Nome != trilha.Unidade))
                        {
                            AddExibicaoToSincronizacao(out sinc, trilha, repositorio);
                            if (sinc.Exibicao.ProgramaID == 0)
                            {
                                return sinc;
                            }
                        }
                        else
                        {
                            sinc = _lastSinc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var vetrix = new RowVetrixErroNovela()
                    {
                        Autores = trilha.Autores,
                        Exibicao = trilha.ExibidoEm,
                        ImportadoEm = DateTime.Now,
                        Interpretes = trilha.Interpretes,
                        Segundos = trilha.Segundos,
                        //trilha.Minutagem = new TimeSpan(),// 0,0, trilha.Segundos),
                        Titulo = trilha.Musica,
                        Processado =
                            "Musica " + trilha.Musica + " não importada. Erro: " + ex.Message,
                        Titulo_Nacional=  trilha.TituloNacional,
                        Caracteristicas = trilha.Caracteristicas,
                        Unidade = trilha.Unidade
                    };
                    vetrixesNovelas.Add(vetrix);
                    _totalErros++;
                    return null;
                }
            }
            return sinc;
        }

        /// <summary>
        /// Adiciona trilha para a Sincronizacao
        /// </summary>
        /// <param name="sinc">Sincronizacao de referencia</param>
        /// <param name="trilha">trilha a ser adicionada</param>
        /// <param name="repositorio"></param>
        protected void AddExibicaoToSincronizacao(out Sincronizacao sinc, TrilhaImportada trilha,
                                                  Repositorio repositorio)
        {
            sinc = new Sincronizacao();

            Exibicao exib =
                repositorio.Obter<Exibicao>(
                    ex =>
                    ex.Programa.Nome == trilha.Programa && EntityFunctions.DiffDays(ex.Data, trilha.ExibidoEm) == 0)
                           .FirstOrDefault();

            if (exib != null && exib.ExibicaoID != 0)
            {
                //Preenchendo Sincronizacao
                var sincronizacao =
                    repositorio.Obter<Sincronizacao>(s => s.ExibicaoID == exib.ExibicaoID).FirstOrDefault();

                if (sincronizacao != null)
                {
                    sinc = sincronizacao;
                    if (sinc.Aprovado)
                    {
                        throw new Exception("Exibição teve sua sonorização aprovada.");
                    }
                }
            }
            else
            {
                exib = new Exibicao {Data = trilha.ExibidoEm};

                Programa prog = repositorio.Obter<Programa>(p => p.Nome == trilha.Programa).SingleOrDefault();

                //TODO: Importação: Verificar similaridade programa
                if (prog != null)
                {
                    exib.Programa = prog;
                }
                else
                {
                    var nome = !string.IsNullOrWhiteSpace(trilha.Programa) ? "Programa "+trilha.Programa : "Novela "+trilha.TituloNacional;
                    throw new Exception(nome + " não cadastrado na base de dados");
                }

                Unidade uni = repositorio.Obter<Unidade>(u => u.Nome == trilha.Unidade).SingleOrDefault();

                //TODO: Importação: Verificar similaridade Unidade
                if (uni != null)
                {
                    exib.Unidade = uni;
                }
                else
                {
                    throw new Exception("Unidade " + trilha.Unidade + " não cadastrada na base de dados");
                }

                repositorio.Adicionar(exib);

                sinc.ExibicaoID = exib.ExibicaoID;

                sinc.AprovadoEm = _importacao.ImportadoEm;

                sinc.UsuarioID = _user.UsuarioID;

                repositorio.Adicionar(sinc);
            }

            //atualiza ultima Sincronizacao
            if (sinc.Exibicao.ProgramaID != 0)
            {
                int sID = sinc.SincronizacaoID;
                _lastSinc = null;
                _lastSinc = repositorio.Obter<Sincronizacao>(s => s.SincronizacaoID == sID).SingleOrDefault();
            }
        }
    }
}