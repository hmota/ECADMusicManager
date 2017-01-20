using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Util;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraPrograma : IMigracao
    {
        public void Migrar()
        {
            Transferencia.DicPrograma = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldProgramas = from p in oldDB.SINCTPROD
                                   select p;

                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var itemPrograma in oldProgramas)
                    {

                        var prog = new Programa
                                       {
                                           Nome = itemPrograma.PROD_DS_NOME.NormalizarTrim(), 
                                           Ordem = itemPrograma.PROD_DS_ORDEM.NormalizarTrim(), 
                                           Ativo = (bool)itemPrograma.PROD_FL_ATIVO
                                       };

                        int genID = Transferencia.DicGenero[itemPrograma.GENE_CD_CODIGO.Value];

                        prog.GeneroID = genID;

                        repositorio.Adicionar(prog);

                        Transferencia.DicPrograma.Add(itemPrograma.PROD_CD_CODIGO, prog.ProgramaID);

                        Console.Write('.');
                    }
                }
            }
        }
    }
}
