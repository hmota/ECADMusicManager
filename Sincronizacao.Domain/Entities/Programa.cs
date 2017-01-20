using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Programa
    {
        public int ProgramaID { get; set; }
        public int GeneroID { get; set; }         
        public string Nome { get; set; }
        public bool Ativo { get; set; }
        public string Ordem { get; set; }        

        public virtual Genero Genero { get; set; }       
    }
}
