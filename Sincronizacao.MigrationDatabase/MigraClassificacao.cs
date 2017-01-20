using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Util;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraClassificacao : IMigracao
    {
        public void Migrar()
        {
            Transferencia.DicClassificacao = new Dictionary<int, int>();

            using (SincOldEntities oldDB = new SincOldEntities())
            {
                var oldClassificacoes = from c in oldDB.SINCTCLAS
                                        select c;

                using (var repositorio = new Repositorio())
                {
                    var q1 = new Classificacao() { Ativo = true, Descricao = "INDEFINIDO" };
                    repositorio.Adicionar(q1);

                    foreach (var itemClassificacao in oldClassificacoes)
                    {
                        var clas = new Classificacao { Descricao = itemClassificacao.CLAS_DS_DESCRICAO.NormalizarTrim(), Ativo = true};

                        repositorio.Adicionar(clas);

                        Transferencia.DicClassificacao.Add(itemClassificacao.CLAS_CD_CODIGO, clas.ClassificacaoID);

                        Console.Write('.');
                    }
                }
            }
        }
    }
}
