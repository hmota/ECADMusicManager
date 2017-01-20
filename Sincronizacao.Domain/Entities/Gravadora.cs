using System.Collections.Generic;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Gravadora
    {
        public int GravadoraID { get; set; }     
        public string Nome { get; set; }
        public string RazaoSocial { get; set; }
        public string Endereco { get; set; }
        public string Bairro { get; set; }
        public string Cep { get; set; }
        public string DDD { get; set; }
        public string Fone { get; set; }
        public string DDD1 { get; set; }
        public string Fone1 { get; set; }
        public string Contato { get; set; }
        public string Contato2 { get; set; }
        public string Email { get; set; }
        public string Email2 { get; set; }
        public string Banco { get; set; }
        public string CPF { get; set; }
        public string Ag { get; set; }
        public string Conta { get; set; }
        public string Complemento { get; set; }
        public string Numero { get; set; }
        public string Cidade { get; set; }
        public string UF { get; set; }       
        public bool Ativo { get; set; }
        public bool CNPJ { get; set; }
        public string CNPJJ { get; set; }
        

        public virtual ICollection<Musica> Musicas { get; set; }
    }
}
