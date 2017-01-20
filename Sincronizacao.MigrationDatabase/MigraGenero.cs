using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Util;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraGenero : IMigracao
    {
        public void Migrar()
        {
            Transferencia.DicGenero = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldGeneros = from g in oldDB.SINCTGENE
                                 select g;
                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var itemGenero in oldGeneros)
                    {
                        var gen = new Genero { Descricao = itemGenero.GENE_DS_DESCRICAO.NormalizarTrim(), Ativo = true};
                        repositorio.Adicionar(gen);

                        Transferencia.DicGenero.Add(itemGenero.GENE_CD_CODIGO, gen.GeneroID);
                        Console.Write('.');
                    }
                }
            }
        }
    }
}
