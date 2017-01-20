using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Genero
    {
        public Genero()
        {
            Classificacoes = new List<Classificacao>();
        }

        public int GeneroID { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }

        public virtual ICollection<Classificacao> Classificacoes { get; set; }
        public virtual ICollection<Programa> Programas { get; set; }
    }
}
