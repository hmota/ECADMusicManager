using System;
namespace SincronizacaoMusical.Domain.Entities
{
    public class Sonorizacao
    {
        public int SonorizacaoID { get; set; }

        public string Execucao { get; set; }
        public TimeSpan Minutagem { get; set; }
        public TimeSpan Captacao { get; set; }
        public bool Alterada { get; set; }
        public string AlteradaPor { get; set; }

        public int SincronizacaoID { get; set; }
        public int MusicaID { get; set; }
        public int ClassificacaoID { get; set; }
        public int QuadroID { get; set; }
        public int TipoExibicaoID { get; set; }
        public int ImportacaoID { get; set; }
        public int EditoraID { get; set; }
        public int GravadoraID { get; set; }

        public virtual Sincronizacao Sincronizacao { get; set; }
        public virtual Musica Musica { get; set; }
        public virtual Classificacao Classificacao { get; set; }
        public virtual Quadro Quadro { get; set; }
        public virtual TipoExibicao TipoExibicao { get; set; }
        public virtual Importacao Importacao { get; set; }
        //public virtual Editora Editora { get; set; }
        //public virtual Gravadora Gravadora { get; set; }
    }
}
