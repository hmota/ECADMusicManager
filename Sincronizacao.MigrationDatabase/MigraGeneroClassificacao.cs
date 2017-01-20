using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraGeneroClassificacao : IMigracao
    {
        public void Migrar()
        {
            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(new Classificacao { Descricao = "BACKGROUND INSTRUMENTAL", Ativo = true});
                repositorio.Adicionar(new Classificacao { Descricao = "BACKGROUND VOCAL", Ativo = true });
                repositorio.Adicionar(new Classificacao { Descricao = "VISUAL INSTRUMENTAL", Ativo = true });
                repositorio.Adicionar(new Classificacao { Descricao = "VISUAL VOCAL", Ativo = true });
                repositorio.Adicionar(new Classificacao { Descricao = "OPENING THEME", Ativo = true });
                repositorio.Adicionar(new Classificacao { Descricao = "CLOSING THEME", Ativo = true });
                repositorio.Adicionar(new Classificacao { Descricao = "PERSON THEME", Ativo = true });

                ICollection<Classificacao> classificacoes = new Collection<Classificacao>
                                                            {
                                                                new Classificacao
                                                                    {Descricao = "BACKGROUND INSTRUMENTAL", Ativo = true},
                                                                new Classificacao {Descricao = "BACKGROUND VOCAL", Ativo = true},
                                                                new Classificacao {Descricao = "VISUAL INSTRUMENTAL", Ativo = true},
                                                                new Classificacao {Descricao = "VISUAL VOCAL", Ativo = true},
                                                                new Classificacao {Descricao = "OPENING THEME", Ativo = true},
                                                                new Classificacao {Descricao = "CLOSING THEME", Ativo = true},
                                                                new Classificacao {Descricao = "PERSON THEME", Ativo = true},
                                                            };
                using (SincOldEntities oldDB = new SincOldEntities())
                {
                    var oldGeneroClassificacao = from gc in oldDB.SINCGECL
                                                 select gc;

                    foreach (var itemGenCla in oldGeneroClassificacao)
                    {
                        var genID = Transferencia.DicGenero[itemGenCla.GENE_CD_CODIGO];
                        var claID = Transferencia.DicClassificacao[itemGenCla.CLAS_CD_CODIGO];

                        var gen = repositorio.Obter<Genero>(g => g.GeneroID == genID).SingleOrDefault();
                        var cla = repositorio.Obter<Classificacao>(c => c.ClassificacaoID == claID).SingleOrDefault();

                        if (gen.Descricao == "JORNALISMO" || gen.Descricao == "SHOW")
                        {
                            gen.Classificacoes.Add(cla);
                            repositorio.Editar(gen);
                        }
                        Console.Write('.');
                    }
                }

                var genNovela = repositorio.Obter<Genero>(g => g.Descricao == "NOVELA").Single();
                var genSerie = repositorio.Obter<Genero>(g => g.Descricao == "SERIE").Single();
                var genMiniSerie = repositorio.Obter<Genero>(g => g.Descricao == "MINISSERIE").Single();

                foreach (var classificacao in classificacoes)
                {
                    repositorio.Adicionar(classificacao);
                    genNovela.Classificacoes.Add(classificacao);
                    genSerie.Classificacoes.Add(classificacao);
                    genMiniSerie.Classificacoes.Add(classificacao);
                    repositorio.Editar(genNovela);
                    repositorio.Editar(genSerie);
                    repositorio.Editar(genMiniSerie);
                    Console.Write('.');
                }
            }
        }
    }
}
