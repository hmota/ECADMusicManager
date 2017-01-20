using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraEditora : IMigracao
    {
        public void Migrar()
        {
            Transferencia.DicEditora = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldEditoras = from e in oldDB.SINCTEDIT
                                  select e;

                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var itemEditora in oldEditoras)
                    {
                        if (!repositorio.Obter<Editora>(e=> e.Nome == itemEditora.EDIT_DS_IDENT).Any())
                        {
                            var edit = new Editora
                                           {
                                               Nome = itemEditora.EDIT_DS_IDENT.NormalizarTrim(),
                                               RazaoSocial = itemEditora.EDIT_DS_NOME.NormalizarTrim(),
                                               Ativo = true
                                           };

                            var assoID = Transferencia.DicAssociacao[itemEditora.ASSO_CD_CODIGO.Value];

                            edit.AssociacaoID = assoID;

                            repositorio.Adicionar(edit);

                            Transferencia.DicEditora.Add(itemEditora.EDIT_CD_CODIGO, edit.EditoraID);

                            Console.Write('.');
                        }
                    }
                }
            }
            Console.Write(Environment.NewLine);
        }
    }
}
