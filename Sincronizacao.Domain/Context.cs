using System.Data.Entity;
using SincronizacaoMusical.Domain.Entities;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Configuration;

namespace SincronizacaoMusical.Domain
{
    public class Context : DbContext
    {
        public DbSet<Associacao> Associacoes { get; set; }
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Album> Albuns { get; set; }
        public DbSet<Autorizacao> Autorizacoes { get; set; }
        public DbSet<Classificacao> Classificacoes { get; set; }
        public DbSet<Editora> Editoras { get; set; }
        public DbSet<Exibicao> Exibicoes { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Gravadora> Gravadoras { get; set; }
        public DbSet<Interprete> Interpretes { get; set; }
        public DbSet<Importacao> Importacoes { get; set; }
        public DbSet<Musica> Musicas { get; set; }
        public DbSet<Novela> Novela { get; set; }
        public DbSet<Sonorizacao> Sonorizacoes { get; set; }
        public DbSet<Preco> Precos { get; set; }
        public DbSet<Programa> Programas { get; set; }
        public DbSet<Quadro> Quadros { get; set; }
        public DbSet<Sincronizacao> Sincronizacoes { get; set; }
        public DbSet<TipoExibicao> TipoExibicoes { get; set; }
        public DbSet<TipoTrilha> TipoTrilhas { get; set; }
        public DbSet<Unidade> Unidades { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vetrix> Vetrix { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }

        public Context()
        {
            string database = ConfigurationManager.AppSettings["database"];
            string connection = ConfigurationManager.ConnectionStrings[database].ConnectionString;
            Database.Connection.ConnectionString = connection;
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<Context, Migrations.Configuration>());
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<Context>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Associacao>().ToTable("Associacoes");
            modelBuilder.Entity<Autor>().ToTable("Autores");
            modelBuilder.Entity<Album>().ToTable("Albuns");
            modelBuilder.Entity<Autorizacao>().ToTable("Autorizacoes");
            modelBuilder.Entity<Classificacao>().ToTable("Classificacoes");
            modelBuilder.Entity<Editora>().ToTable("Editoras");
            modelBuilder.Entity<Exibicao>().ToTable("Exibicoes");
            modelBuilder.Entity<Genero>().ToTable("Generos");
            modelBuilder.Entity<Gravadora>().ToTable("Gravadoras");
            modelBuilder.Entity<Importacao>().ToTable("Importacoes");
            modelBuilder.Entity<Interprete>().ToTable("Interpretes");
            modelBuilder.Entity<Musica>().ToTable("Musicas");
            modelBuilder.Entity<Novela>().ToTable("Novela");
            modelBuilder.Entity<Sonorizacao>().ToTable("Sonorizacoes");
            modelBuilder.Entity<Preco>().ToTable("Precos");
            modelBuilder.Entity<Programa>().ToTable("Programas");
            modelBuilder.Entity<Quadro>().ToTable("Quadros");
            modelBuilder.Entity<Sincronizacao>().ToTable("Sincronizacoes");
            modelBuilder.Entity<TipoExibicao>().ToTable("TipoExibicoes");
            modelBuilder.Entity<TipoTrilha>().ToTable("TipoTrilhas");
            modelBuilder.Entity<Unidade>().ToTable("Unidades");
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Vetrix>().ToTable("Vetrix");
            modelBuilder.Entity<Log>().ToTable("Logs");
            modelBuilder.Entity<Configuracao>().ToTable("Configuracoes");

            //Não cria uma tabela de metadados
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            //Não cria tabelas com nomes pluralizados das entidades
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
