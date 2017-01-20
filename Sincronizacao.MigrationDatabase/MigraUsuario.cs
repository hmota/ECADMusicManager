using System;
using System.Collections.Generic;
using System.Linq;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
//using SincronizacaoMusical.MigrationDatabase.ServiceSincronizacaoMusicalList;

namespace SincronizacaoMusical.MigrationDatabase
{
    internal class MigraUsuario : IMigracao
    {
        public void Migrar()
        {
            var sistema = new Usuario { Login = "SISTEMA", Administrador = true, Analista = true, Supervisor = true };
            var eUsuario = new Usuario { Login = "EBSANTOS", Administrador = true, Analista = false, Supervisor = false };
            var wUsuario = new Usuario { Login = "WLSILVA", Administrador = false, Analista = true, Supervisor = true };
            var aUsuario = new Usuario { Login = "ALMSOUSA", Administrador = false, Analista = true, Supervisor = false };
            var hrUsuario = new Usuario { Login = "HROLIVEIRA", Administrador = false, Analista = true, Supervisor = false };
            var hUsuario = new Usuario { Login = "HMOTA", Administrador = true, Analista = true, Supervisor = true };
            var cUsuario = new Usuario { Login = "CHLIMA", Administrador = false, Analista = false, Supervisor = false };

            using (Repositorio repositorio = new Repositorio())
            {
                repositorio.Adicionar(sistema);
                repositorio.Adicionar(eUsuario);
                repositorio.Adicionar(wUsuario);
                repositorio.Adicionar(aUsuario);
                repositorio.Adicionar(hrUsuario);
                repositorio.Adicionar(hUsuario);
            }
        }
    }
}
