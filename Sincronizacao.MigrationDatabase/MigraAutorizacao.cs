using System;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    internal class MigraAutorizacao : IMigracao
    {
        public void Migrar()
        {
            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldAutorizacoes = from a in oldDB.SINCTAUTO
                                      orderby a.MUSI_CD_CODIGO
                                      select a;

                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var autorizacao in oldAutorizacoes)
                    {
                        if (autorizacao.MUSI_CD_CODIGO.HasValue)
                        {
                            //tem q ser o dicionario de sonorizacao
                            if (Transferencia.DicSonorizacao.ContainsKey(autorizacao.MUSI_CD_CODIGO.Value)
                                && Transferencia.DicEditora.ContainsKey(autorizacao.EDIT_CD_CODIGO.Value))
                            {
                                
                                var auto = new Autorizacao
                                               {
                                                   Vencimento = autorizacao.AUTO_DT_VENCIMENTO.Value,
                                                   Porcentagem = autorizacao.AUTO_VL_PORCENTAGEM.Value,
                                                   Valor = autorizacao.AUTO_VL_VALOR.Value,
                                                   UsuarioID = 1,
                                                   SonorizacaoID =
                                                       Transferencia.DicSonorizacao[autorizacao.MUSI_CD_CODIGO.Value],
                                                       EditoraID = Transferencia.DicEditora[autorizacao.EDIT_CD_CODIGO.Value]
                                               };

                                repositorio.Adicionar(auto);
                                Transferencia.DicAutorizacao.Add(autorizacao.AUTO_CD_CODIGO, auto.AutorizacaoID);

                                Console.Write('.');
                            }
                        }
                    }
                }
            }
        }
    }
}