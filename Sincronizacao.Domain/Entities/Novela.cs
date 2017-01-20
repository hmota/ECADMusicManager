using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Novela
    {
        public int NovelaID { get; set; }
        public int ProgramaID { get; set; }
        public string TituloOriginal { get; set; }
        public string TituloNacional { get; set; }
        public string Produtor { get; set; }
        public string Autor { get; set; }
        public string Diretor { get; set; }
        public string Pais { get; set; }
        public DateTime DataInicial { get; set; }
        public DateTime DataFinal { get; set; }
        public bool Ativo { get; set; }

        public virtual Programa Programa { get; set; }
    }
}
