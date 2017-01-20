using Sincronizacao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sincronizacao.Domain.ViewModels
{
    public class RowExibicaoPagamento
    {
        public int AutID  { get; set; }
        public string Musica  { get; set; }
        public DateTime Vencimento  { get; set; }
        public string Usuario { get; set; }
        public decimal Valor  { get; set; }
        public decimal Porcentagem  { get; set; }
        public string Editora  { get; set; }
        public int AP { get; set; }
    }
}
