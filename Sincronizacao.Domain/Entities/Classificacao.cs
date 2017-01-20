using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Classificacao
    {
        public int ClassificacaoID { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        
        public virtual ICollection<Genero> Generos { get; set; }
    }
}
