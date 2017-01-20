using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Util;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraTipoExibicao : IMigracao
    {
        public void Migrar()
        {
            Transferencia.DicTipoExibicao = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldTiposExibicoes = from te in oldDB.SINCTEXIB
                                        select te;
                using (Repositorio repositorio = new Repositorio())
                {
                    var q1 = new TipoExibicao { Ativo = true, Descricao = "INDEFINIDO" };
                    repositorio.Adicionar(q1);

                    foreach (var tipoExibicao in oldTiposExibicoes)
                    {
                        var tipoExib = new TipoExibicao { Descricao = tipoExibicao.EXIB_DS_DESCRICAO.NormalizarTrim(),Ativo = true};
                        repositorio.Adicionar(tipoExib);

                        Transferencia.DicTipoExibicao.Add(tipoExibicao.EXIB_CD_CODIGO, tipoExib.TipoExibicaoID);

                        Console.Write('.');
                    }
                }
            }
        }
    }
}
