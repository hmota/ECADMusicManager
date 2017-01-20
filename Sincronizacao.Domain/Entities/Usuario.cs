namespace SincronizacaoMusical.Domain.Entities
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Login { get; set; }
        public bool Analista { get; set; }
        public bool Supervisor { get; set; }
        public bool Administrador { get; set; }
    }
}
