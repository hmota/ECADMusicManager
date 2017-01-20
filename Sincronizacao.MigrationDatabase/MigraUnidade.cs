using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Util;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraUnidade : IMigracao
    {
        public void Migrar()
        {
            Transferencia.DicUnidade = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldUnidades = from u in oldDB.SINCTUNID
                                  select u;

                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var itemUnidade in oldUnidades)
                    {
                        var unid = new Unidade
                        {
                            Nome = itemUnidade.UNID_DS_NOME.NormalizarTrim(),
                            Descricao = itemUnidade.UNID_DS_DESCRICAO.NormalizarTrim(),
                            CEP = itemUnidade.UNID_DS_CEP,
                            Cidade = itemUnidade.UNID_DS_CIDADE.NormalizarTrim(),
                            CNPJ = itemUnidade.UNID_DS_CNPJ.NormalizarTrim(),
                            Contato = itemUnidade.UNID_DS_CONTATO.NormalizarTrim(),
                            Logradouro = itemUnidade.UNID_DS_LOGRADOURO.NormalizarTrim(),
                            RazaoSocial = itemUnidade.UNID_DS_RAZAO.NormalizarTrim(),
                            UF = itemUnidade.UNID_DS_UF.NormalizarTrim(),
                        };
                        repositorio.Adicionar(unid);

                        Transferencia.DicUnidade.Add(itemUnidade.UNID_CD_CODIGO, unid.UnidadeID);

                        Console.Write('.');
                    }
                }
            }
        }
    }
}
