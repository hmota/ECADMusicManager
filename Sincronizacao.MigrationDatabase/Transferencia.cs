using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using SincronizacaoMusical.Domain;

namespace SincronizacaoMusical.MigrationDatabase
{
    internal class Transferencia
    {
        //Dicionarios para armazenar chaves da base antiga e as novas chaves geradas pelo EF
        public static Dictionary<int, int> DicAssociacao = new Dictionary<int, int>();
        public static Dictionary<int, int> DicEditora = new Dictionary<int, int>();
        public static Dictionary<int, int> DicTipoExibicao = new Dictionary<int, int>();
        public static Dictionary<int, int> DicGenero = new Dictionary<int, int>();
        public static Dictionary<int, int> DicClassificacao = new Dictionary<int, int>();
        public static Dictionary<int, int> DicPrograma = new Dictionary<int, int>();
        public static Dictionary<int, int> DicMusicaBranca = new Dictionary<int, int>();
        public static Dictionary<int, int> DicMusicaComercial = new Dictionary<int, int>();
        public static Dictionary<int, int> DicMusicaRecord = new Dictionary<int, int>();
        public static Dictionary<int, int> DicUnidade = null;
        public static Dictionary<int, int> DicSincronizacao = null;
        public static Dictionary<int, int> DicExibicao = new Dictionary<int, int>();
        public static Dictionary<int, int> DicAutorizacao = new Dictionary<int, int>();

        public static Dictionary<string, int> DicAlbum = new Dictionary<string, int>();
        public static Dictionary<string, int> DicAutor = new Dictionary<string, int>();
        public static Dictionary<string, int> DicInterprete = new Dictionary<string, int>();

        public static Dictionary<int, int> DicSonorizacao = new Dictionary<int, int>();


        // ReSharper disable UnusedParameter.Local
        private static void Main(string[] args)
            // ReSharper restore UnusedParameter.Local
        {
            Console.WriteLine("########### Migração ###########");
            Console.WriteLine();

            Console.WriteLine("Abrindo base antiga...");
            var oldDB = new SincOldEntities();
            Console.Write("retorno: " + oldDB.SaveChanges() + Environment.NewLine);

            using (var context = new Context())
            {
                Console.WriteLine("Deletando a base " + ConfigurationManager.AppSettings["database"] + "... Existe?");

                Console.Write("retorno: " + context.Database.Delete() + Environment.NewLine);

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Criando a base...");
                context.Database.Create();
            }

            Console.WriteLine("SUCESSO!!!");
            oldDB.Dispose();

            Console.WriteLine("########### Migração ###########");

            //#####################################//
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("TipoTrilha...");
            new MigraTipoTrilha().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine();
            Console.WriteLine("Musica...");
            new MigraMusica().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Usuario...");
            new MigraUsuario().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Associacao...");
            new MigraAssociacao().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine("Editora...");
            new MigraEditora().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Quadro...");
            new MigraQuadro().Migrar();
            Thread.Sleep(1000);
            
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Genero...");
            new MigraGenero().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Unidade...");
            new MigraUnidade().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Classificacao...");
            new MigraClassificacao().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Relacionando: Genero x Classificacao...");
            new MigraGeneroClassificacao().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Preço...");
            new MigraPreco().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Programa...");
            new MigraPrograma().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Tipo Exibicao...");
            new MigraTipoExibicao().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Sincronizacao...");
            new MigraSincronizacao().Migrar();
            Thread.Sleep(1000);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Autorização...");
            new MigraAutorizacao().Migrar();
            Thread.Sleep(1000);

            //#####################################//

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("########### Migração Database completa !!! ###########");
            Thread.Sleep(3000);
        }
    }
}