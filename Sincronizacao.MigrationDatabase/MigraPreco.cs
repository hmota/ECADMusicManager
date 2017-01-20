using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    internal class MigraPreco : IMigracao
    {
        public void Migrar()
        {
            List<SINCTPCGE> oldPrecosGen;
            using (SincOldEntities oldDB = new SincOldEntities())
            {
                oldPrecosGen = (from p in oldDB.SINCTPCGE
                               select p).ToList();
            
            foreach (var precGen in oldPrecosGen)
            {
                    using (Repositorio repositorio = new Repositorio())
                    {
                        var oldPreco = oldDB.SINCTPRCL
                            .Single(p=>p.PRCL_CD_CODIGO == precGen.PRCL_CD_CODIGO);

                        if (oldPreco == null)
                        {
                            continue;
                        }
                        int genID = Transferencia.DicGenero[precGen.GENE_CD_CODIGO];
                        int classID = Transferencia.DicClassificacao[oldPreco.CLAS_CD_CODIGO.Value];

                        var oldAss = (from p in oldDB.SINCTPREC
                                      where p.PREC_CD_CODIGO == oldPreco.PREC_CD_CODIGO
                                      select p).FirstOrDefault();
                        int assID = Transferencia.DicAssociacao[oldAss.ASSO_CD_CODIGO.Value];

                        var prec = new Preco
                                       {
                                           Abrangencia = oldPreco.PRCL_DS_ABRANGENCIA,
                                           AssociacaoID = assID,
                                           ClassificacaoID = classID,
                                           GeneroID = genID,
                                           Valor = oldPreco.PRCL_VL_VALOR,
                                           Vigencia = oldAss.PREC_DT_VIGENCIA_INICIO.Year,
                                           Ativo = true
                                       };

                        repositorio.Adicionar(prec);

                        Console.Write('.');
                    }
                }
            }
        }
    }
}