using System;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
//using SincronizacaoMusical.MigrationDatabase.ServiceSincronizacaoMusicalList;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraQuadro : IMigracao
    {
        public void Migrar()
        {
            var q1 = new Quadro { Ativo = true, Descricao = "INDEFINIDO" };
            var q2 = new Quadro { Ativo = true, Descricao = "GERAL" };

            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(q1);
                repositorio.Adicionar(q2);
            }

            Console.Write('.');
        }
    }
}
