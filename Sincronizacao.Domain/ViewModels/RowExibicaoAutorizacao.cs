using SincronizacaoMusical.Domain.Entities;
using System;

namespace SincronizacaoMusical.Domain.ViewModels
{
    public class RowExibicaoAutorizacao
    {
        public int AutID  { get; set; }
        public int SonID  { get; set; }
        public int SincID  { get; set; }
        public int MusicaID { get; set; }
        public string Musica  { get; set; }
        public Autor Autor  { get; set; }
        public Interprete Interprete  { get; set; }
        public string Classificacao { get; set; }
        public TimeSpan Minutagem { get; set; }
        public decimal Porcentagem { get; set; }
        public bool Incidental { get; set; }
        public bool PoutPourri{ get; set; }
    }
}
