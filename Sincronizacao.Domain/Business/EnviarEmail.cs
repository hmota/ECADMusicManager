using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SincronizacaoMusical.Domain.Business
{
    public class EnviarEmail
    {
        public void Enviar(string corpo)
        {
            var mailService = new ServiceEmail.SincronizacaoMusicalService();

            var email = new ServiceEmail.Email();
            email.Para = new[] {"hmota@outlook.com", "haga1985@gmail.com"};
            email.De = "hmota@outlook.com";
            email.Assunto = "Sincronização Musical - Novas musicas cadastradas";
            
            var conteudo = new ServiceEmail.Conteudo();
            conteudo.Texto = corpo;

            try
            {
                mailService.SendEmailWithContent(email, conteudo);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao enviar email: "+ex.Message);
            }
            
        }
    }
}
