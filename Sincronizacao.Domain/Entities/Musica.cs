using System;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Musica
    {
        public int MusicaID { get; set; }
        public string Titulo { get; set; }
        public TimeSpan Duracao { get; set; }
        public string ISRC { get; set; }
        public string NomeArquivo { get; set; }
        public DateTime CadastradaEm { get; set; }
        public bool Ativo { get; set; }

        public int? AlbumID { get; set; }
        public int TipoTrilhaID { get; set; }
        public int AutorID { get; set; }
        public int InterpreteID { get; set; }

        public virtual Album Album { get; set; }
        public virtual TipoTrilha TipoTrilha { get; set; }
        public virtual Autor Autor {get;set;}
        public virtual Interprete Interprete { get; set; }
    }
}
