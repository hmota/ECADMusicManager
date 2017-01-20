namespace SincronizacaoMusical.Domain.Entities
{
    public class Quadro
    {
        public int QuadroID { get; set; }
        public string Descricao { get; set; }
        public bool Ativo { get; set; }
        public int ProgramaID { get; set; }
    }
}
