using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.MigrationDatabase
{
    internal class MigraSincronizacao : IMigracao
    {
        private int _codigoExibicaoTemp;
        private int _programaIDTemp;
        private bool _preAprovado;
        private bool _aprovado;
        private int _tipoExibicao;
        private int _usuarioID;
        private int _sincAtual;
        private long _tempCriaExibicao;
        private long _tempCriaMusica;
        private long _tempCriaSincronizacao;
        private long _tempObterMusica;
        private long _tempMeio;

        public void Migrar()
        {
            using (Repositorio repositorio = new Repositorio())
            {
                _usuarioID =
                    repositorio.Obter<Usuario>(u => u.Login == "SISTEMA").Select(u => u.UsuarioID).SingleOrDefault();

                var importacao = new Importacao
                                     {
                                         Arquivo = "BASE ANTIGA",
                                         ImportadoEm = DateTime.Now,
                                         Processado = true,
                                         UsuarioID = _usuarioID,
                                         ImportadoVetrix = true
                                     };
                repositorio.Adicionar(importacao);
            }

            Transferencia.DicSincronizacao = new Dictionary<int, int>();

            using (var oldDB = new SincOldEntities())
            {
                var oldSonorizacoes = (from s in oldDB.SINCTMUSI
                                       //orderby s.SIQD_CD_CODIGO descending
                                       orderby s.SIQD_CD_CODIGO
                                       select s).ToList();

                int countLoop = 0;
                int numSon = 0;

                Stopwatch time = new Stopwatch();
                foreach (var itemSincronizacao in oldSonorizacoes)
                {
                    using (Repositorio repositorio = new Repositorio())
                    {
                        countLoop++;

                        _sincAtual = 0;

                        time.Start();

                        _preAprovado = false;
                        _aprovado = false;

                        var son = new Sonorizacao();
                        int clasID = Transferencia.DicClassificacao[itemSincronizacao.CLAS_CD_CODIGO];

                        son.ClassificacaoID = clasID;

                        son.Minutagem = MigraMusica.ConverterTempo("00:" + itemSincronizacao.MUSI_HR_MINUTAGEM);

                        int musID = ObterMusica(itemSincronizacao);

                        if (musID == 0)
                        {
                            continue;
                        }

                        son.MusicaID = musID;

                        Sincronizacao sinc = CriarSincronizacao(itemSincronizacao.SIQD_CD_CODIGO);

                        _tempMeio = time.ElapsedMilliseconds;

                        if (sinc == null)
                        {
                            continue;
                        }

                        if (itemSincronizacao.DATA_CADASTRO.HasValue)
                        {
                            sinc.AprovadoEm = itemSincronizacao.DATA_CADASTRO.Value;
                            sinc.PreAprovado = true;
                            sinc.Aprovado = true;

                            repositorio.Editar(sinc);
                        }

                        if (_aprovado)
                            sinc.Aberto = false;
                        if (sinc.ExibicaoID > 0)
                        {
                            if (_programaIDTemp != 0)
                            {
                                son.SincronizacaoID = sinc.SincronizacaoID;
                                son.TipoExibicaoID = _tipoExibicao;
                                son.QuadroID = 1;
                                son.ImportacaoID = 1;

                                repositorio.Adicionar(son);

                                if (!Transferencia.DicExibicao.ContainsKey(_codigoExibicaoTemp) && _codigoExibicaoTemp != 0)
                                    Transferencia.DicExibicao.Add(_codigoExibicaoTemp, sinc.ExibicaoID);
                                if (!Transferencia.DicSincronizacao.ContainsKey(_sincAtual) && _sincAtual != 0)
                                    Transferencia.DicSincronizacao.Add(_sincAtual, son.SincronizacaoID);
                                if (!Transferencia.DicSonorizacao.ContainsKey(itemSincronizacao.MUSI_CD_CODIGO))
                                    Transferencia.DicSonorizacao.Add(itemSincronizacao.MUSI_CD_CODIGO, son.SonorizacaoID);

                                _codigoExibicaoTemp = 0;

                                Console.Write('.');
                                numSon++;
                            }
                        }

                        time.Stop();

                        if (countLoop%100 == 0)
                        {
                            Console.Write(Environment.NewLine + "Sincronizações: " + Transferencia.DicSincronizacao.Count +
                                          " - Sonorizações: " + numSon + " -ms: " + time.ElapsedMilliseconds +
                                          Environment.NewLine);
                            Console.Write(
                                "ObterMusica-ms: " + _tempObterMusica + Environment.NewLine +
                                "Criar Musica-ms: " + _tempCriaMusica + Environment.NewLine +
                                "Exibicao-ms: " + _tempCriaExibicao + Environment.NewLine +
                                "Sincronização-ms: " + _tempCriaSincronizacao + Environment.NewLine +
                                "Mediano-ms: " + _tempMeio + Environment.NewLine +
                                Environment.NewLine
                                );

                            countLoop = 0;
                            time.Reset();
                        }
                    }
                    //if (numSon > 500)
                    //{
                    //    return;
                    //}
                }
            }
        }

        private int ObterMusica(SINCTMUSI itemSincronizacao)
        {
            int musicaID = 0;
            Stopwatch time = new Stopwatch();
            using (Repositorio repositorio = new Repositorio())
            {
                MigraMusica migraMusica = new MigraMusica();
                time.Start();
                //musica biblioteca musical
                if (itemSincronizacao.TRIB_CD_CODIGO.HasValue)
                {
                    if (!Transferencia.DicMusicaBranca.ContainsKey(itemSincronizacao.TRIB_CD_CODIGO.Value))
                    {
                        musicaID = migraMusica.CriarMusica(itemSincronizacao.TRIB_CD_CODIGO.Value, 1);
                    }
                    else
                    {
                        musicaID = Transferencia.DicMusicaBranca[itemSincronizacao.TRIB_CD_CODIGO.Value];
                    }
                }
                else
                    //musica comercial
                    if (itemSincronizacao.ISRC_CD_CODIGO.HasValue)
                    {
                        if (!Transferencia.DicMusicaComercial.ContainsKey(itemSincronizacao.ISRC_CD_CODIGO.Value))
                        {
                            musicaID = migraMusica.CriarMusica(itemSincronizacao.ISRC_CD_CODIGO.Value, 2);
                        }
                        else
                        {
                            musicaID = Transferencia.DicMusicaComercial[itemSincronizacao.ISRC_CD_CODIGO.Value];
                        }
                    }
                    else
                        //musica Record
                        if (itemSincronizacao.TREC_CD_CODIGO.HasValue)
                        {
                            if (!Transferencia.DicMusicaRecord.ContainsKey(itemSincronizacao.TREC_CD_CODIGO.Value))
                            {
                                musicaID = migraMusica.CriarMusica(itemSincronizacao.TREC_CD_CODIGO.Value, 3);
                            }
                            else
                            {
                                musicaID = Transferencia.DicMusicaRecord[itemSincronizacao.TREC_CD_CODIGO.Value];
                            }
                        }
                time.Stop();
                migraMusica = null;
            }
            _tempObterMusica = time.ElapsedMilliseconds;
            return musicaID;
        }

        private Sincronizacao CriarSincronizacao(int codigoQuadro)
        {
            Sincronizacao sinc;

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                Stopwatch time = new Stopwatch();
                using (Repositorio repositorio = new Repositorio())
                {
                    time.Start();
                    var sincQuadro = (from s in oldDB.SINCTSIQD
                                      where s.SIQD_CD_CODIGO == codigoQuadro
                                      select new {s.SINC_CD_CODIGO, s.EXIB_CD_CODIGO}).SingleOrDefault();
                    if (sincQuadro == null)
                    {
                        return null;
                    }

                    _sincAtual = sincQuadro.SINC_CD_CODIGO;
                    _tipoExibicao = Transferencia.DicTipoExibicao[sincQuadro.EXIB_CD_CODIGO.Value];

                    if (Transferencia.DicSincronizacao.ContainsKey(_sincAtual))
                    {
                        int sincID = Transferencia.DicSincronizacao[_sincAtual];
                        sinc = repositorio.Obter<Sincronizacao>(s => s.SincronizacaoID == sincID).SingleOrDefault();
                    }
                    else
                    {
                        sinc = new Sincronizacao();
                        {
                            Exibicao exib = CriarExibicao(sincQuadro.SINC_CD_CODIGO);
                            if (exib == null) return null;
                            sinc.ExibicaoID = exib.ExibicaoID;
                        }

                        sinc.UsuarioID = _usuarioID;
                        sinc.PreAprovado = _preAprovado;
                        sinc.Aprovado = _aprovado;
                        sinc.AprovadoEm = new DateTime(2000, 1, 1, 0, 0, 0, 0);

                        repositorio.Adicionar(sinc);

                    }
                    time.Stop();
                }
                _tempCriaSincronizacao = time.ElapsedMilliseconds;

                return sinc;
            }
        }

        private Exibicao CriarExibicao(int codigoSinc)
        {
            Exibicao exib;
            _programaIDTemp = 0;

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                Stopwatch time = new Stopwatch();
                using (Repositorio repositorio = new Repositorio())
                {
                    time.Start();
                    //se existir exibicao recupera, se nao cadastra
                    if (Transferencia.DicExibicao.ContainsKey(codigoSinc))
                    {
                        int exibID = Transferencia.DicExibicao[codigoSinc];
                        exib = repositorio.Obter<Exibicao>(e => e.ExibicaoID == exibID).SingleOrDefault();
                        if (exib != null) _programaIDTemp = exib.ProgramaID;
                    }
                    else
                    {
                        var sinc = (from s in oldDB.SINCTSINC
                                    where s.SINC_CD_CODIGO == codigoSinc
                                    select s).SingleOrDefault();
                        Debug.Assert(sinc != null, "sinc != null");
                        Debug.Assert(sinc.SINC_DT_EXIBICAO != null, "sinc.SINC_DT_EXIBICAO != null");
                        exib = new Exibicao {Data = sinc.SINC_DT_EXIBICAO.Value};
                        if (sinc.SINC_DT_CRIACAO != null)
                        {
                            _preAprovado = true;
                        }
                        if (sinc.SINC_DT_AP_ADMIN != null)
                        {
                            _aprovado = true;
                        }
                        if (Transferencia.DicPrograma.ContainsKey(sinc.PROD_CD_CODIGO))
                        {
                            int progID = Transferencia.DicPrograma[sinc.PROD_CD_CODIGO];
                            exib.ProgramaID = progID;
                            int unidID = Transferencia.DicUnidade[sinc.UNID_CD_CODIGO];
                            exib.UnidadeID = unidID;
                            _codigoExibicaoTemp = codigoSinc;
                            _programaIDTemp = progID;
                        }
                        else
                        {
                            return null;
                        }

                        repositorio.Adicionar(exib);

                        Transferencia.DicExibicao.Add(codigoSinc, exib.ExibicaoID);
                    }
                    time.Stop();
                }
                _tempCriaExibicao = time.ElapsedMilliseconds;
            }

            return exib;
        }
    }
}