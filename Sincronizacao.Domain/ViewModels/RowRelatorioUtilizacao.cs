using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowRelatorioUtilizacao
    {
        public string Programa  { get; set; } 
        public string Ordem  { get; set; }
        public string TipoTrilha { get; set; }
        public int Performance { get; set; }
        public int Adorno { get; set; }
        public int Fundo { get; set; }
        public int Tema { get; set; }
        public int Abertura { get; set; }
        public int FundoJornalistico { get; set; }
        public int Total { get; set; }
        public DateTime Data { get; set; }
    }
}
