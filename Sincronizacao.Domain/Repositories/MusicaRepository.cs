using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Data.Repositories
{
    public class MusicaRepository
    {
        public List<Musica> GetMusicas()
        {
            using (Context context = new Context())
            {
                var musicas = context.Musicas.ToList();
                return musicas;
            }
        }

        public List<Musica> GetMusicasByParam(string ISRC, string titulo, string autor, string interprete)
        {
            List<Musica> ret = new List<Musica>();

            using (Context context = new Context())
            {
                var musicas = context.Musicas.Include("TipoTrilha");

                if (!String.IsNullOrWhiteSpace(ISRC))
                    ret = musicas.Where(m => m.ISRC == ISRC).ToList<Musica>();
                if (!String.IsNullOrWhiteSpace(titulo))
                    ret = musicas.Where(m => m.Titulo == titulo).ToList<Musica>();
                if (!String.IsNullOrWhiteSpace(autor))
                    ret = musicas.Include("Autor").Where(m => m.Autor.Nome == autor).ToList<Musica>();
                if (!String.IsNullOrWhiteSpace(interprete))
                    ret = musicas.Include("Interprete").Where(m => m.Interprete.Nome == interprete).ToList<Musica>();
            }

            return ret;
        }

        public Musica GetMusicasByISRC(string ISRC)
        {
            if (!string.IsNullOrWhiteSpace(ISRC))
            {
                Musica musica;

                using (Context context = new Context())
                {
                    musica = (from m in context.Musicas
                              where m.ISRC == ISRC
                              select m).SingleOrDefault();
                }

                return musica;
            }
            else
            {
                return null;
            }
        }

        public List<Musica> GetMusicasByData(DateTime? dataInicial)
        {
            if (dataInicial.HasValue)
            {
                using (Context context = new Context())
                {
                    var musicas = (from mus in context.Musicas
                                   where mus.CadastradaEm >= dataInicial
                                   select mus).ToList();
                    return musicas;
                }
            }
            else
                return null;
        }
    }
}
