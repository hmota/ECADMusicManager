using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.MigrationDatabase
{
    public class MigraMusica : IMigracao
    {
        public void Migrar()
        {
            using (var oldDB = new SincOldEntities())
            {
                var oldMusicasRecord = (from s in oldDB.SINCTTREC
                                        where s.TREC_DS_TITULO != null && s.TREC_DS_TITULO != ""
                                              && s.TREC_DS_AUTOR != null && s.TREC_DS_AUTOR != ""
                                        select s).ToList();
                Console.Write(Environment.NewLine + "Total Musicas Record: " + oldMusicasRecord.Count() + Environment.NewLine);

                int countLoop = 0;
                int numSon = 0;

                var time = new Stopwatch();
                foreach (var itemSincronizacao in oldMusicasRecord)
                {
                    countLoop++;
                    time.Start();

                    ObterMusicaRecord(itemSincronizacao);

                    numSon++;

                    time.Stop();
                    Console.Write(".");

                    if (countLoop%100 == 0)
                    {
                        Console.Write(Environment.NewLine + "Musicas Record: " + numSon
                            + " - t: " + time.ElapsedMilliseconds + Environment.NewLine);

                        countLoop = 0;
                        time.Reset();
                    }
                }


                var oldMusicasBrancas = (from s in oldDB.SINCTTRIB
                                         where s.TRIB_DS_TITULO != null && s.TRIB_DS_TITULO != ""
                                               && s.TRIB_DS_AUTOR != null && s.TRIB_DS_AUTOR != ""
                                               && s.TRIB_CD_CD_TRILHA != null && s.TRIB_CD_CD_TRILHA != ""
                                               && s.TRIB_DS_CD_TITLE != null && s.TRIB_DS_CD_TITLE != ""
                                         select s).ToList();

                Console.Write(Environment.NewLine + "Total Musicas Brancas: " + oldMusicasBrancas.Count() + Environment.NewLine);

                countLoop = 0;
                numSon = 0;

                time = new Stopwatch();
                foreach (var itemSincronizacao in oldMusicasBrancas)
                {
                    countLoop++;
                    time.Start();

                    ObterMusicaBranca(itemSincronizacao);

                    numSon++;

                    time.Stop();
                    Console.Write(".");
                    if (countLoop%100 == 0)
                    {
                        Console.Write(Environment.NewLine + "Musicas Brancas: " + numSon
                            + " - t: " + time.ElapsedMilliseconds + Environment.NewLine);

                        countLoop = 0;
                        time.Reset();
                    }
                }

                var oldMusicasComerciais = (from s in oldDB.SINCTISRC
                                            where s.ISRC_DS_TITULO != null && s.ISRC_DS_TITULO != ""
                                                  && s.ISRC_NM_ARQUIVO != null && s.ISRC_NM_ARQUIVO != ""
                                            select s).ToList();

                Console.Write(Environment.NewLine + "Total Musicas Comerciais: " + oldMusicasComerciais.Count() + Environment.NewLine);

                countLoop = 0;
                numSon = 0;

                time = new Stopwatch();
                foreach (var itemSincronizacao in oldMusicasComerciais)
                {
                    countLoop++;
                    time.Start();

                    ObterMusicaComercial(itemSincronizacao);

                    numSon++;

                    time.Stop();
                    Console.Write(".");
                    if (countLoop%100 == 0)
                    {
                        Console.Write(Environment.NewLine + "Musicas Comerciais: " + numSon
                            + " - t: " + time.ElapsedMilliseconds + Environment.NewLine);

                        countLoop = 0;
                        time.Reset();
                    }
                }
            }
        }

        public int ObterMusicaBranca(SINCTTRIB itemSincronizacao)
        {
            //musica biblioteca musical
            int musicaID = 0;
            if (!Transferencia.DicMusicaBranca.ContainsKey(itemSincronizacao.TRIB_CD_CODIGO))
            {
                return CriarMusica(itemSincronizacao.TRIB_CD_CODIGO, 1);
            }
            return musicaID;
        }

        public int ObterMusicaComercial(SINCTISRC itemSincronizacao)
        {
            //musica Comercial
            int musicaID = 0;
            if (!Transferencia.DicMusicaComercial.ContainsKey(itemSincronizacao.ISRC_CD_CODIGO))
            {
                return CriarMusica(itemSincronizacao.ISRC_CD_CODIGO, 2);
            }
            return musicaID;
        }

        public int ObterMusicaRecord(SINCTTREC itemSincronizacao)
        {
            //musica Record
            int musicaID = 0;
            if (!Transferencia.DicMusicaRecord.ContainsKey(itemSincronizacao.TREC_CD_CODIGO))
            {
                return CriarMusica(itemSincronizacao.TREC_CD_CODIGO, 3);
            }
            return musicaID;
        }

        public int CriarAlbum(string codigoAlbum, string nomeAlbum, Repositorio repositorio)
        {
            if (Transferencia.DicAlbum.ContainsKey(codigoAlbum))
            {
                return Transferencia.DicAlbum[codigoAlbum];
            }

            var alb = new Album
                          {
                              CodigoAlbum = codigoAlbum, 
                              NomeAlbum = nomeAlbum.NormalizarTrim()
                          };
            repositorio.Adicionar(alb);
            Transferencia.DicAlbum.Add(codigoAlbum, alb.AlbumID);

            return alb.AlbumID;
        }

        public int CriarMusica(int musicaID, int tipoTrilhaID)
        {
            var mus = new Musica
                          {
                              TipoTrilhaID = tipoTrilhaID,
                              CadastradaEm = DateTime.Now
                          };

            //TODO: adicionar verificacao de musica existente antes de tentar criar

            using (var oldDB = new SincOldEntities())
            {
                var time = new Stopwatch();
                using (var repositorio = new Repositorio())
                {
                    time.Start();
                    switch (tipoTrilhaID)
                    {
                            //"Biblioteca Musical"
                        case 1:
                            {
                                var oldMus = (from m in oldDB.SINCTTRIB
                                              where m.TRIB_CD_CODIGO == musicaID
                                              select m).FirstOrDefault();
                                if (oldMus != null && !string.IsNullOrWhiteSpace(oldMus.TRIB_DS_AUTOR))
                                {
                                    mus.AutorID =
                                        new MigraAutorInterprete().CriarAutor(oldMus.TRIB_DS_AUTOR.NormalizarTrim());
                                    if (!string.IsNullOrWhiteSpace(oldMus.TRIB_DS_INTERPRETES))
                                    {
                                        mus.InterpreteID =
                                            new MigraAutorInterprete().CriarInterprete(
                                                oldMus.TRIB_DS_INTERPRETES.NormalizarTrim());
                                    }
                                    else
                                        mus.InterpreteID =
                                            new MigraAutorInterprete().CriarInterprete(
                                                oldMus.TRIB_DS_AUTOR.NormalizarTrim());

                                    mus.Duracao = String.IsNullOrEmpty(oldMus.TRIB_DS_TRACK_TIME)
                                                      ? new TimeSpan()
                                                      : ConverterTempo(oldMus.TRIB_DS_TRACK_TIME);
                                    mus.NomeArquivo = oldMus.TRIB_NM_ARQUIVO;

                                    mus.Titulo = oldMus.TRIB_DS_TITULO.NormalizarTrim();

                                    mus.AlbumID = CriarAlbum(oldMus.TRIB_CD_CD_TRILHA, oldMus.TRIB_DS_CD_TITLE,
                                                             repositorio);

                                    mus.Ativo = true;
                                    repositorio.Adicionar(mus);

                                    Transferencia.DicMusicaBranca.Add(musicaID, mus.MusicaID);
                                }
                                else
                                {
                                    return 0;
                                }
                                break;
                            }

                            //"Comercial"
                        case 2:
                            {
                                var oldMus = (from m in oldDB.SINCTISRC
                                              where m.ISRC_CD_CODIGO == musicaID
                                              select m).FirstOrDefault();

                                if (oldMus != null && !string.IsNullOrWhiteSpace(oldMus.ISRC_DS_AUTORES))
                                {
                                    mus.AutorID =
                                        new MigraAutorInterprete().CriarAutor(oldMus.ISRC_DS_AUTORES.NormalizarTrim());
                                    mus.InterpreteID =
                                        new MigraAutorInterprete().CriarInterprete(
                                            oldMus.ISRC_DS_INTERPRETES.NormalizarTrim());
                                    mus.Duracao = String.IsNullOrEmpty(oldMus.ISRC_DS_TRACK_TIME)
                                                      ? new TimeSpan()
                                                      : ConverterTempo(oldMus.ISRC_DS_TRACK_TIME);
                                    mus.NomeArquivo = oldMus.ISRC_NM_ARQUIVO;
                                    mus.Titulo = oldMus.ISRC_DS_TITULO.NormalizarTrim();
                                    mus.ISRC = oldMus.ISRC_DS_ISRC;

                                    mus.Ativo = true;
                                    repositorio.Adicionar(mus);

                                    Transferencia.DicMusicaComercial.Add(musicaID, mus.MusicaID);
                                }
                                else
                                {
                                    return 0;
                                }
                                break;
                            }
                            //"Record"
                        case 3:
                            {
                                var oldMus = (from m in oldDB.SINCTTREC
                                              where m.TREC_CD_CODIGO == musicaID
                                              select m).SingleOrDefault();
                                if (oldMus != null && !string.IsNullOrWhiteSpace(oldMus.TREC_DS_AUTOR)
                                    && !string.IsNullOrWhiteSpace(oldMus.TREC_DS_ISRC)
                                    && !string.IsNullOrWhiteSpace(oldMus.TREC_NM_ARQUIVO))
                                {
                                    mus.AutorID =
                                        new MigraAutorInterprete().CriarAutor(oldMus.TREC_DS_AUTOR.NormalizarTrim());

                                    mus.InterpreteID =
                                        new MigraAutorInterprete().CriarInterprete(
                                            oldMus.TREC_DS_INTERPRETES.NormalizarTrim());

                                    mus.Duracao = String.IsNullOrEmpty(oldMus.TREC_DS_TRACK_TIME)
                                                      ? new TimeSpan()
                                                      : ConverterTempo(oldMus.TREC_DS_TRACK_TIME);
                                    mus.NomeArquivo = oldMus.TREC_NM_ARQUIVO;
                                    mus.Titulo = oldMus.TREC_DS_TITULO.NormalizarTrim();
                                    mus.ISRC = oldMus.TREC_DS_ISRC;

                                    mus.Ativo = true;
                                    repositorio.Adicionar(mus);

                                    Transferencia.DicMusicaRecord.Add(musicaID, mus.MusicaID);
                                }
                                else
                                {
                                    return 0;
                                }
                                break;
                            }
                    }

                    time.Stop();
                }
            }
            return mus.MusicaID;
        }

        public static TimeSpan ConverterTempo(string tempo)
        {
            var valores = tempo.Split(':');
            var time = new TimeSpan();

            switch (valores.Count())
            {
                case 1:
                    {
                        int valor0;
                        if (!Int32.TryParse(valores[0], out valor0))
                        {
                            valores[0] = "00";
                        }
                        time = new TimeSpan(0, 0, 0, Int32.Parse(valores[0]));
                        break;
                    }
                case 2:
                    {
                        int valor1;
                        if (!Int32.TryParse(valores[1], out valor1))
                        {
                            valores[1] = "00";
                        }
                        if (Int32.Parse(valores[1]) >= 60)
                        {
                            valores[1] = (Int32.Parse(valores[1]) - 60).ToString(CultureInfo.InvariantCulture);
                            valores[0] = (Int32.Parse(valores[0]) + 1).ToString(CultureInfo.InvariantCulture);
                        }
                        time = new TimeSpan(0, 0, Int32.Parse(valores[0]), Int32.Parse(valores[1]));
                        break;
                    }
                case 3:
                    {
                        int valor2;
                        if (!Int32.TryParse(valores[2], out valor2))
                        {
                            valores[2] = "00";
                        }

                        if (Int32.Parse(valores[2]) >= 60)
                        {
                            valores[2] = (Int32.Parse(valores[2]) - 60).ToString(CultureInfo.InvariantCulture);
                            valores[1] = (Int32.Parse(valores[1]) + 1).ToString(CultureInfo.InvariantCulture);
                        }
                        if (Int32.Parse(valores[1]) >= 60)
                        {
                            valores[1] = (Int32.Parse(valores[1]) - 60).ToString(CultureInfo.InvariantCulture);
                            valores[0] = (Int32.Parse(valores[0]) + 1).ToString(CultureInfo.InvariantCulture);
                        }
                        time = new TimeSpan(0, 0, Int32.Parse(valores[1]), Int32.Parse(valores[2]));
                        break;
                    }
            }
            return time;
        }
    }
}