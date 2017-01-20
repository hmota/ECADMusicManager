using System;

namespace SincronizacaoMusical.Domain
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
        public string Quadro { get; set; }
        public TimeSpan Minutagem { get; set; }
        public string ISRC { get; set; }
        public int Vetrix { get; set; }
        public string CodAlbum { get; set; }
        public string Album { get; set; }
        //Novelas
        public string TituloNacional { get; set; }
        public int Capitulo { get; set; }
        public string Diretor { get; set; }
        public string Produtor { get; set; }
        public string Destinacao { get; set; }
        public TimeSpan Duracao { get; set; }
        public int Segundos { get; set; }
        public int Ordem { get; set; }
        public string Editora { get; set; }
        public string Gravadora { get; set; }

        public string Categoria { get; set; }
        public string Caracteristicas { get; set; }
        
    }
}