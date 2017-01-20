using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sincronizacao.UI
{
    public class TrilhaImportada
    {
        public string Arquivo { get; set; }
        public DateTime ImportadoEm { get; set; }
        public string Programa { get; set; }
        public string TipoExibicao { get; set; }
        public DateTime ExibidoEm { get; set; }
        public string Unidade { get; set; }
        public string Musica { get; set; }
        public string Autores { get; set; }
        public string Interpretes { get; set; }
        public string Classificacao { get; set; }
        public TimeSpan Minutagem { get; set; }
        public string ISRC { get; set; }
        public int Vetrix { get; set; }
        public string Erro { get; set; }
    }
}