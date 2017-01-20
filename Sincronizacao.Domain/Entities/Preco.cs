using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Preco
    {
        public int PrecoID { get; set; }
        public int ClassificacaoID { get; set; }
        public int GeneroID { get; set; }
        public int AssociacaoID { get; set; }
        public decimal Valor { get; set; }
        public string Abrangencia { get; set; }
        public int Vigencia { get; set; }
        public bool Ativo { get; set; }

        public virtual Genero Genero { get; set; }
        public virtual Classificacao Classificacao { get; set; }
        public virtual Associacao Associacao { get; set; }
    }
}
