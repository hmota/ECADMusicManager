using System;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
//using SincronizacaoMusical.MigrationDatabase.ServiceSincronizacaoMusicalList;

namespace SincronizacaoMusical.MigrationDatabase
{
    class MigraTipoTrilha : IMigracao
    {
        public void Migrar()
        {
            var tt1 = new TipoTrilha { Descricao = "BIBLIOTECA MUSICAL" };
            var tt2 = new TipoTrilha { Descricao = "COMERCIAL" };
            var tt3 = new TipoTrilha { Descricao = "RECORD" };

            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(tt1);
                repositorio.Adicionar(tt2);
                repositorio.Adicionar(tt3);
            }

            Console.Write('.');
        }
    }
}