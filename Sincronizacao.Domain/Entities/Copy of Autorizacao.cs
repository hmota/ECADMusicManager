using System;
namespace SincronizacaoMusical.Domain.Entities
{
    public class Autorizacaoback
    {
        public int AutorizacaoID { get; set; }
        public int SonorizacaoID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Porcentagem { get; set; }
        public decimal Valor { get; set; }
        public int MusicaID { get; set; }
        public int EditoraID { get; set; }
        public int AP { get; set; } //autorização de pagamento
        public DateTime Vencimento { get; set; }
    }
}
