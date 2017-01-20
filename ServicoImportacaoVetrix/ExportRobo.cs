using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Office.Interop.Excel;
using SincronizacaoMusical.Domain.Business;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using System.Configuration;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Infrastructure.Data;


namespace ServicoImportacaoVetrix
{
    public class ExportRobo
    {
        private ILogRepository _logRepository;
        private string _pastaExportacao = ConfigurationSettings.AppSettings["PastaExportacao"];


        public ExportRobo()
        {
            _logRepository = new DatabaseLogRepository();
            _logRepository.WriteLogDebug("Export Construtor OK...", LogType.Informacao);
        }

        public void ExportarErrosVetrix(List<RowVetrixErro> erros, string nomeArquivoExportacao)
        {
            _logRepository.WriteLogDebug("Exportando...", LogType.Informacao);
            // creating Excel Application
            _Application app = new Application();

            // creating new WorkBook within Excel application
            _Workbook workbook = app.Workbooks.Add(Type.Missing);

            // creating new Excelsheet in workbook
            _Worksheet worksheet;

            // see the excel sheet behind the program
            app.Visible = false;

            // get the reference of first sheet. By default its name is Sheet1.
            // store its reference to worksheet
            worksheet = workbook.ActiveSheet;

            // changing the name of active sheet
            worksheet.Name = "Exported from Service";
            _logRepository.WriteLogDebug("preparando export...", LogType.Informacao);
            var export = new Export();
            _logRepository.WriteLogDebug("export criado...", LogType.Informacao);
            //var ds = export.CreateDataSet(dgVetrix.ItemsSource.Cast<VetrixErro>().ToList());
            var ds = export.CreateDataSet(erros);
            _logRepository.WriteLogDebug("dataset...", LogType.Informacao);
            var table = ds.Tables[0];
            _logRepository.WriteLogDebug("count rows table " + table.Rows.Count, LogType.Informacao);
            // storing header part in Excel
            //for (int i = 1; i < dgVetrix.Columns.Count + 1; i++)
            for (int i = 1; i < table.Columns.Count + 1; i++)
            {
                //worksheet.Cells[1, i] = dgVetrix.Columns[i - 1].Header.ToString();
                worksheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
            }

            // storing Each row and column value to excel sheet
            for (int i = 0; i < table.Rows.Count; i++)
            {
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    _logRepository.WriteLogDebug("i: " + i + " / j: " + j, LogType.Informacao);

                    var dt = new DateTime();
                    if (table.Rows[i][j].ToString().Contains('/') &&
                        DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                    {
                        string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                        Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                        rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                        worksheet.Cells[i + 2, j + 1] = dataFormatada;
                    }
                    else
                    {
                        worksheet.Cells[i + 2, j + 1] = table.Rows[i][j].ToString();
                    }
                }
            }
            _logRepository.WriteLogDebug("worksheet criado...", LogType.Informacao);
            try
            {
                // save the application
                workbook.SaveAs(_pastaExportacao + nomeArquivoExportacao + ".xls", Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                Type.Missing,
                                Type.Missing, Type.Missing);
                _logRepository.WriteLogDebug("salvo e fechando...", LogType.Informacao);
            }
            catch (Exception ex)
            {
                _logRepository.WriteLog(ex);
                throw new Exception("Falha ao salvar o arquivo: " + ex.Message);
            }
            finally
            {
                // Exit from the application
                workbook.Close();
                app.Quit();
                _logRepository.WriteLogDebug("Excel fechado.", LogType.Informacao);
            }
        }
    }
}
