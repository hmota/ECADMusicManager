using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sincronizacao.Domain.Entities;

namespace Sincronizacao.UI
{
    public class VerificarUsuario
    {
        public Usuario ObterUsuario()
        {
            return new Usuario()
                       {
                           Login = Environment.UserName
                       };
        }
    }
}