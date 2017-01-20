using System;

namespace SincronizacaoMusical.Domain.Business
{
    public class VerificarExibicao
    {
        Context context = SingletonContext.Instance.Context;

        public bool ExisteSincronizacao(int programaID, DateTime dataExibicao)
        {
            //TODO: Obter Sincronizacao a partir da data e programa
            //Context context = SingletonContext.Instance.Context;

            //Usuario usuario = (from u in context.Usuarios
            //                   where u.Login == login
            //                   select u).Single();
            throw new Exception();
        }

        public bool SincronizacaoAprovada(int programaID, DateTime dataExibicao)
        {
            //var exibicao = (from e in context.Exibicoes
            //                    where e.ProgramaID == programaID &&
            //                    Entity
            //                    )

            //var Sincronizacao = (from s in context.Sonorizacoes 
            //                    where s.Exibicao )
            
            throw new Exception();
        }

    }
}