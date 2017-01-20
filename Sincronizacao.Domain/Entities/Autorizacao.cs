using System;
namespace SincronizacaoMusical.Domain.Entities
{
    public class Autorizacao
    {
        public int AutorizacaoID { get; set; }
        public int SonorizacaoID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Porcentagem { get; set; }
        public decimal Valor { get; set; }
        public int MusicaID { get; set; }
        public int EditoraID { get; set; }
        public int AP { get; set; }
        public DateTime Vencimento { get; set; }
        public string Arquivo { get; set; }

        public virtual Sonorizacao Sonorizacao { get; set; }
        public virtual Musica Musica { get; set; }
        public virtual Editora Editora { get; set; }
    }
}
