using System;
namespace SincronizacaoMusical.Domain.Entities
{
    public class Sincronizacao
    {
        public int SincronizacaoID { get; set; }
        public int ExibicaoID { get; set; }
        public int UsuarioID { get; set; }
        public string Observacao { get; set; }
        public bool PreAprovado { get; set; }
        public bool Aprovado { get; set; }
        public bool Aberto { get; set; }
        public DateTime AprovadoEm { get; set; }

        public virtual Exibicao Exibicao { get; set; }
        public virtual Usuario Usuario { get; set; }
    }
}
