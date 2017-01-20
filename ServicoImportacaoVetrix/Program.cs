using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Infrastructure.Data;

namespace ServicoImportacaoVetrix
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            ILogRepository logRepository = new DatabaseLogRepository();

            logRepository.WriteLog("Robo de importação iniciado.", LogType.Informacao);

            string URLString = ConfigurationSettings.AppSettings["URLOrigem"];
            logRepository.WriteLogDebug("Pasta " + URLString, LogType.Informacao);
            FileSystemWatcher fsw = new FileSystemWatcher();
            logRepository.WriteLogDebug("FSW instaciado", LogType.Informacao);
            fsw.Path = URLString;
            logRepository.WriteLogDebug("Set PATH: " + URLString, LogType.Informacao);
            fsw.Filter = "SINCRECORD_*.xml";

            logRepository.WriteLogDebug("Set filter " + fsw.Filter, LogType.Informacao);
            fsw.NotifyFilter = NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.CreationTime;
            logRepository.WriteLogDebug("Set filter Ok", LogType.Informacao);
            fsw.Created += new FileSystemEventHandler(fsw_Created);
            logRepository.WriteLogDebug("Delegate criado", LogType.Informacao);
            fsw.EnableRaisingEvents = true;
            logRepository.WriteLogDebug("fsw habilitado", LogType.Informacao);
            logRepository.WriteLogDebug("Monitorando...", LogType.Informacao);

            while (true)
            {
            }
        }

        private static void fsw_Created(object sender, FileSystemEventArgs e)
        {
            ILogRepository logRepository = new DatabaseLogRepository();

            logRepository.WriteLogDebug("Arquivo encontrado...", LogType.Informacao);
            
            List<RowVetrixErro> erros = null;
            try
            {
                Thread.Sleep(3000);
                string urlDest = ConfigurationSettings.AppSettings["URLDestino"] + e.Name;
                if (!File.Exists(urlDest))
                {
                    File.Move(e.FullPath, urlDest);
                    logRepository.WriteLogDebug("Arquivo movido para importação.", LogType.Informacao);

                    ImportRobo imp = new ImportRobo();

                    logRepository.WriteLogDebug("DLL importação instanciada.", LogType.Informacao);
                    logRepository.WriteLogDebug("HASH import - " + imp.GetHashCode(), LogType.Informacao);
                    erros = imp.ImportToXML(urlDest);
                    logRepository.WriteLog("Importação concluida.", LogType.Informacao);

                    ExportRobo exp = new ExportRobo();
                    logRepository.WriteLogDebug("DLL exportacao instanciada.", LogType.Informacao);
                    logRepository.WriteLogDebug("HASH export - " + imp.GetHashCode(), LogType.Informacao);
                    exp.ExportarErrosVetrix(erros, "MusicasNaoProcessadas_" + e.Name);
                    logRepository.WriteLog("Exportação de erros concluida.", LogType.Informacao);
                    
                    Thread.Sleep(3000);
                }
                else
                    throw new Exception("O arquivo já existe no diretório de destino!");
            }

            catch (Exception ex)
            {
                logRepository.WriteLog(ex);
            }
        }
    }
}
