using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowRelatorioECAD
    {
        public string Programa  { get; set; }
        public DateTime Data  { get; set; }
        public string TipoExibicao { get; set; }
        public string Titulo  { get; set; }
        public string Autor { get; set; }
        public string Interprete { get; set; }
        public string Classificacao { get; set; }
        public TimeSpan Minutagem { get; set; }
        public string ISRC { get; set; }
        public string TipoTrilha { get; set; }
        public double Segundos { get; set; }        
    }
}
