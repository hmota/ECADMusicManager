using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowRelatorioPgtoAberto
    {
        public string Programa  { get; set; }
        public DateTime Data  { get; set; }
        public string TipoExibicao { get; set; }
        public decimal Porcentagem { get; set; }
        public decimal Valor { get; set; }
        public string Titulo  { get; set; }
        public string Autor { get; set; }
        public string Interprete { get; set; }
        public string Classificacao { get; set; }
        public TimeSpan Minutagem { get; set; }
        public string ISRC { get; set; }
        public string TipoTrilha { get; set; }
        public double Segundos { get; set; }
        public int MusicaID { get; set; }
        public int ClassificacaoID { get; set; }
        public int GeneroID { get; set; }
        public int SonorizacaoID { get; set; }
        public int ProgramaID { get; set; }
    }
}
