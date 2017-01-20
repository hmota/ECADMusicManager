using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Associacao
    {
        public int AssociacaoID { get; set; }
        public string Nome { get; set; }
        public bool Ativo { get; set; }

        public virtual ICollection<Editora> Editoras { get; set; }
    }
}
