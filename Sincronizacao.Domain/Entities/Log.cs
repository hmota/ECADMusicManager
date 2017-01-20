using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Log
    {
        public Log()
        {
            this.Time = DateTime.Now;
        }

        public int LogID { get; set; }

        public string Message { get; set; }

        public LogType Type { get; set; }

        public DateTime Time { get; internal set; }

        public string NomeObjeto { get; set; }

        public int ObjetoID { get; set; }

        public string Descricao { get; set; }

        public AcaoType Acao { get; set; }

        public string ValorNovo { get; set; }

        public string ValorAntigo { get; set; }

        public string Usuario { get; set; }

        public string Sistema { get; set; }

        public string Versao { get; set; }

        public bool Debug { get; set; }
    }
}
