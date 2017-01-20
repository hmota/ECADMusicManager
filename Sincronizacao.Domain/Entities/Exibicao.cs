using System;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Exibicao
    {
        public int ExibicaoID { get; set; }
        public int ProgramaID { get; set; }
        public int UnidadeID { get; set; }
        public DateTime Data { get; set; }

        public virtual Programa Programa { get; set; }
        public virtual Unidade Unidade { get; set; }
    }
}
