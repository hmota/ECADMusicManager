using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Configuracao
    {
        public int ConfiguracaoID { get; set; }
        public string Chave { get; set; }
        public string Valor { get; set; }
    }
}
