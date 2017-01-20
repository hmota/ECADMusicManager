using System;

namespace SincronizacaoMusical.Domain.Entities
{
    public class VetrixMusica
    {
        public string Musica { get; set; }
        public string Autor { get; set; }
        public string Interprete { get; set; }
        public string TipoTrilha { get; set; }
        public DateTime CadastradaEm { get; set; }
        public string Arquivo { get; set; }
    }
}
