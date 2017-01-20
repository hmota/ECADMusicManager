using SincronizacaoMusical.Domain.Entities;

namespace SincronizacaoMusical.Domain.ViewModels
{
    class ViewExibicoes : BaseViewModel
    {
        public ViewExibicoes(Exibicao exibicao, bool preAprovado, bool aprovado )
        {
            Exibicao = exibicao;
            PreAprovado = preAprovado;
            Aprovado = aprovado;
        }
        public Exibicao Exibicao
        {
            
            set { OnPropertyChanged("Exibicao"); }
        }
        public bool PreAprovado
        {
        
            set { OnPropertyChanged("PreAprovado"); }
        }
        public bool Aprovado
        {
      
            set { OnPropertyChanged("Aprovado"); }
        }
    }
}
