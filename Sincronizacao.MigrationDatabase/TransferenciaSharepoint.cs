using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.MigrationDatabase.SincronizacaoSharepoint;

namespace SincronizacaoMusical.MigrationDatabase
{
    public class TransferenciaSharepoint
    {
        private SincronizacaoMusicalDataContext dataContext;

        public void GerarECAD()
        {
            Console.WriteLine("Deletando Sonorizacoes");
            dataContext = GetClientSharepoint();
            foreach (var item in dataContext.ECAD)
            {

                dataContext.DeleteObject(item);
                dataContext.SaveChanges();
                Console.Write(".");
            }
            Console.WriteLine(Environment.NewLine);
            dataContext.SaveChanges();


            using (var ct = new Context())
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Criando Sonorizacoes:");
                Console.WriteLine(Environment.NewLine);
                var sonorizacoes = ct.Sonorizacoes
                    .Include("Classificacao")
                    .Include("Sincronizacao")
                    .Include("Sincronizacao.Exibicao")
                    .Include("Sincronizacao.Exibicao.Programa")
                    .Include("Musica")
                    .Include("Musica.Autor")
                    .Include("Musica.Interprete")
                    .Include("TipoExibicao")
                    .Where(s=>s.Sincronizacao.Aprovado)
                    .Where(s=>s.Sincronizacao.Aberto == false)
                    .Take(50);

                foreach (var ss in sonorizacoes)
                {
                    var ecad = new ECADItem()
                                   {
                                       SonorizacaoID = ss.SonorizacaoID,
                                       Titulo = ss.Musica.Titulo,
                                       Musica = ss.Musica.Titulo,
                                       Autor = ss.Musica.Autor.Nome,
                                       Classificação = ss.Classificacao.Descricao,
                                       Data = ss.Sincronizacao.Exibicao.Data,
                                       ISRC = ss.Musica.ISRC,
                                       Interprete = ss.Musica.Interprete.Nome,
                                       Minutagem = ss.Minutagem.TotalSeconds,
                                       Programa = ss.Sincronizacao.Exibicao.Programa.Nome,
                                       TIpoDeExibição = ss.TipoExibicao.Descricao
                                   };

                    var ctx = GetClientSharepoint();

                    ctx.AddToECAD(ecad);
                    ctx.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        public void Iniciar()
        {
            Console.WriteLine("########### Migração Sharepoint###########");
            Console.WriteLine();

            //dataContext = GetClientSharepoint();
            GerarECAD();


            //Console.WriteLine("Deletando conteudo das listas..." + Environment.NewLine);

            //Console.WriteLine("Deletando Sonorizacoes");
            //foreach (var item in dataContext.Sonorizacoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();

            //Console.WriteLine("Deletando Autorizacoes");
            //foreach (var item in dataContext.Autorizacoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();

            //Console.WriteLine("Deletando Sincronizacoes");
            //foreach (var item in dataContext.Sincronizacoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();

            //Console.WriteLine("Deletando Associacoes");
            //foreach (var item in dataContext.Associacoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Autores");
            //foreach (var item in dataContext.Autores)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Interpretes");
            //foreach (var item in dataContext.Interpretes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();

            //Console.WriteLine("Deletando Classificacoes");
            //foreach (var item in dataContext.Classificacoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Editoras");
            //foreach (var item in dataContext.Editoras)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Generos");
            //foreach (var item in dataContext.Generos)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando GeneroClassificacao");
            //foreach (var item in dataContext.GeneroClassificacao)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Gravadora");
            //foreach (var item in dataContext.Gravadoras)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Musicas");
            //foreach (var item in dataContext.Musicas)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Precos");
            //foreach (var item in dataContext.Precos)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Programas");
            //foreach (var item in dataContext.Programas)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Quadros");
            //foreach (var item in dataContext.Quadros)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando TipoExibicoes");
            //foreach (var item in dataContext.TipoExibicoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando TipoTrilhas");
            //foreach (var item in dataContext.TipoTrilhas)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Unidades");
            //foreach (var item in dataContext.Unidades)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Usuarios");
            //foreach (var item in dataContext.Usuarios)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();
            //Console.WriteLine("Deletando Importacoes");
            //foreach (var item in dataContext.Importacoes)
            //{
            //    dataContext.DeleteObject(item);
            //    dataContext.SaveChanges();
            //    Console.Write(".");
            //}
            //Console.WriteLine(Environment.NewLine);
            //dataContext.SaveChanges();


            ////Atribui valores zerados a algumas tabelas
            ////Console.WriteLine("zero EditorasItem");
            ////dataContext.AddToEditoras(new EditorasItem {EditoraID = 0});
            ////dataContext.SaveChanges();
            ////Console.WriteLine("zero GravadorasItem");
            ////dataContext.AddToGravadoras(new GravadorasItem {GravadoraID = 0});
            ////dataContext.SaveChanges();
            ////Console.WriteLine("zero ImportacoesItem");
            ////dataContext.AddToImportacoes(new ImportacoesItem {ImportacaoID = 0});
            ////dataContext.SaveChanges();
            ////Console.WriteLine("zero EditorasItem");
            ////dataContext.AddToQuadros(new QuadrosItem {QuadroID = 0});
            ////dataContext.SaveChanges();
            ////Console.WriteLine("zero MusicasItem");
            ////dataContext.AddToMusicas(new MusicasItem {MusicaID = 0});
            ////dataContext.SaveChanges();
            ////Console.WriteLine("zero SincronizacoesItem");
            ////dataContext.AddToSincronizacoes(new SincronizacoesItem {SincronizacaoID = 0});
            ////dataContext.SaveChanges();
            ////Console.WriteLine("zero TipoExibicoesItem");
            ////dataContext.AddToTipoExibicoes(new TipoExibicoesItem {TipoExibicaoID = 0});
            ////dataContext.SaveChanges();

            //Console.WriteLine("SUCESSO!!!");

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Autor...");
            //MigraAutor();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Interprete...");
            //MigraInterprete();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Associacao...");
            //MigraAssociacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Editora...");
            //MigraEditora();
            //Thread.Sleep(1000);

            ////Console.WriteLine(Environment.NewLine);
            ////Console.WriteLine("Gravadora...");
            ////MigraGravadora();
            ////Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Usuario...");
            //MigraUsuario();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Importacao...");
            //MigraImportacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Quadro...");
            //MigraQuadro();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("TipoTrilha...");
            //MigraTipoTrilha();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Genero...");
            //MigraGenero();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Unidade...");
            //MigraUnidade();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Classificacao...");
            //MigraClassificacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Relacionando: Genero x Classificacao...");
            //MigraGeneroClassificacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Preço...");
            //MigraPreco();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Programa...");
            //MigraPrograma();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Tipo Exibicao...");
            //MigraTipoExibicao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Exibicao...");
            //MigraExibicao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Sincronizacao...");
            //MigraSincronizacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Musica...");
            //MigraMusica();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Sonorizacao...");
            //MigraSonorizacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("Autorização...");
            //MigraAutorizacao();
            //Thread.Sleep(1000);

            //Console.WriteLine(Environment.NewLine);
            //Console.WriteLine("########### Migração sharepoint completa !!! ###########");
            //Thread.Sleep(3000);
            //GC.SuppressFinalize(dataContext);
            //Thread.Sleep(3000);
        }

        private void MigraSincronizacao()
        {
            using (var context = new Context())
            {
                foreach (var sonorizacao in context.Sonorizacoes.Take(20))
                {
                    var tipoExibicaoItem = dataContext.TipoExibicoes.Where(te => te.TipoExibicaoID == sonorizacao.TipoExibicaoID).FirstOrDefault();
                    var quadroItem = dataContext.Quadros.Where(q => q.QuadroID == sonorizacao.QuadroID).FirstOrDefault();
                    var importacaoItem = dataContext.Importacoes.Where(i => i.ImportacaoID == sonorizacao.ImportacaoID).FirstOrDefault();
                    var editoraItem = dataContext.Editoras.Where(e => e.ID == 591).FirstOrDefault();
                    var gravadoraItem = dataContext.Gravadoras.Where(g => g.ID == 50).FirstOrDefault();
                    var musicaItem = dataContext.Musicas.Where(m => m.MusicaID == sonorizacao.MusicaID).FirstOrDefault();
                    var sincronizacaoItem = dataContext.Sincronizacoes.Where(s => s.SincronizacaoID == sonorizacao.SincronizacaoID).FirstOrDefault();

                    //SonorizacoesItem son = new SonorizacoesItem();
                    //dataContext.AddToSonorizacoes(son);

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    SonorizacoesItem sonorizacaoItem = new SonorizacoesItem
                    {
                        Título = "SonorizacaoID: " + sonorizacao.SonorizacaoID + " " +
                                 "Alterada: " + sonorizacao.Alterada + " " +
                                 "TipoExibicao: " + sonorizacao.TipoExibicaoID + " " +
                                 "Quadro: " + sonorizacao.QuadroID + " " +
                                 "Importacao: " + sonorizacao.ImportacaoID + " " +
                                 "Editora: " + sonorizacao.EditoraID + " " +
                                 "Gravadora: " + sonorizacao.GravadoraID + " " +
                                 "Musica: " + sonorizacao.MusicaID + " " +
                                 "Sincronizacao: " + sonorizacao.SincronizacaoID + " " +
                                 "Minutagem: " + sonorizacao.Minutagem.TotalMinutes.ToString(),

                        SonorizacaoID = sonorizacao.SonorizacaoID,
                        TipoExibicao = tipoExibicaoItem,
                        TipoExibicaoId = tipoExibicaoItem.ID,
                        Quadro = quadroItem,
                        QuadroId = quadroItem.ID,
                        Importacao = importacaoItem,
                        ImportacaoId = importacaoItem.ID,
                        Editora = editoraItem,
                        EditoraId = editoraItem.ID,
                        Gravadora = gravadoraItem,
                        GravadoraId = gravadoraItem.ID,
                        Musica = musicaItem,
                        MusicaId = musicaItem.ID,
                        Sincronizacao = sincronizacaoItem,
                        Minutagem = sonorizacao.Minutagem.TotalSeconds,
                        Alterada = true.ToString()
                    };

                    dataContext.AddToSonorizacoes(sonorizacaoItem);
                    dataContext.SaveChanges();




                    //obter todas os items de consulta consultas
                    // var tipoExibicaoItem = dataContext.TipoExibicoes.Where(te => te.TipoExibicaoID == sonorizacao.TipoExibicaoID).FirstOrDefault();
                    //var quadroItem = dataContext.Quadros.Where(q => q.QuadroID == sonorizacao.QuadroID).FirstOrDefault();
                    //var importacaoItem = dataContext.Importacoes.Where(i => i.ImportacaoID == sonorizacao.ImportacaoID).FirstOrDefault();
                    //var editoraItem = dataContext.Editoras.Where(e => e.EditoraID == sonorizacao.EditoraID).FirstOrDefault();
                    //var gravadoraItem = dataContext.Gravadoras.Where(g => g.GravadoraID == sonorizacao.GravadoraID).FirstOrDefault();
                    //var musicaItem = dataContext.Musicas.Where(m => m.MusicaID == sonorizacao.MusicaID).FirstOrDefault();
                    // var sincronizacaoItem = dataContext.Sincronizacoes.Where(s => s.SincronizacaoID == sonorizacao.SincronizacaoID).FirstOrDefault();


                    var sonItem = dataContext.Sonorizacoes.Where(s => s.ID == sonorizacaoItem.ID).FirstOrDefault();
                    sonItem.SonorizacaoID = sonorizacao.SonorizacaoID;
                    sonItem.TipoExibicaoId = tipoExibicaoItem.ID;
                    sonItem.QuadroId = quadroItem.ID;
                    sonItem.ImportacaoId = importacaoItem.ID;
                    sonItem.EditoraId = editoraItem.ID;
                    //sonItem.GravadoraId = gravadoraItem.ID;
                    sonItem.MusicaId = musicaItem.ID;
                    sonItem.SincronizacaoId = sincronizacaoItem.ID;

                    //atualizar na lista
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    SonorizacoesItem sonorizacaoCompletoItem = dataContext.Sonorizacoes.Where(s => s.ID == sonorizacaoItem.ID).FirstOrDefault();

                    //corrigir informacoes e salvar
                    sonorizacaoCompletoItem.SonorizacaoID = sonorizacao.SonorizacaoID;
                    sonorizacaoCompletoItem.Alterada = sonorizacao.Alterada.ToString();
                    sonorizacaoCompletoItem.Editora = editoraItem;
                    sonorizacaoCompletoItem.EditoraId = (int)editoraItem.EditoraID;
                    //sonorizacaoCompletoItem.Gravadora = gravadoraItem;
                    //sonorizacaoCompletoItem.GravadoraId = (int)gravadoraItem.GravadoraID;
                    sonorizacaoCompletoItem.Importacao = importacaoItem;
                    sonorizacaoCompletoItem.ImportacaoId = sonorizacao.ImportacaoID;
                    sonorizacaoCompletoItem.Minutagem = sonorizacao.Minutagem.TotalSeconds;
                    sonorizacaoCompletoItem.Musica = musicaItem;
                    sonorizacaoCompletoItem.MusicaId = (int)musicaItem.MusicaID;
                    sonorizacaoCompletoItem.Quadro = quadroItem;
                    sonorizacaoCompletoItem.QuadroId = (int)quadroItem.QuadroID;
                    sonorizacaoCompletoItem.Sincronizacao = sincronizacaoItem;
                    sonorizacaoCompletoItem.SincronizacaoId = (int)sincronizacaoItem.SincronizacaoID;
                    sonorizacaoCompletoItem.SonorizacaoID = sonorizacao.SonorizacaoID;
                    sonorizacaoCompletoItem.TipoExibicao = tipoExibicaoItem;
                    sonorizacaoCompletoItem.TipoExibicaoId = (int)tipoExibicaoItem.TipoExibicaoID;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraAutorizacao()
        {
            using (var context = new Context())
            {
                foreach (var autorizacao in context.Autorizacoes.Take(20))
                {
                    //obter todas os items de consulta consultas
                    var editoraItem = dataContext.Editoras.Where(e => e.EditoraID == autorizacao.EditoraID).FirstOrDefault();
                    var sonorizacaoItem = dataContext.Sonorizacoes.Where(s => s.SonorizacaoID == autorizacao.SonorizacaoID).FirstOrDefault();
                  
                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    var autorizacaoItem = new AutorizacoesItem()
                    {
                        AutorizacaoID = autorizacao.MusicaID,
                        EditoraId = editoraItem.ID,
                        SonorizacaoId = sonorizacaoItem.ID
                    };

                    //salvar na lista
                    dataContext.AddToAutorizacoes(autorizacaoItem);
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    AutorizacoesItem autorizacaoCompletoItem = dataContext.Autorizacoes.Where(a => a.ID == autorizacaoItem.ID).FirstOrDefault();

                    //corrigir informacoes e salvar
                    autorizacaoCompletoItem.AutorizacaoID = autorizacao.AutorizacaoID;
                    autorizacaoCompletoItem.AP = autorizacao.AP;
                    autorizacaoCompletoItem.Editora = editoraItem;
                    autorizacaoCompletoItem.EditoraId = (int)editoraItem.EditoraID;
                    autorizacaoCompletoItem.Porcentagem = (double)autorizacao.Porcentagem;
                    //autorizacaoCompletoItem.Sonorizacao = sonorizacaoItem;
                    autorizacaoCompletoItem.SonorizacaoId = (int)sonorizacaoItem.SonorizacaoID;
                    autorizacaoCompletoItem.Valor = (double)autorizacao.Valor;
                    autorizacaoCompletoItem.Vencimento = autorizacao.Vencimento;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraSonorizacao()
        {
            using (var context = new Context())
            {
                foreach (var sonorizacao in context.Sonorizacoes.Take(20))
                {
                    var tipoExibicaoItem = dataContext.TipoExibicoes.Where(te => te.TipoExibicaoID == sonorizacao.TipoExibicaoID).FirstOrDefault();
                    var quadroItem = dataContext.Quadros.Where(q => q.QuadroID == sonorizacao.QuadroID).FirstOrDefault();
                    var importacaoItem = dataContext.Importacoes.Where(i => i.ImportacaoID == sonorizacao.ImportacaoID).FirstOrDefault();
                    var editoraItem = dataContext.Editoras.Where(e => e.ID == 591).FirstOrDefault();
                    var gravadoraItem = dataContext.Gravadoras.Where(g => g.ID == 50).FirstOrDefault();
                    var musicaItem = dataContext.Musicas.Where(m => m.MusicaID == sonorizacao.MusicaID).FirstOrDefault();
                    var sincronizacaoItem = dataContext.Sincronizacoes.Where(s => s.SincronizacaoID == sonorizacao.SincronizacaoID).FirstOrDefault();

                    //SonorizacoesItem son = new SonorizacoesItem();
                    //dataContext.AddToSonorizacoes(son);

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    SonorizacoesItem sonorizacaoItem = new SonorizacoesItem
                    {
                        Título = "SonorizacaoID: " + sonorizacao.SonorizacaoID + " " +
                                 "Alterada: " + sonorizacao.Alterada + " " +
                                 "TipoExibicao: " + sonorizacao.TipoExibicaoID + " " +
                                 "Quadro: " + sonorizacao.QuadroID + " " +
                                 "Importacao: " + sonorizacao.ImportacaoID + " " +
                                 "Editora: " + sonorizacao.EditoraID + " " +
                                 "Gravadora: " + sonorizacao.GravadoraID + " " +
                                 "Musica: " + sonorizacao.MusicaID + " " +
                                 "Sincronizacao: " + sonorizacao.SincronizacaoID + " " +
                                 "Minutagem: " + sonorizacao.Minutagem.TotalMinutes.ToString(),

                                 SonorizacaoID = sonorizacao.SonorizacaoID,
                                 TipoExibicao = tipoExibicaoItem,
                                 TipoExibicaoId = tipoExibicaoItem.ID,
                                 Quadro = quadroItem,
                                 QuadroId = quadroItem.ID,
                                 Importacao = importacaoItem,
                                 ImportacaoId = importacaoItem.ID,
                                 Editora = editoraItem,
                                 EditoraId = editoraItem.ID,
                                 Gravadora = gravadoraItem,
                                 GravadoraId = gravadoraItem.ID,
                                 Musica = musicaItem,
                                 MusicaId = musicaItem.ID,
                                 Sincronizacao = sincronizacaoItem,
                                 Minutagem = sonorizacao.Minutagem.TotalSeconds,
                                 Alterada = true.ToString()
                    };

                    dataContext.AddToSonorizacoes(sonorizacaoItem);
                    dataContext.SaveChanges();

                   


                    //obter todas os items de consulta consultas
                    // var tipoExibicaoItem = dataContext.TipoExibicoes.Where(te => te.TipoExibicaoID == sonorizacao.TipoExibicaoID).FirstOrDefault();
                    //var quadroItem = dataContext.Quadros.Where(q => q.QuadroID == sonorizacao.QuadroID).FirstOrDefault();
                    //var importacaoItem = dataContext.Importacoes.Where(i => i.ImportacaoID == sonorizacao.ImportacaoID).FirstOrDefault();
                    //var editoraItem = dataContext.Editoras.Where(e => e.EditoraID == sonorizacao.EditoraID).FirstOrDefault();
                    //var gravadoraItem = dataContext.Gravadoras.Where(g => g.GravadoraID == sonorizacao.GravadoraID).FirstOrDefault();
                    //var musicaItem = dataContext.Musicas.Where(m => m.MusicaID == sonorizacao.MusicaID).FirstOrDefault();
                   // var sincronizacaoItem = dataContext.Sincronizacoes.Where(s => s.SincronizacaoID == sonorizacao.SincronizacaoID).FirstOrDefault();
                    

                    var sonItem = dataContext.Sonorizacoes.Where(s => s.ID == sonorizacaoItem.ID).FirstOrDefault();
                    sonItem.SonorizacaoID = sonorizacao.SonorizacaoID;
                    sonItem.TipoExibicaoId = tipoExibicaoItem.ID;
                    sonItem.QuadroId = quadroItem.ID;
                    sonItem.ImportacaoId = importacaoItem.ID;
                    sonItem.EditoraId = editoraItem.ID;
                    //sonItem.GravadoraId = gravadoraItem.ID;
                    sonItem.MusicaId = musicaItem.ID;
                    sonItem.SincronizacaoId = sincronizacaoItem.ID;

                    //atualizar na lista
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    SonorizacoesItem sonorizacaoCompletoItem = dataContext.Sonorizacoes.Where(s => s.ID == sonorizacaoItem.ID).FirstOrDefault();

                    //corrigir informacoes e salvar
                    sonorizacaoCompletoItem.SonorizacaoID = sonorizacao.SonorizacaoID;
                    sonorizacaoCompletoItem.Alterada = sonorizacao.Alterada.ToString();
                    sonorizacaoCompletoItem.Editora = editoraItem;
                    sonorizacaoCompletoItem.EditoraId = (int)editoraItem.EditoraID;
                    //sonorizacaoCompletoItem.Gravadora = gravadoraItem;
                    //sonorizacaoCompletoItem.GravadoraId = (int)gravadoraItem.GravadoraID;
                    sonorizacaoCompletoItem.Importacao = importacaoItem;
                    sonorizacaoCompletoItem.ImportacaoId = sonorizacao.ImportacaoID;
                    sonorizacaoCompletoItem.Minutagem = sonorizacao.Minutagem.TotalSeconds;
                    sonorizacaoCompletoItem.Musica = musicaItem;
                    sonorizacaoCompletoItem.MusicaId = (int)musicaItem.MusicaID;
                    sonorizacaoCompletoItem.Quadro = quadroItem;
                    sonorizacaoCompletoItem.QuadroId = (int)quadroItem.QuadroID;
                    sonorizacaoCompletoItem.Sincronizacao = sincronizacaoItem;
                    sonorizacaoCompletoItem.SincronizacaoId = (int)sincronizacaoItem.SincronizacaoID;
                    sonorizacaoCompletoItem.SonorizacaoID = sonorizacao.SonorizacaoID;
                    sonorizacaoCompletoItem.TipoExibicao = tipoExibicaoItem;
                    sonorizacaoCompletoItem.TipoExibicaoId = (int)tipoExibicaoItem.TipoExibicaoID;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraMusica()
        {
            using (var context = new Context())
            {
                foreach (var musica in context.Musicas.Take(20))
                {
                    //obter todas os items de consulta consultas
                    var autorItem = dataContext.Autores.Where(a => a.AutorID == musica.AutorID).FirstOrDefault();
                    var tipoTrilhaItem = dataContext.TipoTrilhas.Where(tt => tt.TipoTrilhaID == musica.TipoTrilhaID).FirstOrDefault();
                    var interpreteItem = dataContext.Interpretes.Where(i => i.InterpreteID == musica.InterpreteID).FirstOrDefault();

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    var musicaItem = new MusicasItem()
                    {
                        MusicaID = musica.MusicaID,
                        TipoTrilhaId = tipoTrilhaItem.ID,
                        AutorId = autorItem.ID,
                        InterpreteId = interpreteItem.ID
                    };

                    //salvar na lista
                    dataContext.AddToMusicas(musicaItem);
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    MusicasItem musicaCompletoItem = dataContext.Musicas.Where(m => m.ID == musicaItem.ID).FirstOrDefault();

                    //corrigir informacoes e salvar
                    musicaCompletoItem.Autor = autorItem;
                    musicaCompletoItem.AutorId = (int)autorItem.AutorID;
                    musicaCompletoItem.ISRC = musica.ISRC;
                    musicaCompletoItem.ISWC = musica.ISWC;
                    musicaCompletoItem.Interprete = interpreteItem;
                    musicaCompletoItem.InterpreteId = (int)interpreteItem.InterpreteID;
                    musicaCompletoItem.MusicaID = musica.MusicaID;
                    musicaCompletoItem.CadastradaEm = musica.CadastradaEm;
                    musicaCompletoItem.Duracao = musica.Duracao.TotalSeconds;
                    musicaCompletoItem.NomeArquivo = musica.NomeArquivo;
                    musicaCompletoItem.TipoTrilha = tipoTrilhaItem;
                    musicaCompletoItem.TipoTrilhaId = (int)tipoTrilhaItem.TipoTrilhaID;
                    musicaCompletoItem.Título = musica.Titulo;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        

        private void MigraExibicao()
        {
            using (var context = new Context())
            {
                foreach (var exibicao in context.Exibicoes.Take(20))
                {
                    //obter todas os items de consulta consultas
                    var programaItem = dataContext.Programas.Where(p => p.ProgramaID == exibicao.ProgramaID).FirstOrDefault();
                    var unidadeItem = dataContext.Unidades.Where(u => u.UnidadeID== exibicao.UnidadeID).FirstOrDefault();

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    var exibicaoItem = new ExibicoesItem()
                    {
                        ExibicaoID = exibicao.ExibicaoID,
                        ProgramaId = programaItem.ID,
                        UnidadeId = unidadeItem.ID
                    };

                    //salvar na lista
                    dataContext.AddToExibicoes(exibicaoItem);
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    ExibicoesItem exibicaoCompletoItem = dataContext.Exibicoes.Where(e => e.ID == exibicaoItem.ID).FirstOrDefault();

                    //corrigir informacoes e salvar
                    exibicaoCompletoItem.ExibicaoID = exibicao.ExibicaoID;
                    exibicaoCompletoItem.Programa = programaItem;
                    exibicaoCompletoItem.ProgramaId = (int)programaItem.ProgramaID;
                    exibicaoCompletoItem.Unidade = unidadeItem;
                    exibicaoCompletoItem.UnidadeId = (int)unidadeItem.UnidadeID;
                    exibicaoCompletoItem.Data = exibicao.Data;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraPrograma()
        {
            using (var context = new Context())
            {
                foreach (var programa in context.Programas.Take(20))
                {
                    //obter todas os items de consulta consultas
                    var generoItem = dataContext.Generos.Where(g => g.GeneroID == programa.GeneroID).FirstOrDefault();

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    var programaItem = new ProgramasItem()
                    {
                        ProgramaID = programa.ProgramaID,
                        GeneroId = generoItem.ID,
                    };

                    //salvar na lista
                    dataContext.AddToProgramas(programaItem);
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    ProgramasItem programaCompletoItem = dataContext.Programas.Where(p => p.ID == programaItem.ID).FirstOrDefault();
                    
                    //corrigir informacoes e salvar
                    programaCompletoItem.Ativo = programa.Ativo;
                    programaCompletoItem.Genero = generoItem;
                    programaCompletoItem.GeneroId = (int)generoItem.GeneroID;
                    programaCompletoItem.Nome = programa.Nome;
                    programaCompletoItem.Ordem = programa.Ordem;
                    programaCompletoItem.ProgramaID = programa.ProgramaID;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraPreco()
        {
            using (var context = new Context())
            {
                foreach (var preco in context.Precos.Take(20))
                {
                    //obter todas os items de consulta consultas
                    // ReSharper disable ReplaceWithSingleCallToFirstOrDefault
                    var classificacaoItem = dataContext.Classificacoes.Where(c => c.ClassificacaoID == preco.ClassificacaoID).FirstOrDefault();
                    var generoItem = dataContext.Generos.Where(g => g.GeneroID == preco.GeneroID).FirstOrDefault();
                    var associcaoItem = dataContext.Associacoes.Where(a => a.AssociacaoID== preco.AssossiacaoID).FirstOrDefault();
                    // ReSharper restore ReplaceWithSingleCallToFirstOrDefault

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    var precoItem = new PrecosItem()
                    {
                        PrecoID= preco.PrecoID,
                        ClassificacaoId = classificacaoItem.ID,
                        GeneroId = generoItem.ID,
                        AssociacaoId = associcaoItem.ID
                    };

                    //salvar na lista
                    dataContext.AddToPrecos(precoItem);
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    PrecosItem precoCompletoItem =
                        dataContext.Precos.Where(p => p.ID == precoItem.ID).FirstOrDefault();
                    //corrigir informacoes e salvar
                    precoCompletoItem.Abrangencia = preco.Abrangencia;
                    precoCompletoItem.Associacao = associcaoItem;
                    precoCompletoItem.AssociacaoId = (int)associcaoItem.AssociacaoID;
                    precoCompletoItem.Classificacao = classificacaoItem;
                    precoCompletoItem.ClassificacaoId = (int)classificacaoItem.ClassificacaoID;
                    precoCompletoItem.Genero = generoItem;
                    precoCompletoItem.GeneroId = (int)generoItem.GeneroID;
                    precoCompletoItem.PrecoID = preco.PrecoID;
                    precoCompletoItem.Valor = (int)preco.Valor;
                    precoCompletoItem.Vigencia = preco.Vigencia;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraGeneroClassificacao()
        {
            using (var context = new Context())
            {
                IEnumerable<Genero> generos = context.Generos.ToList();
                foreach (var genero in generos)
                {
                    IEnumerable<Classificacao> classificacoes = genero.Classificacoes.ToList();

                    foreach (var classificacao in classificacoes)
                    {

                        // ReSharper disable ReplaceWithSingleCallToFirstOrDefault
                        var classificacaoItem =
                            dataContext.Classificacoes.Where(c => c.ClassificacaoID == classificacao.ClassificacaoID)
                                       .FirstOrDefault();
                        var generoItem = dataContext.Generos.Where(g => g.GeneroID == genero.GeneroID).FirstOrDefault();
                        // ReSharper restore ReplaceWithSingleCallToFirstOrDefault

                        var genclass = new GeneroClassificacaoItem
                                           {
                                               Título = classificacaoItem.ID + " " + generoItem.ID,
                                               ClassificacaoId = classificacaoItem.ID,
                                               GeneroId = generoItem.ID
                                           };

                        dataContext.AddToGeneroClassificacao(genclass);
                        dataContext.SaveChanges();




                        GeneroClassificacaoItem generoClassificacao =
                            dataContext.GeneroClassificacao.Where(g => g.ID == genclass.ID).FirstOrDefault();

                        generoClassificacao.Classificacao = classificacaoItem;
                        generoClassificacao.Genero = generoItem;
                        generoClassificacao.ClassificacaoId = classificacao.ClassificacaoID;
                        generoClassificacao.GeneroId = genero.GeneroID;

                        dataContext.SaveChanges();

                        Console.Write(".");
                    }
                }
            }
        }

        private void MigraEditora()
        {
            using (var context = new Context())
            {
                foreach (var editora in context.Editoras.Take(20))
                {
                    var associacaoItem = (from ass in dataContext.Associacoes
                                          where
                                              ass.AssociacaoID ==
                                              (double)editora.AssociacaoID
                                          select ass).FirstOrDefault();
                    var editItem = new EditorasItem()
                    {
                        EditoraID = associacaoItem.ID,
                        AssociacaoId = associacaoItem.ID,
                        CNPJ = editora.CNPJ,
                        Contato = editora.Contato,
                        Email = editora.Email,
                        Endereco = editora.Endereco,
                        Nome = editora.Nome,
                        RazaoSocial = editora.RazaoSocial
                    };
                    dataContext.AddToEditoras(editItem);
                    dataContext.SaveChanges();

                    EditorasItem editoraItem = dataContext.Editoras.Where(e => e.ID == editItem.ID).FirstOrDefault();
                    editoraItem.Associacao = associacaoItem;
                    editoraItem.AssociacaoId = editora.AssociacaoID;
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraImportacao()
        {
            using (var context = new Context())
            {
                foreach (var importacao in context.Importacoes.Take(20))
                {
                    //obter todas os items de consulta consultas
                    var usuarioItem = dataContext.Usuarios.Where(u => u.UsuarioID == importacao.UsuarioID).FirstOrDefault();

                    //criar objeto para ser salvo na lista com algumas informacoes falsas, tendo somente ID e consultas
                    var importacaoItem = new ImportacoesItem()
                    {
                        ImportacaoID = importacao.ImportacaoID,
                        UsuarioId = usuarioItem.ID
                    };

                    //salvar na lista
                    dataContext.AddToImportacoes(importacaoItem);
                    dataContext.SaveChanges();

                    //recuperar o objeto
                    ImportacoesItem importacaoCompletoItem = dataContext.Importacoes.Where(i => i.ID == importacaoItem.ID).FirstOrDefault();

                    //corrigir informacoes e salvar
                    importacaoCompletoItem.ImportacaoID = importacao.ImportacaoID;
                    importacaoCompletoItem.Arquivo = importacao.Arquivo;
                    importacaoCompletoItem.ImportadoEm = importacao.ImportadoEm;
                    importacaoCompletoItem.ImportadoVetrix = importacao.ImportadoVetrix;
                    importacaoCompletoItem.Processado = importacao.Processado;
                    importacaoCompletoItem.Usuario = usuarioItem;
                    importacaoCompletoItem.UsuarioId = (int)usuarioItem.UsuarioID;

                    dataContext.SaveChanges();

                    Console.Write(".");
                }
            }
        }

        private void MigraGravadora()
        {
            using (var context = new Context())
            {
                foreach (var gravadora in context.Gravadoras.Take(20))
                {
                    dataContext.AddToGravadoras(new GravadorasItem()
                                                    {
                                                        GravadoraID = gravadora.GravadoraID,
                                                        CNPJ = gravadora.CNPJ,
                                                        Contato = gravadora.Contato,
                                                        Email = gravadora.Email,
                                                        Endereco = gravadora.Endereco,
                                                        Nome = gravadora.Nome,
                                                    }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraTipoExibicao()
        {
            using (var context = new Context())
            {
                foreach (var tipoExibicao in context.TipoExibicoes.Take(20))
                {
                    dataContext.AddToTipoExibicoes(new TipoExibicoesItem()
                                                       {
                                                           TipoExibicaoID = tipoExibicao.TipoExibicaoID,
                                                           Descricao = tipoExibicao.Descricao
                                                       }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraClassificacao()
        {
            using (var context = new Context())
            {
                foreach (var classificacao in context.Classificacoes.Take(20))
                {
                    dataContext.AddToClassificacoes(new ClassificacoesItem()
                                                        {
                                                            ClassificacaoID = classificacao.ClassificacaoID,
                                                            Descricao = classificacao.Descricao
                                                        }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraUnidade()
        {
            using (var context = new Context())
            {
                foreach (var unidade in context.Unidades.Take(20))
                {
                    dataContext.AddToUnidades(new UnidadesItem()
                                                  {
                                                      UnidadeID = unidade.UnidadeID,
                                                      CNPJ = unidade.CNPJ,
                                                      Contato = unidade.Contato,
                                                      CEP = unidade.CEP,
                                                      Logradouro = unidade.Logradouro,
                                                      Cidade = unidade.Cidade,
                                                      Descricao = unidade.Descricao,
                                                      Telefone = unidade.Telefone,
                                                      UF = unidade.UF,
                                                      Nome = unidade.Nome,
                                                      RazaoSocial = unidade.RazaoSocial
                                                  }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraGenero()
        {
            using (var context = new Context())
            {
                foreach (var genero in context.Generos.Take(20))
                {
                    dataContext.AddToGeneros(new GenerosItem()
                                                 {
                                                     GeneroID = genero.GeneroID,
                                                     Descricao = genero.Descricao
                                                 }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraTipoTrilha()
        {
            using (var context = new Context())
            {
                foreach (var tipoTrilha in context.TipoTrilhas.Take(20))
                {
                    dataContext.AddToTipoTrilhas(new TipoTrilhasItem()
                                                     {
                                                         TipoTrilhaID = tipoTrilha.TipoTrilhaID,
                                                         Descricao = tipoTrilha.Descricao
                                                     }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraQuadro()
        {
            using (var context = new Context())
            {
                foreach (var quadro in context.Quadros.Take(20))
                {
                    dataContext.AddToQuadros(new QuadrosItem()
                                                 {
                                                     QuadroID = quadro.QuadroID,
                                                     Ativo = quadro.Ativo,
                                                     Descricao = quadro.Descricao
                                                 }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraUsuario()
        {
            using (var context = new Context())
            {
                foreach (var usuario in context.Usuarios.Take(20))
                {
                    dataContext.AddToUsuarios(new UsuariosItem()
                                                  {
                                                      UsuarioID = usuario.UsuarioID,
                                                      Administrador = usuario.Administrador,
                                                      Analista = usuario.Analista,
                                                      Login = usuario.Login,
                                                      Supervisor = usuario.Supervisor
                                                  }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraAssociacao()
        {
            using (var context = new Context())
            {
                foreach (var associacao in context.Associacoes.Take(20))
                {
                    dataContext.AddToAssociacoes(new AssociacoesItem()
                                                     {
                                                         AssociacaoID = associacao.AssociacaoID,
                                                         Nome = associacao.Nome
                                                     }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraInterprete()
        {
            using (var context = new Context())
            {
                foreach (var interprete in context.Interpretes.Take(20))
                {
                    dataContext.AddToInterpretes(new InterpretesItem()
                    {
                        InterpreteID = interprete.InterpreteID,
                        Nome = interprete.Nome
                    }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        private void MigraAutor()
        {
            using (var context = new Context())
            {
                foreach (var autor in context.Autores.Take(20))
                {
                    dataContext.AddToAutores(new AutoresItem()
                    {
                        AutorID = autor.AutorID,
                        Nome = autor.Nome
                    }
                        );
                    dataContext.SaveChanges();
                    Console.Write(".");
                }
            }
        }

        public SincronizacaoMusicalDataContext GetClientSharepoint()
        {
            var webClient = new WebClient
                                {
                                    UseDefaultCredentials = true,
                                    Credentials =
                                        new NetworkCredential("hmota",
                                                              ConfigurationManager.AppSettings["pk"].ToString(),
                                                              "REDERECORD")
                                };
            var dataContext =
                new SincronizacaoMusicalDataContext(new Uri("http://xisto02/musical/_vti_bin/listdata.svc"))
                    {
                        Credentials = webClient.Credentials
                    };

            return dataContext;
        }
    }
}