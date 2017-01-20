using System;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Vetrix
    {
        public string Roteiro { get; set; }
        public int UnidadeID { get; set; }
        public int ProgramaID { get; set; }
        public string Processado { get; set; }
        public DateTime ImportadoEm { get; set; }
        public DateTime ExibidoEm { get; set; }
        public int VetrixID { get; set; }
        public int UsuarioID { get; set; }
    }
}
