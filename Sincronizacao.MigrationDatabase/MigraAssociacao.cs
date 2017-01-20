using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraAssociacao : IMigracao
    {
        public void Migrar()
        { 
            Transferencia.DicAssociacao = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldAssociacoes = from a in oldDB.SINCTASSO
                                     select a;

                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var associacao in oldAssociacoes)
                    {
                        var ass = new Associacao
                                      {
                                          Nome = associacao.ASSO_DS_NOME.NormalizarTrim(),
                                          Ativo = true
                                      };

                        repositorio.Adicionar(ass);

                        Transferencia.DicAssociacao.Add(associacao.ASSO_CD_CODIGO, ass.AssociacaoID);

                        Console.Write('.');
                    }
                }
            }

            Console.Write(Environment.NewLine);
        }
    }
}