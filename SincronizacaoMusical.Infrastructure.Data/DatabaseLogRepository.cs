using System.Configuration;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Infrastructure.Data
{
    public class DatabaseLogRepository : ILogRepository 
    {
        public void WriteLog(string message, LogType type, string nomeObjeto, int objetoID, string descricao, 
            AcaoType acao, string valorNovo, string valorAntigo, string usuario)
        {
            using (var context = new Context())
            {
                Log entry = new Log
                                {
                                    Message = message,
                                    Type = type,
                                    NomeObjeto = nomeObjeto,
                                    ObjetoID = objetoID,
                                    Descricao = descricao,
                                    Acao = acao,
                                    ValorNovo = valorNovo,
                                    ValorAntigo = valorAntigo,
                                    Usuario = usuario,
                                    Sistema = "Sincronização",
                                    Versao = ConfigurationManager.AppSettings["Versao"]
                                };

                context.Logs.Add(entry);
                context.SaveChanges();
            }
        }

        public void WriteLog(string message, LogType type, string user)
        {
            using (var context = new Context())
            {
                Log entry = new Log();

                entry.Message = message;
                entry.Type = type;
                entry.Usuario = user;
                entry.Sistema = "Sincronização";
                entry.Versao = ConfigurationManager.AppSettings["Versao"];

                context.Logs.Add(entry);

                context.SaveChanges();
            }
        }

        public void WriteLog(string message, LogType type)
        {
            using (var context = new Context())
            {
                Log entry = new Log();

                entry.Message = message;
                entry.Type = type;
                entry.Sistema = "Sincronização";
                entry.Versao = ConfigurationManager.AppSettings["Versao"];

                context.Logs.Add(entry);
                context.SaveChanges();
            }
        }
        
        public void WriteLog(System.Exception exception)
        {
            string message = string.Format("{0}\n\n{1}\n\n{2}",
                                               exception.Message,
                                               (exception.InnerException == null) ? string.Empty : exception.InnerException.Message,
                                               exception.StackTrace);

            WriteLog(message, LogType.Erro);
        }

        #region Debug
        //Logs para debug

        public void WriteLogDebug(string message, LogType type, string nomeObjeto, int objetoID, string descricao,
                                  AcaoType acao, string valorNovo, string valorAntigo, string usuario)
        {
            if (ConfigurationManager.AppSettings["Debug"] == "false") return;
            using (var context = new Context())
            {
                Log entry = new Log
                                {
                                    Message = message,
                                    Type = type,
                                    NomeObjeto = nomeObjeto,
                                    ObjetoID = objetoID,
                                    Descricao = descricao,
                                    Acao = acao,
                                    ValorNovo = valorNovo,
                                    ValorAntigo = valorAntigo,
                                    Usuario = usuario,
                                    Debug = true,
                                    Sistema = "Sincronização",
                                    Versao = ConfigurationManager.AppSettings["Versao"]
                                };

                context.Logs.Add(entry);
                context.SaveChanges();
            }
        }

        public void WriteLogDebug(string message, LogType type, string user)
        {
            if (ConfigurationManager.AppSettings["Debug"] == "false") return;
            using (var context = new Context())
            {
                Log entry = new Log();

                entry.Message = message;
                entry.Type = type;
                entry.Usuario = user;
                entry.Debug = true;
                entry.Sistema = "Sincronização";
                entry.Versao = ConfigurationManager.AppSettings["Versao"];

                context.Logs.Add(entry);

                context.SaveChanges();
            }
        }

        public void WriteLogDebug(string message, LogType type)
        {
            if (ConfigurationManager.AppSettings["Debug"] == "false") return;
            using (var context = new Context())
            {
                Log entry = new Log();

                entry.Message = message;
                entry.Type = type;
                entry.Debug = true;
                entry.Sistema = "Sincronização";
                entry.Versao = ConfigurationManager.AppSettings["Versao"];

                context.Logs.Add(entry);
                context.SaveChanges();
            }
        }

        public void WriteLogDebug(System.Exception exception)
        {
            if (ConfigurationManager.AppSettings["Debug"] == "false") return;
            string message = string.Format("{0}\n\n{1}\n\n{2}",
                                               exception.Message,
                                               (exception.InnerException == null) ? string.Empty : exception.InnerException.Message,
                                               exception.StackTrace);

            WriteLogDebug(message, LogType.Erro);
        }
        #endregion
    }
}
