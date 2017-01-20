using System;
using System.Linq;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;

namespace SincronizacaoMusical.Domain.Business
{
    public class VerificarUsuario
    {
        public Usuario ObterUsuario(string login)
        {
            //TODO: Obter usuario logado na maquina
            try
            {
            Repositorio repositorio = new Repositorio();
            Usuario usuario = repositorio.Obter<Usuario>(u => u.Login.Contains(login)).FirstOrDefault();
            return usuario;
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao logar: "+ex.Message, ex);
            }
        }
    }
}