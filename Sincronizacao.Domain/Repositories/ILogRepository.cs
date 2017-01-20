using System;
using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Domain.Repositories
{
    public interface ILogRepository
    {
        void WriteLog(string message, LogType type, string nomeObjeto, int objetoID, string descricao, AcaoType acao, 
            string valorNovo, string valorAntigo, string Usuario);

        void WriteLog(string message, LogType type, string user);

        void WriteLog(string message, LogType type);

        void WriteLog(Exception exception);

        void WriteLogDebug(string message, LogType type, string nomeObjeto, int objetoID, string descricao, AcaoType acao,
            string valorNovo, string valorAntigo, string Usuario);

        void WriteLogDebug(string message, LogType type, string user);

        void WriteLogDebug(string message, LogType type);

        void WriteLogDebug(Exception exception);
    }
}