using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SincronizacaoMusical.Domain.Entities
{
    public class Editora
    {
        public Editora()
        {
            Musicas = new List<Musica>();
        }

        public int EditoraID { get; set; }
        public int AssociacaoID { get; set; }
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
        public string CNPJ { get; set; }
        public bool Ativo { get; set; }
        public bool CNPJJ { get; set; }
        
        public virtual Associacao Associacao { get; set; }
        public virtual ICollection<Musica> Musicas { get; set; }
    }
}
