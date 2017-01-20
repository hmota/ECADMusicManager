using Sincronizacao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sincronizacao.UI
{
    public class RowExibicaoAprovacao
    {
        public Exibicao Exibicao { get; set; }
        public bool PreAprovado { get; set; }
        public bool Aprovado { get; set; }
    }
}
