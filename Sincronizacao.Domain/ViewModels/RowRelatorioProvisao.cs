using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowRelatorioProvisao
    {
        public string Programa  { get; set; } 
        public string Ordem  { get; set; }
        public DateTime Data { get; set; }

        public decimal Performance { get; set; }
        public decimal Adorno { get; set; }
        public decimal Fundo { get; set; }
        public decimal Tema { get; set; }
        public decimal Abertura { get; set; }
        public decimal FundoJornalistico { get; set; }
        public decimal Total { get; set; }
        
        public decimal PerformanceQntd { get; set; }
        public decimal AdornoQntd { get; set; }
        public decimal FundoQntd { get; set; }
        public decimal TemaQntd { get; set; }
        public decimal AberturaQntd { get; set; }
        public decimal FundoJornalisticoQntd { get; set; }
        public decimal TotalQntd { get; set; }
    }
}
