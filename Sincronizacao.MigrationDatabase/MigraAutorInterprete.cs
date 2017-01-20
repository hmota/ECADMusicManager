using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.MigrationDatabase
{
    public class MigraAutorInterprete
    {
        public int CriarAutor(string nomeAutor)
        {
            Autor aut;

            if (!Transferencia.DicAutor.ContainsKey(nomeAutor))
            {
                bool autorExiste;
                using (Repositorio repositorio = new Repositorio())
                {
                    autorExiste = repositorio.Obter<Autor>(a => a.Nome == nomeAutor).Any();

                    if (!autorExiste)
                    {
                        aut = new Autor
                                  {
                                      Nome = nomeAutor.NormalizarTrim()
                                  };
                        repositorio.Adicionar(aut);
                        Transferencia.DicAutor.Add(aut.Nome, aut.AutorID);
                    }
                    else
                    {
                        aut = repositorio.Obter<Autor>(a => a.Nome == nomeAutor).FirstOrDefault();
                    }
                }
            }
            else
            {
                return Transferencia.DicAutor[nomeAutor];
            }
            return aut != null ? aut.AutorID : 0;
        }

        public int CriarInterprete(string nomeInterprete)
        {
            Interprete inter;

            if (!Transferencia.DicInterprete.ContainsKey(nomeInterprete))
            {
                bool interpreteExiste;
                using (Repositorio repositorio = new Repositorio())
                {
                    interpreteExiste = repositorio.Obter<Interprete>(a => a.Nome == nomeInterprete).Any();

                    if (!interpreteExiste)
                    {
                        inter = new Interprete
                                    {
                                        Nome = nomeInterprete.NormalizarTrim()
                                    };
                        repositorio.Adicionar(inter);
                        Transferencia.DicInterprete.Add(inter.Nome, inter.InterpreteID);
                    }
                    else
                    {
                        inter = repositorio.Obter<Interprete>(a => a.Nome == nomeInterprete).FirstOrDefault();
                    }
                }
            }
            else
            {
                return Transferencia.DicInterprete[nomeInterprete];
            }

            return inter != null ? inter.InterpreteID : 0;
        }
    }
}
