using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowVetrixErro
    {
        public int VetrixID { get; set; }
        public DateTime Roteiro { get; set; }
        public string Processado { get; set; }
        public DateTime ImportadoEm { get; set; }
        public string Unidade { get; set; }
        public string Programa { get; set; }
        public DateTime Exibicao { get; set; }
        public string Tipo_De_Exibicao { get; set; }
        public string Nome_Da_Musica { get; set; }
        public string Interpretes { get; set; }
        public string Autores { get; set; }
        public string Classificacao { get; set; }
        public TimeSpan Minutagem { get; set; }
        public string Quadro { get; set; }
    }
}
