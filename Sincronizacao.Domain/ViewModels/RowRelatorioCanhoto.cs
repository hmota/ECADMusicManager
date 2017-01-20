using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowRelatorioCanhoto
    {
        public DateTime Vencimento  { get; set; }
        public decimal Valor  { get; set; }
        public string Editora  { get; set; }    
    

        public string Programa  { get; set; } 
        public string Ordem  { get; set; }
        public decimal Performance { get; set; }
        public decimal Adorno { get; set; }
        public decimal Fundo { get; set; }
        public decimal Tema { get; set; }
        public decimal Abertura { get; set; }
        public decimal FundoJornalistico { get; set; }
        public decimal Total { get; set; }
    }
}
