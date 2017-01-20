using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Importacao
    {
        public int ImportacaoID { get; set; }
        public string Arquivo { get; set; }
        public bool Processado { get; set; }
        public DateTime ImportadoEm { get; set; }
        public int UsuarioID { get; set; }
        public bool ImportadoVetrix { get; set; }
    }
}
