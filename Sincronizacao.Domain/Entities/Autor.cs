using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Autor
    {
        public Autor()
        {
            Musicas = new List<Musica>();
        }

        public int AutorID { get; set; }
        public string Nome { get; set; }

        public virtual ICollection<Musica> Musicas { get; set; }
    }
}
