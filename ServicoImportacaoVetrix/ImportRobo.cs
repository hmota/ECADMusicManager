using System.Threading;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Domain.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Infrastructure.Data;

namespace ServicoImportacaoVetrix
{
    public class ImportRobo
    {
        private int _totalImportadasMusicas;
        private int _totalImportadasSucesso;
        private int _totalImportadasErros;
        private ILogRepository _logRepository;
        List<RowVetrixErro> erros;

        public ImportRobo()
        {
            _logRepository = new DatabaseLogRepository();
            _logRepository.WriteLogDebug("Import Construtor OK...", LogType.Informacao);
        }
        public List<RowVetrixErro> ImportToXML(string arquivo)
        {
            try
            {
                _logRepository.WriteLogDebug("Importando...", LogType.Informacao);
                //List<VetrixErro> erros = null;
                erros = new ImportacaoVetrix().Importar(arquivo, out _totalImportadasMusicas,
                                                        out _totalImportadasSucesso,
                                                        out _totalImportadasErros);
                return erros;
            }
            catch (Exception ex)
            {
                _logRepository.WriteLog(ex);
                return erros;
            }
        }
    }
}
