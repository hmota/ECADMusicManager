using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Interprete
    {
        public Interprete()
        {
            Musicas = new List<Musica>();
        }

        public int InterpreteID { get; set; }
        public string Nome { get; set; }

        public virtual ICollection<Musica> Musicas { get; set; }
    }
}
