using SincronizacaoMusical.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class EnumRowExibicaoAutorizacao
    {
        public EnumRowExibicaoAutorizacao(string _classificacao, int _musicaID)
        {
            classificacao = _classificacao;
            musicaID = _musicaID;
        }

        public string classificacao { get; set; }
        public int musicaID { get; set; }
        //public RowExibicaoAutorizacao rwa { get; set; }
        //public KeyValuePair<string,int> keyValue{ get; set; }
    }
}
