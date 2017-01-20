using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowExibicaoAprovacao
    {
        public Exibicao Exibicao { get; set; }
        public bool PreAprovado { get; set; }
        public bool Aprovado { get; set; }
        public bool Aberto { get; set; }
    }
}
