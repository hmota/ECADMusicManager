using System;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Deployment.Application;
using Microsoft.Office.Interop.Excel;
using SincronizacaoMusical.Domain.Business;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Domain;
using SincronizacaoMusical.Data.Repositories;
using SincronizacaoMusical.Infrastructure.Data;
using SincronizacaoMusical.Util;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using TabControl = System.Windows.Controls.TabControl;
using TextBox = System.Windows.Controls.TextBox;
using ToolBar = System.Windows.Controls.ToolBar;
using SincronizacaoMusical.Domain.Repositories;
using Application = System.Windows.Application;
using ListBox = System.Windows.Controls.ListBox;
using Usuario = SincronizacaoMusical.Domain.Entities.Usuario;
using Window = System.Windows.Window;
using System.Windows.Threading;
using ComboBox = System.Windows.Controls.ComboBox;

namespace SincronizacaoMusical.UI
{
    /// <summary>
    /// Interação principal do sistema
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILogRepository _logRepository;

        private readonly Usuario _usuario;
            

        private string _unidade = "";
        private Sincronizacao _sincSel = null;
        private RowExibicaoAprovacao _exibSel = null;
        private int _genAutoSel = 0;
        private int _totalImportadasMusicas;
        private int _totalImportadasSucesso;
        private int _totalImportadasErros;
        private int _musEditSel;
        private int _exbEditSel;
        private bool _isLoaded;
        //pagamentos
        private decimal _totalPorcentagem = 0;
        private decimal _total;
        private int _genID = 0;
        private int _sincID = 0;
        private int _sonID = 0;
        private int _musID = 0;

        private string _pastaExportacao = ConfigurationManager.AppSettings["PastaExportacao"];

        private List<VetrixMusica> _MusicasParaEmail;

        private Context context = null;
        public int UnidadeID = 0;

        private List<ComboBoxPairs> _meses;
        private List<ComboBoxPairs> _anos;
        private List<Editora> _editoras;
        private List<TipoTrilha> _tipoTrilhas;
        private List<Genero> _generos;

        private Dictionary<string, object> Items;
        private Dictionary<string, object> SelectedItems;

        /// <summary>
        /// Inicializa parte das listas usadas nas combos da tela
        /// </summary>
        private void CarregaListasBasicas()
        {
            Items = new Dictionary<string, object>();
            Items.Add("Chennai", "MAS");
            Items.Add("Trichy", "TPJ");
            Items.Add("Bangalore", "SBC");
            Items.Add("Coimbatore", "CBE");

            SelectedItems = new Dictionary<string, object>();
            SelectedItems.Add("Chennai", "MAS");
            SelectedItems.Add("Trichy", "TPJ");

            _meses = new List<ComboBoxPairs>
                         {
                             new ComboBoxPairs("Janeiro", 01),
                             new ComboBoxPairs("Fevereiro", 02),
                             new ComboBoxPairs("Março", 03),
                             new ComboBoxPairs("Abril", 04),
                             new ComboBoxPairs("Maio", 05),
                             new ComboBoxPairs("Junho", 06),
                             new ComboBoxPairs("Julho", 07),
                             new ComboBoxPairs("Agosto", 08),
                             new ComboBoxPairs("Setembro", 09),
                             new ComboBoxPairs("Outubro", 10),
                             new ComboBoxPairs("Novembro", 11),
                             new ComboBoxPairs("Dezembro", 12)
                         };

            _anos = new List<ComboBoxPairs>();
            for (int year = 2010; year <= DateTime.Now.Year; year++)
            {
                _anos.Add(new ComboBoxPairs(year.ToString(), year));
            }

            _editoras = context.Editoras.OrderBy(e => e.Nome).AsNoTracking().ToList();
            var editoraDefault = new Editora() {Nome = "--Todos--", EditoraID = 0};
            _editoras.Insert(0, editoraDefault);

            _tipoTrilhas = context.TipoTrilhas.OrderBy(tt => tt.Descricao).AsNoTracking().ToList();
            var tipoTrilhaDefault = new TipoTrilha() {Descricao = "--Todos--", TipoTrilhaID = 0};
            _tipoTrilhas.Insert(0, tipoTrilhaDefault);

            _generos = new Repositorio().Obter<Genero>().OrderBy(g => g.Descricao).AsNoTracking().ToList();
            var generoDefault = new Genero() {Descricao = "--Todos--", GeneroID = 0};
            _generos.Insert(0, generoDefault);
        }

        /// <summary>
        /// Inicializa os componentes do sistema.
        /// </summary>
        public MainWindow()
        {
            context = SingletonContext.Instance.Context;

            _logRepository = new DatabaseLogRepository();

            try
            {
            _usuario = new VerificarUsuario().ObterUsuario(ConfigurationManager.AppSettings["Login"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao logar com o usuário: " + ConfigurationManager.AppSettings["Login"] +" "+ ex.Message);
                Thread.Sleep(3000);

                try
                {
                    _logRepository.WriteLog("Erro Login", LogType.Erro, _usuario.Login);

                }
                catch (Exception)
                {
                }
                finally
                {
                    this.Close();
                }
            }

            if (_usuario == null)
            {
                MessageBox.Show("O usuario " +
                                ConfigurationManager.AppSettings["Login"] +
                                " não tem permissão para acessar este sistema.");
                Thread.Sleep(3000);
                this.Close();
            }
            else
            {
                _logRepository.WriteLog("Login", LogType.Informacao, _usuario.Login);
            }
            InitializeComponent();
            System.Windows.Controls.Panel.SetZIndex(Loading, 20);

            lblVersion.Text = ConfigurationManager.AppSettings["Versao"];

            

            CarregaListasBasicas();

            cbGeneroAut.DataContext = _generos;
            datePickerInicialAut.SelectedDate = DateTime.Now;

            cbProvGen.DataContext = _generos;
            cbRankGen.DataContext = _generos;
            cbCanhotGen.DataContext = _generos;
            cbPgAbertGen.DataContext = _generos;
            cbUtilGen.DataContext = _generos;
            cbGenero.DataContext = _generos;

            datePickerInicial.SelectedDate = DateTime.Now;

            cbCanhotEdit.DataContext = _editoras;
        }

        /// <summary>
        /// Carrega e configura os principais componentes e filtros de tela. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (!_isLoaded)
                _isLoaded = true;
            this.WindowState = WindowState.Maximized;

            _unidade = Properties.Settings.Default.Unidade;
            cbUnidade.DataContext = new UnidadeRepository().GetUnidades().OrderBy(u => u.Descricao);
            cbUnidade.SelectedValue = int.Parse(_unidade);

            var editoras = new Repositorio().Obter<Editora>().OrderBy(ed => ed.Nome).AsNoTracking().ToList();
            var editoraDefault = new Editora() {Nome = "--Todos--", EditoraID = 0};
            editoras.Insert(0, editoraDefault);
            cbEditoraAP.ItemsSource = editoras;

            var associacao = new Repositorio().Obter<Associacao>().OrderBy(ass => ass.Nome).AsNoTracking().ToList();         
            cbxEditoraAss.ItemsSource = associacao;
            cbxPrecoAss.ItemsSource = associacao;

            var classificacao =
                new Repositorio().Obter<Classificacao>().OrderBy(cls => cls.Descricao).AsNoTracking().ToList();
            cbxGenClass.ItemsSource = classificacao;
            cbxPrecoClass.ItemsSource = classificacao;

            var genP = new Repositorio().Obter<Genero>().OrderBy(genPs => genPs.Descricao).AsNoTracking().ToList();
            cbxPrecoGen.ItemsSource = genP;          
            cbxProgGen.ItemsSource = genP;

            //var gennovela = new Repositorio().Obter<Genero>().OrderBy(g => g.Descricao).AsNoTracking().ToList();
            //var gennov = new Genero();
            //gennovela.Insert(0, gennov);
            //cbNovelaGen.ItemsSource = gennovela;

            CarregaNovela();
            
            //var prog = new Repositorio().Obter<Programa>().OrderBy(pg => pg.Nome).AsNoTracking().ToList();
            //var progP = new Programa();
            //prog.Insert(0, progP);
            //cbNovelaProg.ItemsSource = prog;

            var quadropg = new Repositorio().Obter<Programa>().OrderBy(m => m.Nome).AsNoTracking().ToList();
            var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
            quadropg.Insert(0, defaultValue);
            cbQuadroProg.ItemsSource = quadropg;
           


            //•	Ter como o usuário informar o tipo de trilha que está sendo referenciada.
            var tipoTrilhas = new Repositorio().Obter<TipoTrilha>().OrderBy(tt => tt.Descricao).AsNoTracking().ToList();

            var tipoTrilhaDefault = new TipoTrilha() {Descricao = "--Todos--", TipoTrilhaID = 0};
            tipoTrilhas.Insert(0, tipoTrilhaDefault);

            cbMusTipoTrilha.ItemsSource = tipoTrilhas;
            cbRankTipTril.ItemsSource = tipoTrilhas;


            btnAprovar.IsEnabled = false;
            Uri uri = new Uri("Images/OKGray.png", UriKind.Relative);
            ImageSource imgSource = new BitmapImage(uri);
            imgBtnAprovar.Source = imgSource;

            btnTrancar.IsEnabled = false;

            btnPesquisar.IsEnabled = false;

            if (_usuario.Supervisor)
                btnAprovar.Visibility = Visibility.Visible;

            if (_usuario.Administrador)
            {
                btnTrancar.Visibility = Visibility.Visible;
                dgMusicas.Columns[0].Visibility = Visibility.Visible;
                dgMusicas.Columns[1].Visibility = Visibility.Visible;
                dgMusicas.Columns[2].Visibility = Visibility.Visible;
            }

            toolbarClassificaSonorizacao.IsEnabled = false;

            lblUsuario.Text = "Usuário: " + _usuario.Login;

            tabControlGeral.Height = Double.NaN;
            gridTabSonorizacao.Height = Double.NaN;

            dgExibicoes.Height = ActualHeight - 360;
            dgMusicas.Width = (ActualWidth - dgExibicoes.ActualWidth) - 100;
            dgMusicas.Height = dgExibicoes.Height;
            dockGridsSincronizacao.Height = dgExibicoes.Height + 20;

            cbMusTipoTrilha.SelectedValue = 0;

            dgMusicas.Visibility = Visibility.Hidden;
            dgExibicoes.Visibility = Visibility.Hidden;

            var cbp = _meses;


            cbECADMes.DisplayMemberPath = "monthName";
            cbECADMes.SelectedValuePath = "monthNumber";
            cbECADMes.ItemsSource = cbp;
            cbECADMes.SelectedIndex = 0;

            var cbpAno = _anos;

            cbECADAno.DisplayMemberPath = "monthName";
            cbECADAno.SelectedValuePath = "monthNumber";
            cbECADAno.ItemsSource = cbpAno;
            cbECADAno.Text = DateTime.Now.Year.ToString();

            cbPgAbertMes.DisplayMemberPath = "monthName";
            cbPgAbertMes.SelectedValuePath = "monthNumber";
            cbPgAbertMes.ItemsSource = cbp;

            cbPgAbertAno.DisplayMemberPath = "monthName";
            cbPgAbertAno.SelectedValuePath = "monthNumber";
            cbPgAbertAno.ItemsSource = cbpAno;
            cbPgAbertAno.Text = DateTime.Now.Year.ToString();
        }

        /// <summary>
        /// Seleciona o genero e preenche  combo de programas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGeneroSonorizacao_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbPrograma.DataContext = null;

            //Obtem programas a partir do genero
            if (cbGenero.SelectedIndex != -1)
            {
                //Programas
                using (Repositorio repositorio = new Repositorio())
                {
                    var genSel = int.Parse(cbGenero.SelectedValue.ToString());

                    List<Programa> programas;
                    if (genSel == 0)
                        programas = repositorio
                            .Obter<Programa>()
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();
                    else
                        programas = repositorio
                            .Obter<Programa>(p => p.GeneroID == genSel)
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();

                    var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                    programas.Insert(0, defaultValue);
                    cbPrograma.DataContext = null;
                    cbPrograma.DataContext = programas;
                    cbPrograma.SelectedIndex = 0;
                    cbPrograma.SelectedValue = 0;
                    //  cbAnoECAD.SelectedValue = 0;
                    cbPrograma.UpdateLayout();
                }
                btnPesquisar.IsEnabled = true;
            }
        }

        /// <summary>
        /// Seleciona o genero e preenche  combo de programas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbRankGen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbRankProg.DataContext = null;

            //Obtem programas a partir do genero
            if (cbRankGen.SelectedIndex != -1)
            {
                //Programas
                using (Repositorio repositorio = new Repositorio())
                {
                    var genSel = int.Parse(cbRankGen.SelectedValue.ToString());

                    List<Programa> programas;
                    if (genSel == 0)
                        programas = repositorio
                            .Obter<Programa>()
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();
                    else
                        programas = repositorio
                            .Obter<Programa>(p => p.GeneroID == genSel)
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();

                    var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                    programas.Insert(0, defaultValue);
                    cbRankProg.DataContext = null;
                    cbRankProg.DataContext = programas;
                    cbRankProg.SelectedIndex = 0;
                    cbRankProg.SelectedValue = 0;
                    //  cbAnoECAD.SelectedValue = 0;
                    cbRankProg.UpdateLayout();
                }
                btnPesquisar.IsEnabled = true;
            }
        }

        private void cbGeneroCanhoto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbCanhotProg.DataContext = null;

            //Obtem programas a partir do genero
            if (cbCanhotGen.SelectedIndex != -1)
            {
                //Programas
                using (Repositorio repositorio = new Repositorio())
                {
                    var genSel = int.Parse(cbCanhotGen.SelectedValue.ToString());

                    List<Programa> programas;
                    if (genSel == 0)
                        programas = repositorio
                            .Obter<Programa>()
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();
                    else
                        programas = repositorio
                            .Obter<Programa>(p => p.GeneroID == genSel)
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();

                    var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                    programas.Insert(0, defaultValue);
                    cbCanhotProg.DataContext = null;
                    cbCanhotProg.DataContext = programas;
                    cbCanhotProg.SelectedIndex = 0;
                    cbCanhotProg.SelectedValue = 0;
                    //  cbAnoECAD.SelectedValue = 0;
                    cbCanhotProg.UpdateLayout();
                }

            }
        }

        private void cbPgAbertGen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbPgAbertProg.DataContext = null;

            //Obtem programas a partir do genero
            if (cbPgAbertGen.SelectedIndex != -1)
            {
                //Programas
                using (Repositorio repositorio = new Repositorio())
                {
                    var genSel = int.Parse(cbPgAbertGen.SelectedValue.ToString());

                    List<Programa> programas;
                    if (genSel == 0)
                        programas = repositorio
                            .Obter<Programa>()
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();
                    else
                        programas = repositorio
                            .Obter<Programa>(p => p.GeneroID == genSel)
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();

                    var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                    programas.Insert(0, defaultValue);
                    cbPgAbertProg.DataContext = null;
                    cbPgAbertProg.DataContext = programas;
                    cbPgAbertProg.SelectedIndex = 0;
                    cbPgAbertProg.SelectedValue = 0;
                    //  cbAnoECAD.SelectedValue = 0;
                    cbPgAbertProg.UpdateLayout();
                }

            }
        }

        private void CarregaNovela()
        {
            var result = from a in context.Generos
                         where a.GeneroID == 2 || a.GeneroID == 3 || a.GeneroID == 4
                         select a;

            cbNovelaGen.ItemsSource = result.ToList();

            var prog = from p in context.Programas
                       where p.GeneroID == 2 || p.GeneroID == 3 || p.GeneroID == 4
                       select p;

            cbNovelaProg.ItemsSource = prog.ToList();

        }


        /// <summary>
        /// Seleciona o genero e preenche  combo de programas
        /// </summary>
        /// <param name="genID"> ID do genero selecionado</param>
        private List<Programa> ObterListaProgramasPorGenero(int genID)
        {
            //Obtem programas a partir do genero

            List<Programa> programas;
            using (var repositorio = new Repositorio())
            {
                if (genID != 0)
                {
                    programas = repositorio.Obter<Programa>(p => p.GeneroID == genID)
                                           .OrderBy(p => p.Nome)
                                           .ToList();
                }
                else
                {
                    programas = repositorio.Obter<Programa>()
                                           .OrderBy(p => p.Nome)
                                           .ToList();
                }

                var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                programas.Insert(0, defaultValue);
            }
            return programas;
        }

        /// <summary>
        /// Inicia pesquisa de sonorizações populando a grid de exibições caso os filtros selecionados sejam validos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PesquisarSonorizacoes(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgExibicoes);
            toolbarClassificaSonorizacao.IsEnabled = false;
            UpdateLayout();
            if (cbGenero.SelectedIndex != -1 && datePickerInicial.SelectedDate.HasValue)
            {
                datePickerFinal.SelectedDate = datePickerFinal.SelectedDate ?? DateTime.Now;
                if (datePickerInicial.SelectedDate <= datePickerFinal.SelectedDate)
                {
                    DateTime dtInicial = DateTime.Parse(datePickerInicial.SelectedDate.Value.ToShortDateString());
                    DateTime dtFinal = DateTime.Parse(datePickerFinal.SelectedDate.Value.ToShortDateString());

                    var genID = int.Parse(cbGenero.SelectedValue.ToString());

                    CarregarGridExibicoes(dtInicial, dtFinal, genID);
                }
            }
            ClearGrid(dgMusicas);
            dgExibicoes.Height = ActualHeight - 360;
            dgMusicas.Height = dgExibicoes.ActualHeight;
            dgMusicas.Width = (ActualWidth - dgExibicoes.ActualWidth) - 100;
            dockGridsSincronizacao.Height = dgExibicoes.Height + 20;

            dgMusicas.Visibility = Visibility.Visible;
            dgExibicoes.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Carregar combos para sonorizacao
        /// </summary>
        private void CarregarCombosSonorizacao()
        {
            var genID = _sincSel.Exibicao.Programa.GeneroID;

            using (Repositorio repositorio = new Repositorio())
            {
                List<Classificacao> classificacoes =
                    repositorio.Obter<Genero>(g => g.GeneroID == genID)
                               .Select(g => g.Classificacoes)
                               .Single()
                               .ToList();
                cbClassificacao.ItemsSource = classificacoes.OrderBy(c => c.Descricao);
                cbClassificacao.Items.Refresh();

                var tipoExibicoes = repositorio.Obter<TipoExibicao>().ToList().OrderBy(te => te.Descricao);
                cbTipoExibicao.ItemsSource = tipoExibicoes;
                cbTipoExibicao.Items.Refresh();

                var quadros = repositorio.Obter<Quadro>().ToList().OrderBy(q => q.Descricao);
                cbQuadro.ItemsSource = quadros;
                cbQuadro.Items.Refresh();

                toolbarClassificaSonorizacao.IsEnabled = true;
            }
        }

        /// <summary>
        /// Altera e salva as modificações feitas na coluna classificação ao mudar a combobox responsavel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlterarClassificacao(object sender, SelectionChangedEventArgs e)
        {
            if (dgExibicoes.SelectedItems.Count > 0 && cbClassificacao.SelectedIndex != -1)
            {
                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (object row in dgMusicas.SelectedItems)
                    {
                        Classificacao classificacao = repositorio.Obter<Classificacao>()
                                                                 .Single(
                                                                     c =>
                                                                     c.ClassificacaoID ==
                                                                     (int) cbClassificacao.SelectedValue);

                        var ms = (RowMusicasSonorizacao) row;

                        var son =
                            context.Sonorizacoes.FirstOrDefault(m => m.SonorizacaoID == ms.Sonorizacao.SonorizacaoID);

                        son.ClassificacaoID = classificacao.ClassificacaoID;

                        if (_sincSel.Aberto)
                        {
                            son.Alterada = true;
                            son.AlteradaPor = _usuario.Login;
                        }
                        context.Sonorizacoes.Attach(son);
                        context.Entry(son).Property(p => p.ClassificacaoID).IsModified = true;
                        context.Entry(son).Property(p => p.Alterada).IsModified = true;
                        context.Entry(son).Property(p => p.AlteradaPor).IsModified = true;
                        context.SaveChanges();
                    }
                }
                dgMusicas.Items.Refresh();
                dgMusicas.UpdateLayout();

                if (MusicasClassificadas())
                    PreAprovarSincronizacao();
            }
            else
            {
                cbClassificacao.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Preenche combobox de programas após selecionar o gênero.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGeneroAutorizacao_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbProgramaAut.DataContext = null;

            //Obtem programas a partir do genero
            if (cbGeneroAut.SelectedIndex != -1)
            {
                cbProgramaAut.DataContext = null;
                cbProgramaAut.DataContext = ObterListaProgramasPorGenero((int) cbGeneroAut.SelectedValue);
                cbProgramaAut.SelectedIndex = 0;
                cbProgramaAut.SelectedValue = 0;
                cbProgramaAut.UpdateLayout();
            }
        }

        /// <summary>
        /// Pesquisa autorizações após selecionar os filtros.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PesquisarAutorizacoes(object sender, RoutedEventArgs e)
        {
            toolbarRegistraAutorizacao.IsEnabled = false;
            ClearGrid(dgAutProgramas);
            UpdateLayout();
            if (cbGeneroAut.SelectedIndex != -1 && datePickerInicialAut.SelectedDate.HasValue)
            {
                CarregarGridExibicoesAutorizacao();
            }
            ClearGrid(dgAutMusicas);
            dgAutProgramas.Height = ActualHeight - 360;
            dgAutMusicas.Height = dgAutProgramas.ActualHeight;
            dgAutMusicas.Width = (ActualWidth - dgAutProgramas.ActualWidth) - 100;
            dockGridsAutorizacoes.Height = dgAutProgramas.Height + 20;

            dgAutProgramas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Exibi as sonorizações de acordo com a exibição selecionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExibirSonorizacoes(object sender, SelectedCellsChangedEventArgs e)
        {
            if (((DataGrid) sender).CurrentItem != null)
            {
                _sincSel = null;
                txtObservacao.Text = "";
                ClearGrid(dgMusicas);
                CarregarGridMusicas();
                CarregarCombosSonorizacao();
            }
            dgMusicas.Height = dgExibicoes.ActualHeight;
            dgMusicas.Width = (ActualWidth - dgExibicoes.ActualWidth) - 100;
            dockGridsSincronizacao.Height = dgExibicoes.Height + 20;
            dgMusicas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Exibi as autorizações de acordo com a exibição selecionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExibirAutorizacoes(object sender, SelectedCellsChangedEventArgs e)
        {
            ClearGrid(dgAutMusicas);

            CarregarGridMusicasAutorizacao();

            dgAutMusicas.Height = dgAutProgramas.ActualHeight;
            dgAutMusicas.Width = (ActualWidth - dgAutProgramas.ActualWidth) - 100;
            dockGridsAutorizacoes.Height = dgAutProgramas.Height + 20;

            cbPagEditora.SelectedIndex = -1;
            cbPagValor.SelectedIndex = -1;
            toolbarRegistraAutorizacao.IsEnabled = false;

            dgAutMusicas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Altera e salva as modificações feitas na coluna tipo de exibição ao mudar a combobox responsavel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlterarTipoExibicao(object sender, SelectionChangedEventArgs e)
        {
            if (dgExibicoes.SelectedItems.Count > 0 && cbTipoExibicao.SelectedIndex != -1)
            {
                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var row in dgMusicas.SelectedItems)
                    {
                        var tipExibSel =
                            repositorio.Obter<TipoExibicao>(
                                te => te.TipoExibicaoID == (int) cbTipoExibicao.SelectedValue)
                                       .FirstOrDefault();

                        var ms = (RowMusicasSonorizacao) row;

                        var son =
                            context.Sonorizacoes.FirstOrDefault(s => s.SonorizacaoID == ms.Sonorizacao.SonorizacaoID);

                        son.TipoExibicaoID = tipExibSel.TipoExibicaoID;

                        if (_sincSel.Aberto)
                        {
                            son.Alterada = true;
                            son.AlteradaPor = _usuario.Login;
                        }

                        context.Sonorizacoes.Attach(son);
                        context.Entry(son).Property(p => p.TipoExibicaoID).IsModified = true;
                        context.Entry(son).Property(p => p.Alterada).IsModified = true;
                        context.Entry(son).Property(p => p.AlteradaPor).IsModified = true;
                        context.SaveChanges();
                    }
                }

                dgMusicas.Items.Refresh();
                dgMusicas.UpdateLayout();

                if (MusicasClassificadas())
                    PreAprovarSincronizacao();
            }
            else
            {
                cbTipoExibicao.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Altera e salva as modificações feitas na coluna quadro ao mudar a combobox responsavel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlterarQuadro(object sender, SelectionChangedEventArgs e)
        {
            if (dgExibicoes.SelectedItems.Count > 0 && cbQuadro.SelectedIndex != -1)
            {
                using (Repositorio repositorio = new Repositorio())
                {
                    foreach (var row in dgMusicas.SelectedItems)
                    {
                        var quadSel =
                            repositorio.Obter<Quadro>(q => q.QuadroID == (int) cbQuadro.SelectedValue).FirstOrDefault();

                        var ms = (RowMusicasSonorizacao) row;

                        var son =
                            context.Sonorizacoes.FirstOrDefault(s => s.SonorizacaoID == ms.Sonorizacao.SonorizacaoID);

                        son.QuadroID = quadSel.QuadroID;

                        if (_sincSel.Aberto)
                        {
                            son.Alterada = true;
                            son.AlteradaPor = _usuario.Login;
                        }

                        context.Sonorizacoes.Attach(son);
                        context.Entry(son).Property(p => p.QuadroID).IsModified = true;
                        context.Entry(son).Property(p => p.Alterada).IsModified = true;
                        context.Entry(son).Property(p => p.AlteradaPor).IsModified = true;
                        context.SaveChanges();
                    }

                    dgMusicas.Items.Refresh();
                    dgMusicas.UpdateLayout();

                    if (MusicasClassificadas())
                        PreAprovarSincronizacao();
                }
            }
            else
            {
                cbQuadro.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Altera a unidade que será usado nos filtros das funcionalidades do sistema
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlterarUnidade(object sender, SelectionChangedEventArgs e)
        {
            UnidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
        }


        private void Importar(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgVetrix);

            string arquivo = null;
            var dialog = new OpenFileDialog {RestoreDirectory = true};

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
                arquivo = dialog.FileName;

            var extension = Path.GetExtension(arquivo);
            if (extension != null && (!string.IsNullOrEmpty(arquivo) &&
                                      (extension.Equals(".xml", StringComparison.CurrentCultureIgnoreCase)
                                       || extension.Equals(".xls", StringComparison.CurrentCultureIgnoreCase)
                                       || extension.Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase)
                                      )))
            {
                if (MessageBox.Show("Gostaria de iniciar a importação do arquivo " + Path.GetExtension(arquivo) + "?",
                                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {

                    try
                    {
                        List<RowVetrixErro> erros = null;
                        // ImportBar.IsIndeterminate = true;
                        statusBarMain.UpdateLayout();
                        var s = Path.GetExtension(arquivo);
                        ShowLoader();
                        ThreadStart dataDownloadThread = delegate
                                                             {
                                                                 if (s != null &&
                                                                     s.Equals(".xml",
                                                                              StringComparison.CurrentCultureIgnoreCase))
                                                                 {
                                                                     erros = new ImportacaoVetrix().Importar(arquivo,
                                                                                                             out
                                                                                                                 _totalImportadasMusicas,
                                                                                                             out
                                                                                                                 _totalImportadasSucesso,
                                                                                                             out
                                                                                                                 _totalImportadasErros);
                                                                 }
                                                                 else
                                                                 {
                                                                     var extension1 = Path.GetExtension(arquivo);
                                                                     if (extension1 != null &&
                                                                         (extension1.Equals(".xls",
                                                                                            StringComparison
                                                                                                .CurrentCultureIgnoreCase)
                                                                          ||
                                                                          extension1.Equals(".xlsx",
                                                                                            StringComparison
                                                                                                .CurrentCultureIgnoreCase)))
                                                                         erros =
                                                                             new ImportacaoFiliais().Importar(arquivo,
                                                                                                              out
                                                                                                                  _totalImportadasMusicas,
                                                                                                              out
                                                                                                                  _totalImportadasSucesso,
                                                                                                              out
                                                                                                                  _totalImportadasErros);
                                                                 }

                                                                 Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                        (EventHandler)
                                                                                        delegate
                                                                                            {
                                                                                                HideLoader();
                                                                                                dgVetrix.ItemsSource =
                                                                                                    erros;
                                                                                                cbClassificacao.Items
                                                                                                               .Refresh();
                                                                                                //ImportBar.IsIndeterminate = false;
                                                                                                MessageBox.Show(
                                                                                                    "Resultado:" +
                                                                                                    Environment.NewLine +
                                                                                                    "Musicas encontradas: " +
                                                                                                    _totalImportadasMusicas +
                                                                                                    Environment.NewLine +
                                                                                                    "Importadas com sucesso: " +
                                                                                                    _totalImportadasSucesso +
                                                                                                    Environment.NewLine +
                                                                                                    "Não importadas: " +
                                                                                                    _totalImportadasErros +
                                                                                                    Environment.NewLine,
                                                                                                    "Importação");
                                                                                            }, null, null);
                                                             };
                        dataDownloadThread.BeginInvoke(
                            delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);

                        _logRepository.WriteLog("Importação", LogType.Informacao, _usuario.Login);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Importação");
                        _logRepository.WriteLog(ex);
                    }

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(arquivo))
                    MessageBox.Show("Arquivo inválido!");
            }
        }

        #region DatePicker Events

        private void datePicker_TextChanged(object sender)
        {
            DateTime dt;
            var dp = (sender as DatePicker);
            string currentText = dp.Text;
            if (!DateTime.TryParse(currentText, out dt))
            {
                try
                {
                    string day = currentText.Substring(0, 2);
                    string month = currentText.Substring(2, 2);
                    string year = currentText.Substring(4, 4);

                    dt = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                    dp.SelectedDate = dt;
                }
                catch (Exception)
                {
                    dp.SelectedDate = null;
                }
            }
        }

        private void datePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter == e.Key)
            {
                datePicker_TextChanged(sender);
            }
        }

        private void datePicker_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            datePicker_TextChanged(sender);
        }

        private void datePicker_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DatePicker dp = (sender as DatePicker);
            dp.Text = String.Empty;
        }

        #endregion DatePicker Events

        private void tabControlGeral_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof (TabControl))
            {
                //sonorizacao
                ClearGrid(dgExibicoes);
                ClearGrid(dgMusicas);
                ClearGrid(dgVetrix);
                cbGenero.SelectedIndex = -1;
                cbPrograma.SelectedIndex = -1;
                datePickerInicial.SelectedDate = DateTime.Now;
                datePickerFinal.SelectedDate = DateTime.Now;

                //autorizacao
                ClearGrid(dgAutProgramas);
                ClearGrid(dgAutMusicas);
                cbGeneroAut.SelectedIndex = -1;
                cbProgramaAut.SelectedIndex = -1;
                datePickerInicialAut.SelectedDate = DateTime.Now;
                //Cadastros
                ClearGrid(dgMusCadastradas);

                //ECAD
                ClearGrid(dgECAD);
                //Provisao
                ClearGrid(dgProvisao);
                //Canhoto
                //ClearGrid();


                e.Handled = true;
            }
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void dgMusicas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid) sender;
            if (!_sincSel.Aprovado)
            {
                if (Key.Delete == e.Key)
                {
                    var result = MessageBox.Show("Deseja excluir as linhas selecionadas?", "Deletar sincronização",
                                                 MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        dgMusicas.CanUserDeleteRows = true;
                        foreach (var row in grid.SelectedItems)
                        {
                            int sonID = ((RowMusicasSonorizacao) row).Sonorizacao.SonorizacaoID;
                            var musicaSelecionada = (from m in context.Sonorizacoes
                                                     where m.SonorizacaoID == sonID
                                                     select m).Single();
                            context.Entry(musicaSelecionada).State = EntityState.Deleted;
                            context.SaveChanges();
                        }
                    }
                }
            }
        }

        private void dgMusicas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgMusicas.CanUserDeleteRows = false;

            cbClassificacao.SelectedIndex = -1;
            cbQuadro.SelectedIndex = -1;
            cbTipoExibicao.SelectedIndex = -1;
        }

        private void dgMusicasAut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            dgMusicas.CanUserDeleteRows = false;

            cbClassificacao.SelectedIndex = -1;
            cbQuadro.SelectedIndex = -1;
            cbTipoExibicao.SelectedIndex = -1;
            cbPagEditora.SelectedIndex = -1;

            cbPagValor.SelectedIndex = -1;
            cbPagValor.ItemsSource = null;

            CarregarComboPagamentosEditoras();

            dtPagVencimento.DisplayDateStart = DateTime.Now;

            var aut = (RowExibicaoAutorizacao) dgAutMusicas.CurrentItem;
            if (aut != null)
            {
                CarregaInfoPagamentos(aut.AutID, aut.MusicaID, aut.SincID, aut.SonID);

                toolbarRegistraAutorizacao.IsEnabled = true;
            }
        }


        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelecionaAutorizacao(object sender, MouseButtonEventArgs e)
        {
            //musica selecionada
            var aut = (RowExibicaoAutorizacao) dgAutMusicas.CurrentItem;
            var autWindow = new PaymentWindow(aut.AutID, aut.MusicaID, aut.SincID, aut.SonID);
            autWindow.Closed += (sender2, e2) => CarregarGridMusicasAutorizacao();
            autWindow.ShowActivated = true;
            autWindow.ShowDialog();
        }

        #region ### Metodos ###

        /// <summary>
        /// Responsavel por carregar as musicas sonorizadas da exibição selecionada
        /// </summary>
        private void CarregarGridMusicas()
        {
            if ((dgExibicoes.Items.Count > 0) &&
                (dgExibicoes.Columns.Count > 0))
            {
                if (dgExibicoes.CurrentCell.Item.ToString() != "{DependencyProperty.UnsetValue}")
                {
                    //using (Repositorio repositorio = new Repositorio())

                    RowExibicaoAprovacao exibSel = (RowExibicaoAprovacao) dgExibicoes.CurrentCell.Item;
                    var sonorizacoes =
                        //repositorio.Obter<Sonorizacao>(s => s.Sincronizacao.ExibicaoID == exibSel.Exibicao.ExibicaoID, false).ToList();
                        from son in context.Sonorizacoes
                        join usu in context.Usuarios
                            on son.Importacao.UsuarioID equals usu.UsuarioID
                        where son.Sincronizacao.ExibicaoID == exibSel.Exibicao.ExibicaoID
                        select new RowMusicasSonorizacao() {Sonorizacao = son, Usuario = usu};

                    lblNumMusicas.Text = "Nº Musicas:" + sonorizacoes.Count();
                    var totalMinutagem = sonorizacoes.Sum(s => s.Sonorizacao.Minutagem.Seconds);
                    lblTotalMinutagem.Text = "Minutagem total: " +
                                             TimeSpan.FromSeconds(totalMinutagem).TotalMinutes.ToString("0.##") +
                                             " minutos";

                    dgMusicas.ItemsSource = sonorizacoes.ToList();
                    dgMusicas.Items.Refresh();

                    _sincSel = (from s in context.Sincronizacoes
                                where exibSel.Exibicao.ExibicaoID == s.ExibicaoID
                                select s).First();

                    if (!String.IsNullOrWhiteSpace(_sincSel.Observacao))
                        txtObservacao.Text = _sincSel.Observacao;

                    var genero = _sincSel.Exibicao.Programa.Genero.Descricao;
                    if (genero == "NOVELA" || genero == "SERIE" || genero == "MINISSERIE")
                    {
                        var novela = context.Novela.FirstOrDefault(n => n.ProgramaID == _sincSel.Exibicao.ProgramaID);

                        if (novela != null)
                        {
                            context.Sonorizacoes.FirstOrDefault(s => s.SincronizacaoID == _sincSel.SincronizacaoID);
                            lblTituloNacional.Text = "Título: "+novela.TituloNacional;
                            //TODO: capitulo
                        }   
                    }
                        

                    if (exibSel.Aprovado)
                    {
                        toolbarClassificaSonorizacao.IsEnabled = false;

                        btnAprovar.IsEnabled = false;
                        Uri uriOkGray = new Uri("Images/OKGray.png", UriKind.Relative);
                        ImageSource imgSourceOkGray = new BitmapImage(uriOkGray);
                        imgBtnAprovar.Source = imgSourceOkGray;

                        if (_usuario.Administrador && _sincSel != null)
                        {
                            btnTrancar.IsEnabled = true;
                            btnTrancar.Click -= BtnDestrancarClick;
                            btnTrancar.Click -= ReabrirAprovacao;

                            btnTrancar.Click += BtnDestrancarClick;

                            var uriLock = new Uri("Images/lock.png", UriKind.Relative);
                            var imgSourceLock = new BitmapImage(uriLock);
                            imgBtnTrancar.Source = imgSourceLock;

                            if (_sincSel.Aberto)
                            {
                                btnAprovar.Visibility = Visibility.Visible;
                                btnAprovar.IsEnabled = true;
                                Uri uriOk = new Uri("Images/OK.png", UriKind.Relative);
                                ImageSource imgSourceOk = new BitmapImage(uriOk);
                                imgBtnAprovar.Source = imgSourceOk;
                            }
                        }
                    }
                    else
                    {
                        toolbarClassificaSonorizacao.IsEnabled = false;

                        if (_usuario.Supervisor && MusicasClassificadas())
                        {
                            btnAprovar.IsEnabled = true;
                            Uri uri = new Uri("Images/OK.png", UriKind.Relative);
                            ImageSource imgSource = new BitmapImage(uri);
                            imgBtnAprovar.Source = imgSource;
                        }
                        else
                        {
                            btnAprovar.IsEnabled = false;
                            Uri uri = new Uri("Images/OKGray.png", UriKind.Relative);
                            ImageSource imgSource = new BitmapImage(uri);
                            imgBtnAprovar.Source = imgSource;
                        }
                        if (_usuario.Administrador)
                        {
                            btnTrancar.IsEnabled = false;
                            btnTrancar.Click -= BtnDestrancarClick;
                            btnTrancar.Click -= ReabrirAprovacao;

                            btnTrancar.Click += ReabrirAprovacao;
                            Uri uri = new Uri("Images/unlock.png", UriKind.Relative);
                            ImageSource imgSource = new BitmapImage(uri);
                            imgBtnTrancar.Source = imgSource;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Carrega as musicas sonorizadas depois de que a exibição é selecionada.
        /// </summary>
        private void AtualizarGridMusicas()
        {
            if ((dgExibicoes.Items.Count > 0) &&
                (dgExibicoes.Columns.Count > 0))
            {
                if (_sincSel != null)
                {
                    var sinc = _sincSel;
                    var sonorizacoes =
                        new Repositorio().Obter<Sonorizacao>(s => s.Sincronizacao.ExibicaoID == sinc.ExibicaoID)
                                         .ToList();

                    ClearGrid(dgMusicas);
                    dgMusicas.ItemsSource = sonorizacoes;
                    dgMusicas.Items.Refresh();
                    dgMusicas.UpdateLayout();

                    if (!String.IsNullOrWhiteSpace(sinc.Observacao))
                        txtObservacao.Text = sinc.Observacao;

                    if (sinc.Aprovado)
                    {
                        toolbarClassificaSonorizacao.IsEnabled = false;
                        btnAprovar.IsEnabled = false;
                        Uri uriOKGray = new Uri("Images/OKGray.png", UriKind.Relative);
                        ImageSource imgSourceOKGray = new BitmapImage(uriOKGray);
                        imgBtnAprovar.Source = imgSourceOKGray;

                        if (_usuario.Administrador && _sincSel != null)
                        {
                            btnTrancar.IsEnabled = true;
                            btnTrancar.Click -= BtnDestrancarClick;
                            btnTrancar.Click -= ReabrirAprovacao;

                            btnTrancar.Click += BtnDestrancarClick;

                            var uriLock = new Uri("Images/lock.png", UriKind.Relative);
                            var imgSourceLock = new BitmapImage(uriLock);
                            imgBtnTrancar.Source = imgSourceLock;

                            if (sinc.Aberto)
                            {
                                btnAprovar.Visibility = Visibility.Visible;
                                btnAprovar.IsEnabled = true;
                                Uri uriOK = new Uri("Images/OK.png", UriKind.Relative);
                                ImageSource imgSourceOK = new BitmapImage(uriOK);
                                imgBtnAprovar.Source = imgSourceOK;
                            }
                        }
                    }
                    else
                    {
                        toolbarClassificaSonorizacao.IsEnabled = true;
                        if (dgMusicas.Items.Count > 0)
                            if (MusicasClassificadas())
                                PreAprovarSincronizacao();

                        if (_usuario.Supervisor && MusicasClassificadas())
                        {
                            btnAprovar.IsEnabled = true;
                            Uri uri = new Uri("Images/OK.png", UriKind.Relative);
                            ImageSource imgSource = new BitmapImage(uri);
                            imgBtnAprovar.Source = imgSource;
                        }
                        else
                        {
                            btnAprovar.IsEnabled = false;
                            Uri uri = new Uri("Images/OKGray.png", UriKind.Relative);
                            ImageSource imgSource = new BitmapImage(uri);
                            imgBtnAprovar.Source = imgSource;
                        }
                        if (_usuario.Administrador)
                        {
                            btnTrancar.IsEnabled = false;
                            btnTrancar.Click -= BtnDestrancarClick;
                            btnTrancar.Click -= ReabrirAprovacao;

                            btnTrancar.Click += ReabrirAprovacao;
                            Uri uri = new Uri("Images/unlock.png", UriKind.Relative);
                            ImageSource imgSource = new BitmapImage(uri);
                            imgBtnTrancar.Source = imgSource;
                        }
                    }
                    _sincSel = sinc;
                }
            }
        }

        /// <summary>
        /// Responsavel por carregar as musicas autorizadas da exibição selecionada
        /// </summary>
        private void CarregarGridMusicasAutorizacao()
        {
            if ((dgAutProgramas.Items.Count > 0) &&
                (dgAutProgramas.Columns.Count > 0))
            {
                if (dgAutProgramas.CurrentCell.Item.ToString() != "{DependencyProperty.UnsetValue}" || _exibSel != null)
                {
                    if (dgAutProgramas.CurrentCell.Item.ToString() != "{DependencyProperty.UnsetValue}")
                        _exibSel = (RowExibicaoAprovacao) dgAutProgramas.CurrentCell.Item;
                    AutorizacaoBusiness autorizacao = new AutorizacaoBusiness();

                    dgAutMusicas.ItemsSource = autorizacao.Obter(context, _exibSel.Exibicao.ExibicaoID);
                    dgAutMusicas.Items.Refresh();

                    //TODO: descobrir porque existe mais de uma Sincronizacao para cada exibicao
                    _sincSel = (from s in context.Sincronizacoes
                                where _exibSel.Exibicao.ExibicaoID == s.ExibicaoID
                                select s).First();

                    //musicasSincronizacao.FirstOrDefault().SincronizacaoID;
                    if (!String.IsNullOrWhiteSpace(_sincSel.Observacao))
                        txtObservacao.Text = _sincSel.Observacao;
                }
            }
        }

        /// <summary>
        /// Reabre uma sonorização aprovada se o usuario for administrador
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReabrirAprovacao(object sender, RoutedEventArgs e)
        {
            if (_sincSel.Aprovado)
            {
                if (_sincSel.Aberto)
                {
                    if (MusicasClassificadas())
                    {
                        btnTrancar.IsEnabled = false;
                        btnTrancar.Click += ReabrirAprovacao;
                        Uri uri = new Uri("Images/lock.png", UriKind.Relative);
                        ImageSource imgSource = new BitmapImage(uri);
                        imgBtnTrancar.Source = imgSource;
                        imgBtnTrancar.UpdateLayout();

                        _sincSel.Aberto = false;
                        _sincSel.Aprovado = true;

                        context.Entry(_sincSel).State = EntityState.Modified;
                        context.SaveChanges();

                        if (datePickerInicial.SelectedDate != null && datePickerFinal.SelectedDate != null)
                        {
                            DateTime dtInicial = DateTime.Parse(datePickerInicial.SelectedDate.Value.ToShortDateString());
                            DateTime dtFinal = DateTime.Parse(datePickerFinal.SelectedDate.Value.ToShortDateString());

                            var genID = int.Parse(cbGenero.SelectedValue.ToString());

                            CarregarGridExibicoes(dtInicial, dtFinal, genID);
                        }
                        CarregarCombosSonorizacao();

                        ClearGrid(dgMusicas);
                    }
                }
            }
        }

        private void BtnDestrancarClick(object sender, RoutedEventArgs e)
        {
            if (_sincSel == null)
                return;

            if (_sincSel.Aprovado)
            {
                if (!_sincSel.Aberto)
                {
                    btnTrancar.IsEnabled = false;
                    btnTrancar.Click += ReabrirAprovacao;
                    Uri uri = new Uri("Images/unlock.png", UriKind.Relative);
                    ImageSource imgSource = new BitmapImage(uri);
                    imgBtnTrancar.Source = imgSource;
                    imgBtnTrancar.UpdateLayout();

                    //Reabre sessão
                    _sincSel.Aberto = true;
                    _sincSel.Aprovado = false;
                    _sincSel.PreAprovado = false;

                    context.Entry(_sincSel).State = EntityState.Modified;
                    context.SaveChanges();

                    DateTime dtInicial = DateTime.Parse(datePickerInicial.SelectedDate.Value.ToShortDateString());
                    DateTime dtFinal = DateTime.Parse(datePickerFinal.SelectedDate.Value.ToShortDateString());

                    var genID = int.Parse(cbGenero.SelectedValue.ToString());

                    CarregarGridExibicoes(dtInicial, dtFinal, genID);

                    CarregarCombosSonorizacao();

                    ClearGrid(dgMusicas);
                }
            }
        }

        /// <summary>
        /// Verifica se todas as músicas estão classificadas e retorna o resultado
        /// </summary>
        /// <returns></returns>
        private bool MusicasClassificadas()
        {
            foreach (var item in dgMusicas.Items.SourceCollection.OfType<RowMusicasSonorizacao>())
            {
                if (item.Sonorizacao.QuadroID == 0 || item.Sonorizacao.ClassificacaoID == 0 ||
                    item.Sonorizacao.TipoExibicaoID == 0
                    || item.Sonorizacao.Quadro.Descricao.Contains("NÃO DEFINID") ||
                    item.Sonorizacao.Classificacao.Descricao.Contains("NÃO DEFINID") ||
                    item.Sonorizacao.TipoExibicao.Descricao.Contains("NÃO DEFINID"))
                    return false;
            }
            return true;
        }


        private void PreAprovarSincronizacao()
        {
            if (_sincSel == null)
            {
                return;
            }
            foreach (var item in dgMusicas.Items.SourceCollection.OfType<Sonorizacao>())
            {
                item.Sincronizacao.PreAprovado = true;
                context.Entry(item).State = EntityState.Modified;
                context.SaveChanges();
            }
            //salva observacao para a Sincronizacao
            if (_sincSel.Observacao != txtObservacao.Text)
            {
                _sincSel.Observacao = txtObservacao.Text;
                context.Entry(_sincSel).State = EntityState.Modified;
                context.SaveChanges();
            }
            DateTime dtInicial = DateTime.Parse(datePickerInicial.SelectedDate.Value.ToShortDateString());
            DateTime dtFinal = DateTime.Parse(datePickerFinal.SelectedDate.Value.ToShortDateString());

            var genID = int.Parse(cbGenero.SelectedValue.ToString());

            CarregarGridExibicoes(dtInicial, dtFinal, genID);

            CarregarCombosSonorizacao();

            if (_usuario.Supervisor || _usuario.Administrador)
            {
                btnAprovar.IsEnabled = true;
                Uri uri = new Uri("Images/OK.png", UriKind.Relative);
                ImageSource imgSource = new BitmapImage(uri);
                imgBtnAprovar.Source = imgSource;
            }
        }


        /// <summary>
        /// Aprova a Sincronização após as sonorizações terem sidas classificadas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AprovarSincronizacao(object sender, RoutedEventArgs e)
        {
            if (_sincSel == null)
                return;

            _sincSel.Aprovado = true;
            if (_usuario.Administrador && _sincSel.Aberto)
            {
                _sincSel.Aberto = false;
            }
            _sincSel.AprovadoEm = DateTime.Now;

            context.Entry(_sincSel).State = EntityState.Modified;
            context.SaveChanges();

            if (_sincSel.Aberto == false)
            {
                using (Context repositorio = new Context())
                {
                    repositorio.Configuration.ProxyCreationEnabled = true;
                    var sons = repositorio.Sonorizacoes
                                          .Include("Classificacao")
                                          .Include("Sincronizacao")
                                          .Include("Sincronizacao.Exibicao")
                                          .Include("Sincronizacao.Exibicao.Programa")
                                          .Include("Musica")
                                          .Include("Musica.TipoTrilha")
                                          .Include("Musica.Autor")
                                          .Include("Musica.Interprete")
                                          .Include("TipoExibicao")
                                          .Where(s => s.SincronizacaoID == _sincSel.SincronizacaoID)
                                          .AsNoTracking();
                }
            }

            btnAprovar.IsEnabled = false;
            btnAprovar.UpdateLayout();

            DateTime dtInicial = DateTime.Parse(datePickerInicial.SelectedDate.Value.ToShortDateString());
            DateTime dtFinal = DateTime.Parse(datePickerFinal.SelectedDate.Value.ToShortDateString());

            var genID = int.Parse(cbGenero.SelectedValue.ToString());


            dgExibicoes.UnselectAll();
            _sincSel = null;

            PreAprovarSincronizacao();

            CarregarGridExibicoes(dtInicial, dtFinal, genID);

            CarregarCombosSonorizacao();

            dgMusicas.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Responsavel por carregar a grid de exibições
        /// Pesquisa traz as exibições dos programas nas datas sugeridas e carrega a grid de exibicoes
        /// </summary>
        /// <param name="dtInicial">Data inicial</param>
        /// <param name="dtFinal">Data final</param>
        /// <param name="genID">Genêro</param>
        private void CarregarGridExibicoes(DateTime dtInicial, DateTime dtFinal, int genID)
        {
            var selectedIndex = dgExibicoes.SelectedIndex;
            var items = ((ListBox) cbPrograma.Template.FindName("lstBox", cbPrograma)).SelectedItems;
            List<Programa> programas = new List<Programa>();
            foreach (var item in items)
            {
                programas.Add((Programa) item);
            }

            if (programas.Count <= 0)
            {
                return;
            }
            btnTrancar.IsEnabled = false;
            btnTrancar.UpdateLayout();
            cbClassificacao.SelectedIndex = -1;
            cbTipoExibicao.SelectedIndex = -1;
            cbQuadro.SelectedIndex = -1;
            TimeSpan diff = dtFinal.Subtract(dtInicial);
            //ClearGrid(dgExibicoes);
            //ClearGrid(dgMusicas);

            List<RowExibicaoAprovacao> exibicoes;

            using (Repositorio repositorio = new Repositorio())
            {
                IQueryable<Exibicao> query;

                if (genID != 0)
                    query = repositorio.Obter<Exibicao>()
                                       .Where(ex => ex.UnidadeID == UnidadeID)
                                       .Where(ex => ex.Programa.GeneroID == genID)
                                       .Where(
                                           ex =>
                                           EntityFunctions.DiffDays(dtInicial, ex.Data).Value <= diff.TotalDays &&
                                           EntityFunctions.DiffDays(dtInicial, ex.Data).Value >= 0);
                else
                    query = repositorio.Obter<Exibicao>()
                                       .Where(ex => ex.UnidadeID == UnidadeID)
                                       .Where(
                                           ex =>
                                           EntityFunctions.DiffDays(dtInicial, ex.Data).Value <= diff.TotalDays &&
                                           EntityFunctions.DiffDays(dtInicial, ex.Data).Value >= 0);

                List<Exibicao> prog = new List<Exibicao>();

                if (programas.Any(p => p.Nome.Contains("--Todos--")))
                {
                    prog.AddRange(
                        query.Where(ex => ex.ProgramaID > 0).AsQueryable()
                        );
                }
                else
                {
                    foreach (var programa in programas)
                    {
                        prog.AddRange(
                            query.Where(ex => ex.ProgramaID == programa.ProgramaID).AsQueryable()
                            );
                    }
                }

                exibicoes = prog.OrderBy(ex => ex.Data)
                                .Select(ex => new RowExibicaoAprovacao {Exibicao = ex})
                                .ToList();

                foreach (var exib in exibicoes)
                {
                    exib.Exibicao.Programa.Nome =
                        repositorio.Obter<Programa>(p => p.ProgramaID == exib.Exibicao.ProgramaID)
                                   .Select(p => p.Nome)
                                   .FirstOrDefault();
                    exib.Aprovado = false;
                    exib.PreAprovado = false;

                    exib.Aprovado =
                        repositorio.Obter<Sincronizacao>(s => s.ExibicaoID == exib.Exibicao.ExibicaoID)
                                   .Select(s => s.Aprovado)
                                   .FirstOrDefault();

                    exib.PreAprovado =
                        repositorio.Obter<Sincronizacao>(s => s.ExibicaoID == exib.Exibicao.ExibicaoID)
                                   .Select(s => s.PreAprovado)
                                   .FirstOrDefault();

                    if (exib.Aprovado)
                    {
                        exib.Aberto =
                            repositorio.Obter<Sincronizacao>(s => s.ExibicaoID == exib.Exibicao.ExibicaoID)
                                       .Select(s => s.Aberto)
                                       .FirstOrDefault();
                    }
                }

                dgExibicoes.ItemsSource = exibicoes;
            }
            dgExibicoes.Items.Refresh();
            dgExibicoes.SelectedIndex = selectedIndex;

            UpdateLayout();
        }

        /// <summary>
        /// Responsavel por carregar a grid de exibições da tela de autorizacaos
        /// </summary>
        private void CarregarGridExibicoesAutorizacao()
        {
            if (datePickerInicialAut.SelectedDate != null)
            {
                DateTime dtBusca = DateTime.Parse(datePickerInicialAut.SelectedDate.Value.ToShortDateString());
                ClearGrid(dgAutProgramas);
                ClearGrid(dgAutMusicas);

                List<RowExibicaoAprovacao> exibicoes;

                _genAutoSel = (int) cbGeneroAut.SelectedValue;

                if ((int) cbProgramaAut.SelectedValue == 0)
                {
                    exibicoes = (from sinc in context.Sincronizacoes
                                 join son in context.Sonorizacoes
                                     on sinc.SincronizacaoID equals son.SincronizacaoID
                                 where
                                     _genAutoSel != 0
                                         ? sinc.Exibicao.Programa.GeneroID == _genAutoSel
                                         : sinc.Exibicao.Programa.GeneroID != _genAutoSel
                                           && sinc.Aprovado
                                           && sinc.Exibicao.Data >= dtBusca
                                           && sinc.Exibicao.UnidadeID == UnidadeID
                                           && son.Musica.TipoTrilha.Descricao == "Comercial"
                                           &&
                                           (son.TipoExibicao.Descricao == "VT" ||
                                            son.TipoExibicao.Descricao == "REPRISE")
                                 select new RowExibicaoAprovacao {Exibicao = sinc.Exibicao})
                        .Distinct()
                        .ToList();
                }
                else
                {
                    exibicoes = (from sinc in context.Set<Sincronizacao>()
                                 join son in context.Sonorizacoes
                                     on sinc.SincronizacaoID equals son.SincronizacaoID
                                 where sinc.Exibicao.Programa.GeneroID == _genAutoSel &&
                                       sinc.Aprovado == true &&
                                       sinc.Exibicao.Data >= dtBusca &&
                                       sinc.Exibicao.UnidadeID == UnidadeID &&
                                       sinc.Exibicao.ProgramaID == (int) cbProgramaAut.SelectedValue
                                       && son.Musica.TipoTrilha.Descricao == "Comercial"
                                       &&
                                       (son.TipoExibicao.Descricao == "VT" ||
                                        son.TipoExibicao.Descricao == "REPRISE")
                                 select new RowExibicaoAprovacao {Exibicao = sinc.Exibicao})
                        .Distinct()
                        .ToList();
                }

                dgAutProgramas.ItemsSource = exibicoes.OrderBy(s => s.Exibicao.Data)
                                                      .ThenBy(s => s.Exibicao.Programa.Nome);
            }
            dgAutProgramas.Items.Refresh();
            dgAutProgramas.UnselectAllCells();
            dgAutProgramas.UnselectAll();

            UpdateLayout();
        }

        /// <summary>
        /// Limpa qualquer DataGrid enviado por parametro. Defini ItemsSource como nulo, limpa, executa um refresh e atualiza o layout
        /// </summary>
        /// <param name="dataGrid">Grid que será limpa</param>
        
        public void ClearGrid(DataGrid dataGrid)
        {
            dataGrid.UnselectAll();
            dataGrid.ItemsSource = null;
            dataGrid.Items.Clear();
            dataGrid.Items.Refresh();
            dataGrid.UpdateLayout();
            _sincSel = null;
            UpdateLayout();
        }

        #endregion ### Metodos ###

        private void PesquisarPagamentos(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgPagamentos);

            CarregarGridPagamentos();
        }

        private void CarregarGridPagamentos()
        {
            lblTotalPagamentos.Content = "";
            if (cbEditoraAP.SelectedIndex == -1 && !datePickerVencimentoAP.SelectedDate.HasValue &&
                string.IsNullOrWhiteSpace(txtAP.Text))
            {
                MessageBox.Show("Selecionar editora, data de vencimento ou Numero de AP");
                return;
            }

            int numAP = 0;
            if (!string.IsNullOrWhiteSpace(txtAP.Text))
            {
                if (int.TryParse(txtAP.Text, out numAP))
                {
                    numAP = int.Parse(txtAP.Text);
                }
            }

            var pagamentosMusSon = from aut in context.Autorizacoes
                                   join son in context.Sonorizacoes
                                       on aut.MusicaID equals son.MusicaID
                                   join user in context.Usuarios
                                       on aut.UsuarioID equals user.UsuarioID
                                   join edit in context.Editoras
                                       on aut.EditoraID equals edit.EditoraID
                                   select new RowExibicaoPagamento
                                              {
                                                  AutID = aut.AutorizacaoID,
                                                  Musica = son.Musica.Titulo,
                                                  Vencimento = aut.Vencimento,
                                                  Usuario = user.Login,
                                                  Valor = aut.Valor,
                                                  Porcentagem = aut.Porcentagem,
                                                  Editora = edit.Nome,
                                                  EditoraID = edit.EditoraID,
                                                  AP = aut.AP
                                                  //Observacoes 
                                                  //Exclusão
                                              };
            if (cbEditoraAP.SelectedValue != null && (int) cbEditoraAP.SelectedValue > 0)
            {
                pagamentosMusSon = pagamentosMusSon.Where(p => p.EditoraID == (int) cbEditoraAP.SelectedValue);
            }
            if (datePickerVencimentoAP.SelectedDate.HasValue)
            {
                pagamentosMusSon =
                    pagamentosMusSon.Where(
                        p => EntityFunctions.DiffDays(p.Vencimento, datePickerVencimentoAP.SelectedDate) == 0);
            }
            if (numAP > 0)
            {
                pagamentosMusSon = pagamentosMusSon.Where(p => p.AP == numAP);
            }
            var result = pagamentosMusSon.GroupBy(p => p.AutID);

            dgPagamentos.ItemsSource = result.ToList();
            dgPagamentos.Items.Refresh();
            dgPagamentos.UnselectAllCells();
            dgPagamentos.UnselectAll();
            if (result.Any())
            {
                decimal totalPag = 0;
                foreach (var items in result.ToList())
                {
                    totalPag += items.FirstOrDefault().Valor;
                }
                lblTotalPagamentos.Content = totalPag;
            }
        }

        private void btnSalvarAP_Click(object sender, RoutedEventArgs e)
        {
            int numAP;
            if (!String.IsNullOrWhiteSpace(txtAP.Text) && Int32.TryParse(txtAP.Text, out numAP))
            {
                foreach (var row in dgPagamentos.SelectedItems)
                {
                    var pag = (IGrouping<int, RowRelatorioCanhoto>) row;

                    var aut = (from p in context.Autorizacoes
                               where p.AutorizacaoID == pag.Key
                               select p).SingleOrDefault();
                    if (aut != null)
                    {
                        aut.AP = numAP;
                        context.Autorizacoes.Attach(aut);
                        context.Entry(aut).Property(p => p.AP).IsModified = true;
                        context.SaveChanges();
                    }
                }
                txtAP.Clear();
                ClearGrid(dgPagamentos);
                CarregarGridPagamentos();
            }
        }

        private void ImportarBibliotecaMusical(object sender, RoutedEventArgs e)
        {
            string pasta = null;
            var dialog = new FolderBrowserDialog();

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
                pasta = dialog.SelectedPath;

            //TODO: Retirar linha de codigo de teste
            if (!string.IsNullOrEmpty(pasta))
            {
                var musicas = Directory.EnumerateFiles(pasta, @"*.mp3");
                if (MessageBox.Show(
                    "Foram encontradas " + musicas.Count() + " na pasta. Gostaria de iniciar a importação dos arquivos?",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    ClearGrid(dgMusCadastradas);
                    try
                    {
                        int importadas = 0;
                        int naoImportadas = 0;
                        foreach (var mus in musicas)
                        {
                            using (Repositorio repositorio = new Repositorio())
                            {
                                try
                                {
                                    int musicaID = CriarMusica(mus);
                                    importadas++;
                                    dgMusCadastradas.Items.Add(
                                        repositorio.Obter<Musica>(m => m.MusicaID == musicaID).Single()
                                        );
                                }
                                catch (Exception ex)
                                {
                                    naoImportadas++;
                                    _logRepository.WriteLog(ex);
                                }
                            }
                        }

                        MessageBox.Show("Musicas encontradas: " + musicas.Count() + Environment.NewLine +
                                        "Importadas com sucesso: " + importadas + Environment.NewLine +
                                        "Não importadas: " + naoImportadas + Environment.NewLine,
                                        "Importação");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Importação MP3");
                        _logRepository.WriteLog(ex);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(pasta))
                    MessageBox.Show("Arquivo inválido!");
            }
        }

        private int CriarMusica(string mus)
        {
            TagLib.File mp3 = TagLib.File.Create(mus);

            Musica musica = null;
            using (Repositorio repositorio = new Repositorio())
            {
                var tpTrilha = repositorio.Obter<TipoTrilha>(tt => tt.Descricao == "Biblioteca Musical").Single();

                musica = new Musica()
                             {
                                 TipoTrilha = tpTrilha,
                                 Titulo = mp3.Tag.Title,
                                 AutorID = PesquisaAutorBiblioteca(mp3.Tag.FirstComposer),
                                 //Tem que ser o mesmo do autor para biblioteca musical //PesquisaInterprete(mp3.Tag.FirstPerformer),
                                 InterpreteID = PesquisaInterpreteBiblioteca(mp3.Tag.FirstComposer),
                                 Duracao = mp3.Properties.Duration,
                                 NomeArquivo = mp3.Name,
                                 CadastradaEm = DateTime.Now
                             };
                repositorio.Adicionar(musica);
            }
            return musica.MusicaID;
        }

        private int PesquisaAutorBiblioteca(string nomeAutor)
        {
            Autor autor = null;

            using (Repositorio repositorio = new Repositorio())
            {
                autor = repositorio.Obter<Autor>(a => a.Nome == nomeAutor).SingleOrDefault();

                if (autor == null)
                {
                    autor = new Autor() {Nome = nomeAutor};
                    repositorio.Adicionar(autor);
                }
            }
            return autor.AutorID;
        }

        private int PesquisaInterpreteBiblioteca(string nomeInterprete)
        {
            Interprete interprete = null;

            using (Repositorio repositorio = new Repositorio())
            {
                interprete = repositorio.Obter<Interprete>(i => i.Nome == nomeInterprete).SingleOrDefault();

                if (interprete == null)
                {
                    interprete = new Interprete() {Nome = nomeInterprete};
                    repositorio.Adicionar(interprete);
                }
            }
            return interprete.InterpreteID;
        }

        private void GdgExbDesc_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Context ctx = new Context();
                TipoExibicao tipoExb = e.Row.DataContext as TipoExibicao;

                var _resultado =
                    (from a in ctx.TipoExibicoes where a.TipoExibicaoID == tipoExb.TipoExibicaoID select a)
                        .SingleOrDefault();

                if (_resultado == null)
                {
                    TipoExibicao _tipoExb = new TipoExibicao();
                    _tipoExb.Descricao = tipoExb.Descricao;
                    _tipoExb.Ativo = tipoExb.Ativo;

                    ctx.TipoExibicoes.Add(_tipoExb);
                    context.SaveChanges();

                }
                else
                {
                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        _resultado.Descricao = tipoExb.Descricao;
                        _resultado.Ativo = tipoExb.Ativo;
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {

                    }
                }
            }

        }

        private void GdgNovelasDesc_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var ctx = new Context();
                var view = e.Row.DataContext as ViewNovela;

                var _resultado =
                    (from a in ctx.Novela where a.NovelaID == view.Novela.NovelaID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    Novela _novela = new Novela();
                    _novela.TituloNacional = view.Novela.TituloNacional;
                    _novela.TituloOriginal = view.Novela.TituloOriginal;
                    _novela.Produtor = view.Novela.Produtor;
                    _novela.Pais = view.Novela.Pais;
                    _novela.Autor = view.Novela.Autor;
                    _novela.Ativo = view.Novela.Ativo;
                    _novela.DataFinal = view.Novela.DataFinal;
                    _novela.DataInicial = view.Novela.DataInicial;
                    _novela.Diretor = view.Novela.Diretor;

                    ctx.Novela.Add(_novela);
                    context.SaveChanges();

                }
                else
                {
                    btNovelaSalvar.IsEnabled = false;
                    _resultado.TituloNacional = view.Novela.TituloNacional;
                    _resultado.TituloOriginal = view.Novela.TituloOriginal;
                    _resultado.Diretor = view.Novela.Diretor;
                    _resultado.DataFinal = view.Novela.DataFinal;
                    _resultado.DataInicial = view.Novela.DataInicial;
                    _resultado.Ativo = view.Novela.Ativo;
                    _resultado.Produtor = view.Novela.Produtor;
                 
                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                       MessageBoxResult.Yes)
                    {
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {

                    }
                    btNovelaSalvar.IsEnabled = true;


                }
            }

        }


        private void GdgProgDesc_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {

            if (e.EditAction == DataGridEditAction.Commit)
            {
                Context ctx = new Context();
                Programa tipoProg = e.Row.DataContext as Programa;

                var _resultado =
                    (from a in ctx.Programas where a.ProgramaID == tipoProg.ProgramaID select a).SingleOrDefault();

                if (_resultado == null)
                {


                }
                else
                {
                    _resultado.Nome = tipoProg.Nome;
                    _resultado.Ordem = tipoProg.Ordem;
                    _resultado.Genero = tipoProg.Genero;
                    _resultado.Ativo = tipoProg.Ativo;
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");

                    }

                }
                    else
                    {
                                         

                    } 
                }
            }


        private void gdg_ClassRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Context ctx = new Context();
                Classificacao tipoCls = e.Row.DataContext as Classificacao;

                var _resultado =
                    (from a in ctx.Classificacoes where a.ClassificacaoID == tipoCls.ClassificacaoID select a)
                        .SingleOrDefault();

                if (_resultado == null)
                {
                    Classificacao _tipoExb = new Classificacao();
                    _tipoExb.Descricao = tipoCls.Descricao;
                    _tipoExb.Ativo = tipoCls.Ativo;

                    ctx.Classificacoes.Add(_tipoExb);
                    context.SaveChanges();

                }
                else
                {
                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        _resultado.Descricao = tipoCls.Descricao;
                        _resultado.Ativo = tipoCls.Ativo;
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {

                    } 

                }
            }

        }

        private void gdg_QuadroRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            btQuadroSalvar.IsEnabled = false;
                
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var ctx = new Context();
                var view = e.Row.DataContext as ViewQuadro;

                var _resultado =
                    (from a in ctx.Quadros where a.QuadroID == view.Quadro.QuadroID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    Quadro _tipoExb = new Quadro();
                    _tipoExb.Descricao = view.Quadro.Descricao;
                    _tipoExb.Ativo = view.Quadro.Ativo;

                    ctx.Quadros.Add(_tipoExb);
                    context.SaveChanges();

                }
                else
                {
                    _resultado.Descricao = view.Quadro.Descricao;
                    _resultado.Ativo = view.Quadro.Ativo;
                    
                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        private void gdg_AssRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var ctx = new Context();
                var tipoAss = e.Row.DataContext as Associacao;

                var _resultado =
                    (from a in ctx.Associacoes where a.AssociacaoID == tipoAss.AssociacaoID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    Associacao _tipoAss = new Associacao();
                    _tipoAss.Ativo = tipoAss.Ativo;
                    _tipoAss.Nome = tipoAss.Nome;

                    ctx.Associacoes.Add(_tipoAss);
                    context.SaveChanges();

                }
                else
                {

                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        _resultado.Ativo = tipoAss.Ativo;
                        _resultado.Nome = tipoAss.Nome;
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {

                    } 
                   
                    
                }
            }

        }

        private void gdg_GenRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var ctx = new Context();
                var tipoGen = e.Row.DataContext as Genero;

                var _resultado = (from a in ctx.Generos where a.GeneroID == tipoGen.GeneroID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    Genero _tipoGen = new Genero();
                    _tipoGen.Descricao = tipoGen.Descricao;
                    _tipoGen.Classificacoes = tipoGen.Classificacoes;
                    _tipoGen.Programas = tipoGen.Programas;
                    _tipoGen.Ativo = tipoGen.Ativo;

                    ctx.Generos.Add(_tipoGen);
                    context.SaveChanges();

                }
                else
                {

                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        _resultado.Descricao = tipoGen.Descricao;
                        _resultado.Classificacoes = tipoGen.Classificacoes;
                        _resultado.Programas = tipoGen.Programas;
                        _resultado.Ativo = tipoGen.Ativo;
                        context.SaveChanges();                        
                    }
                    else
                    {

                    }
                }
            }

        }


        private void gdg_MusicEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var ctx = new Context();
                var tipoMus = e.Row.DataContext as Musica;

                var _resultado = (from a in ctx.Musicas where a.MusicaID == tipoMus.MusicaID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    //Musica _tipoMus = new Musica();
                    //_tipoMus.Titulo = tipoMus.Titulo;
                    //_tipoMus.Autor = tipoMus.Autor;
                    //_tipoMus.Interprete = tipoMus.Interprete;
                    //_tipoMus.TipoTrilha = tipoMus.TipoTrilha;

                    //context.Musicas.Add(_tipoMus);
                    //context.SaveChanges();

                    MessageBox.Show("Para cadastrar uma música utilize os campos acima!");

                }
                else
                {
                    _resultado.Ativo = tipoMus.Ativo;
                    //_resultado.Titulo = tipoMus.Titulo;
                    //_resultado.Autor = tipoMus.Autor;
                    //_resultado.Interprete = tipoMus.Interprete;
                    //_resultado.TipoTrilha = tipoMus.TipoTrilha;

                    context.SaveChanges();
                }
            }
        }

        private void gdg_EditoraRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Context ctx = new Context();
                Editora tipoEdt = e.Row.DataContext as Editora;

                var _resultado =
                    (from a in ctx.Editoras where a.EditoraID == tipoEdt.EditoraID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    Editora _tipoEdt = new Editora();
                    _tipoEdt.Ag = tipoEdt.Ag;
                    _tipoEdt.Associacao = tipoEdt.Associacao;
                    _tipoEdt.Ativo = tipoEdt.Ativo;
                    _tipoEdt.Bairro = tipoEdt.Bairro;
                    _tipoEdt.Banco = tipoEdt.Banco;
                    _tipoEdt.Cep = tipoEdt.Cep;
                    _tipoEdt.Cidade = tipoEdt.Cidade;
                    _tipoEdt.CNPJ = tipoEdt.CNPJ;
                    _tipoEdt.CNPJJ = tipoEdt.CNPJJ;
                    _tipoEdt.Complemento = tipoEdt.Complemento;
                    _tipoEdt.Conta = tipoEdt.Conta;
                    _tipoEdt.Contato = tipoEdt.Contato;
                    _tipoEdt.Contato2 = tipoEdt.Contato2;
                    _tipoEdt.CPF = tipoEdt.CPF;
                    _tipoEdt.DDD = tipoEdt.DDD;
                    _tipoEdt.DDD1 = tipoEdt.DDD1;
                    _tipoEdt.Email = tipoEdt.Email;
                    _tipoEdt.Email2 = tipoEdt.Email2;
                    _tipoEdt.Endereco = tipoEdt.Endereco;
                    _tipoEdt.Fone = tipoEdt.Fone;
                    _tipoEdt.Fone1 = tipoEdt.Fone1;
                    _tipoEdt.Nome = tipoEdt.Nome;
                    _tipoEdt.Numero = tipoEdt.Numero;
                    _tipoEdt.UF = tipoEdt.UF;
                    _tipoEdt.RazaoSocial = tipoEdt.RazaoSocial;


                    context.Editoras.Add(_tipoEdt);
                    context.SaveChanges();

                }
                else
                {
                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        _resultado.Ag = tipoEdt.Ag;
                        _resultado.Associacao = tipoEdt.Associacao;
                        _resultado.Ativo = tipoEdt.Ativo;
                        _resultado.Bairro = tipoEdt.Bairro;
                        _resultado.Cep = tipoEdt.Cep;
                        _resultado.Cidade = tipoEdt.Cidade;
                        _resultado.CNPJ = tipoEdt.CNPJ;
                        _resultado.CNPJJ = tipoEdt.CNPJJ;
                        _resultado.Complemento = tipoEdt.Complemento;
                        _resultado.Conta = tipoEdt.Conta;
                        _resultado.Contato = tipoEdt.Contato;
                        _resultado.Contato2 = tipoEdt.Contato2;
                        _resultado.CPF = tipoEdt.CPF;
                        _resultado.DDD = tipoEdt.DDD;
                        _resultado.DDD1 = tipoEdt.DDD1;
                        _resultado.Email = tipoEdt.Email;
                        _resultado.Email2 = tipoEdt.Email2;
                        _resultado.Endereco = tipoEdt.Endereco;
                        _resultado.Fone = tipoEdt.Fone;
                        _resultado.Fone1 = tipoEdt.Fone1;
                        _resultado.Nome = tipoEdt.Nome;
                        _resultado.Numero = tipoEdt.Numero;
                        _resultado.UF = tipoEdt.UF;
                        _resultado.RazaoSocial = tipoEdt.RazaoSocial;
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {

                    } 
                }
            }
        }

        private void gdg_PrecoRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Context ctx = new Context();
                Preco tipoPrc = e.Row.DataContext as Preco;

                var _resultado = (from a in ctx.Precos where a.PrecoID == tipoPrc.PrecoID select a).SingleOrDefault();

                if (_resultado == null)
                {
                    Preco _tipoPrc = new Preco();
                    _tipoPrc.Valor = tipoPrc.Valor;
                    _tipoPrc.Ativo = tipoPrc.Ativo;
                    _tipoPrc.Abrangencia = tipoPrc.Abrangencia;
                    _tipoPrc.Vigencia = tipoPrc.Vigencia;

                    context.Precos.Add(_tipoPrc);
                    context.SaveChanges();

                }
                else
                {
                    if (MessageBox.Show("Deseja modificar os dados?", "Confirmação", MessageBoxButton.YesNo) ==
                        MessageBoxResult.Yes)
                    {
                        _resultado.Valor = tipoPrc.Valor;
                        _resultado.Ativo = tipoPrc.Ativo;
                        _resultado.Abrangencia = tipoPrc.Abrangencia;
                        _resultado.Vigencia = tipoPrc.Vigencia;
                        
                        context.SaveChanges();
                        MessageBox.Show("Dados modificados com sucesso!");
                    }
                    else
                    {

                    } 
                }
            }
        }


        private void cbGenNovela_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbPrecoAss_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
     
        private void cbEditoraAP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbECADGerar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbQuadroProg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }       

        private void cbAssociacaoAP_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbGenero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbPrecoClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbPrecoGen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbProgGen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbProgQuadro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbAbragencia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void dgPagamentos_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
        }

        private void cbMusTipoTrilha_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbRakingTipoTrilha_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbAbrangencia_SelectionChanged(object sender, SelectedCellsChangedEventArgs e)
        {
        }

        private void SalvarMusica(object sender, RoutedEventArgs e)
        {
            lblMusErro.Content = "";
            if (cbMusTipoTrilha.SelectedIndex != -1)
            {
                TimeSpan duracaoMusica = TimeSpan.TryParse(txtMusDuracao.Text, out duracaoMusica)
                                             ? TimeSpan.Parse(txtMusDuracao.Text)
                                             : new TimeSpan(0);

                //•	A informação ISRC será obrigatória somente para as trilhas Record e Comercial.
                if (((TipoTrilha) cbMusTipoTrilha.SelectedItem).Descricao.ToUpper() != "BIBLIOTECA MUSICAL")
                {
                    //•	A informação ISRC será obrigatória somente para as trilhas Record e Comercial.
                    if (string.IsNullOrWhiteSpace(txtMusISRC.Text))
                    {
                        lblMusErro.Content = "Campo ISRC é obrigatório";
                    }
                    else
                    {
                        //o	Na inclusão verificar: se a musica existe pelo campo ISRC.
                        if (ExisteISRC(txtMusISRC.Text))
                        {
                            if (_musEditSel > 0)
                            {
                                CadastrarMusica(cbMusTipoTrilha.SelectedItem as TipoTrilha, txtMusTitulo.Text,
                                                txtMusAutor.Text, txtMusInterprete.Text,
                                                duracaoMusica, DateTime.Now, txtMusArquivo.Text, txtMusISRC.Text);
                                ClearGrid(dgMusCadastradas);
                                var musicaParaSalvar = context.Musicas.FirstOrDefault(m => m.ISRC == txtMusISRC.Text);
                                dgMusCadastradas.Items.Add(musicaParaSalvar);
                                dgMusCadastradas.UpdateLayout();

                                LimparTextosFormularios(formCadastroMusica);
                            }
                            else
                            {
                                //	Caso exista, dar aviso de musica já cadastrada e não incluir para qualquer tipo de trilha.
                                lblMusErro.Content = "Esta música já existe na base de dados";
                            }
                        }
                        else
                        {
                            CadastrarMusica(cbMusTipoTrilha.SelectedItem as TipoTrilha, txtMusTitulo.Text,
                                            txtMusAutor.Text, txtMusInterprete.Text,
                                            duracaoMusica, DateTime.Now, txtMusArquivo.Text, txtMusISRC.Text);

                            ClearGrid(dgMusCadastradas);
                            if (context.Musicas.Any(m => m.ISRC == txtMusISRC.Text))
                            {
                                var musicaParaSalvar = context.Musicas.FirstOrDefault(m => m.ISRC == txtMusISRC.Text);
                                dgMusCadastradas.Items.Add(musicaParaSalvar);
                                _logRepository.WriteLog("Salvar Musica", LogType.Informacao, _usuario.Login);
                            }
                            dgMusCadastradas.UpdateLayout();

                            LimparTextosFormularios(formCadastroMusica);
                        }
                    }
                }
                else
                {
                    CadastrarMusica(cbMusTipoTrilha.SelectedItem as TipoTrilha, txtMusTitulo.Text, txtMusAutor.Text,
                                    txtMusInterprete.Text,
                                    duracaoMusica, DateTime.Now, txtMusArquivo.Text, "");

                    ClearGrid(dgMusCadastradas);
                    if (context.Musicas.Any(m => m.ISRC == txtMusISRC.Text))
                    {
                        var musicaParaSalvar = context.Musicas.FirstOrDefault(m => m.ISRC == txtMusISRC.Text);
                        dgMusCadastradas.Items.Add(musicaParaSalvar);
                    }
                    dgMusCadastradas.UpdateLayout();

                    LimparTextosFormularios(formCadastroMusica);
                }
            }
            else
            {
                lblMusErro.Content = "Preencha o formulário";
            }
        }

        /// <summary>
        /// Limpa textos do formulario/grid passado por parametro
        /// </summary>
        /// <param name="formulario"></param>
        private void LimparTextosFormularios(DependencyObject formulario)
        {
            foreach (TextBox txtBox in FindVisualChildren<TextBox>(formulario))
            {
                txtBox.Clear();
            }
            lblMusErro.Content = "";
        }

        /// <summary>
        /// Caixa alta para textos do formulario/grid passado por parametro
        /// </summary>
        /// <param name="formulario"></param>
        private void ToUpperTextosFormularios(DependencyObject formulario)
        {
            foreach (TextBox txtBox in FindVisualChildren<TextBox>(formulario))
            {
                txtBox.Text = txtBox.Text.ToUpper();
            }
        }

        /// <summary>
        /// Encontra elementos do tipo desejado no objeto passado por parametro
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T) child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// Verifica se a musica ja existe usando Titulo, autor, e interprete
        /// </summary>
        /// <param name="txtMusTitulo"></param>
        /// <param name="txtMusAutor"></param>
        /// <param name="txtMusInterprete"></param>
        /// <param name="titulo"></param>
        /// <returns></returns>
        private bool ExisteTrilha(string titulo, string autor, string interprete)
        {
            var existeTrilha = (from m in context.Musicas
                                where m.Titulo == titulo
                                      && m.Autor.Nome == autor
                                      && m.Interprete.Nome == interprete
                                select m).Any();
            return existeTrilha;
        }

        /// <summary>
        /// Verifica se a musica ja existe no banco pelo ISRC
        /// </summary>
        /// <param name="ISRC"></param>
        /// <returns></returns>
        private bool ExisteISRC(string ISRC)
        {
            var existeISRC = (from i in context.Musicas
                              where i.ISRC == ISRC
                              select i).Any();
            return existeISRC;
        }

        private bool ExisteDesc(string nome)
        {
            var existeDesc = (from p in context.Programas
                              where p.Nome == nome
                              select p).Any();
            return existeDesc;
        }


        /// <summary>
        /// /// Cadastra Musicas
        /// Tipo de trilha, Título, Autor, Interprete, Duração da musica, Data de cadastro, Arquivo de áudio (nome do arquivo) e ISRC
        /// </summary>
        /// <param name="tt"></param>
        /// <param name="titulo"></param>
        /// <param name="aut"></param>
        /// <param name="inter"></param>
        /// <param name="duracao"></param>
        /// <param name="dtCad"></param>
        /// <param name="arquivo"></param>
        /// <param name="ISRC"></param>
        private void CadastrarMusica(TipoTrilha tt, string titulo, string aut, string inter, TimeSpan duracao,
                                     DateTime dtCad, string arquivo, string ISRC)
        {
            //verifica se autor existe
            Autor autor = (from a in context.Autores
                           where a.Nome == aut
                           select a).SingleOrDefault();
            if (autor == null)
                autor = new Autor() {Nome = aut};
            //verifica se interprete existe
            Interprete interprete = (from i in context.Interpretes
                                     where i.Nome == inter
                                     select i).SingleOrDefault();
            TipoTrilha tipoTrilha = (from t in context.TipoTrilhas
                                     where t.TipoTrilhaID == tt.TipoTrilhaID
                                     select t).SingleOrDefault();

            if (interprete == null)
                interprete = new Interprete() {Nome = aut};

            Musica musica = null;

            if (_musEditSel > 0)
            {
                musica = (from m in context.Musicas
                          where m.MusicaID == _musEditSel
                          select m).SingleOrDefault();
                musica.TipoTrilha = tipoTrilha;
                musica.Titulo = titulo;
                musica.Autor = autor;
                musica.Interprete = interprete;
                musica.CadastradaEm = dtCad;
                musica.NomeArquivo = arquivo;
                musica.ISRC = ISRC;
                musica.Duracao = duracao;
                context.Entry(musica).State = EntityState.Modified;
            }
            else
            {
                musica = new Musica()
                             {
                                 TipoTrilha = tipoTrilha,
                                 Titulo = titulo,
                                 Autor = autor,
                                 Interprete = interprete,
                                 CadastradaEm = dtCad,
                                 NomeArquivo = arquivo,
                                 ISRC = ISRC,
                                 Duracao = duracao
                             };
                context.Musicas.Add(musica);
            }
            context.SaveChanges();
            _musEditSel = 0;
        }

        private void PesquisarMusicasCadastradas(object sender, RoutedEventArgs e)
        {
            if (chkMusicaAtivo.IsChecked == true)
            {
                btnMusSalvar.IsEnabled = false;
                lblMusErro.Content = "";
                DateTime dtInicial = datePickerMusInicial.SelectedDate.HasValue
                                         ? DateTime.Parse(datePickerMusInicial.SelectedDate.Value.ToShortDateString())
                                         : DateTime.Now;
                DateTime dtFinal = datePickerMusFinal.SelectedDate.HasValue
                                       ? DateTime.Parse(datePickerMusFinal.SelectedDate.Value.ToShortDateString())
                                       : DateTime.Now;
                dtFinal = dtFinal.AddDays(1);
                ClearGrid(dgMusCadastradas);

                ToUpperTextosFormularios(formCadastroMusica);
                var musicas =
                    context.Musicas
                           .Include("Autor")
                           .Include("Interprete")
                           .Include("TipoTrilha")
                           .Where(m => m.CadastradaEm >= dtInicial)
                           .Where(m => m.CadastradaEm < dtFinal)
                           .Where(m => m.Ativo)
                           .Where(
                               m =>
                               (int) cbMusTipoTrilha.SelectedValue != 0
                                   ? m.TipoTrilhaID == (int) cbMusTipoTrilha.SelectedValue
                                   : m.TipoTrilhaID > 0);

                if (!string.IsNullOrWhiteSpace(txtMusISRC.Text))
                    musicas = musicas.Where(m => m.ISRC.Contains(txtMusISRC.Text));

                if (!string.IsNullOrWhiteSpace(txtMusTitulo.Text))
                    musicas = musicas.Where(m => m.Titulo.Contains(txtMusTitulo.Text));

                if (!string.IsNullOrWhiteSpace(txtMusAutor.Text))
                    musicas = musicas.Where(m => m.Autor.Nome.Contains(txtMusAutor.Text));

                if (!string.IsNullOrWhiteSpace(txtMusInterprete.Text))
                    musicas = musicas.Where(m => m.Interprete.Nome.Contains(txtMusInterprete.Text));



                dgMusCadastradas.ItemsSource = musicas.ToList();

                dgMusCadastradas.UnselectAllCells();
                dgMusCadastradas.UnselectAll();
                dgMusCadastradas.Visibility = Visibility.Visible;
                dgMusCadastradas.UpdateLayout();

                dgMusCadastradas.Height = ActualHeight - 450;
                dgMusCadastradas.Width = ActualWidth - 100;
                //stackGridsMusicas.Height = dgMusCadastradas.Height + 20;
                _MusicasParaEmail = new List<VetrixMusica>();
                foreach (var musica in musicas.ToList())
                {
                    VetrixMusica vetrixMusica = new VetrixMusica()
                        {
                            Arquivo = musica.NomeArquivo,
                            Musica = musica.Titulo,
                            Autor = musica.Autor.Nome,
                            Interprete = musica.Interprete.Nome,
                            TipoTrilha = musica.TipoTrilha.Descricao,
                            CadastradaEm = musica.CadastradaEm,
                        };

                    _MusicasParaEmail.Add(vetrixMusica);
                }
            }
            else
            {
                lblMusErro.Content = "";
                DateTime dtInicial = datePickerMusInicial.SelectedDate.HasValue
                                         ? DateTime.Parse(datePickerMusInicial.SelectedDate.Value.ToShortDateString())
                                         : DateTime.Now;
                DateTime dtFinal = datePickerMusFinal.SelectedDate.HasValue
                                       ? DateTime.Parse(datePickerMusFinal.SelectedDate.Value.ToShortDateString())
                                       : DateTime.Now;
                dtFinal = dtFinal.AddDays(1);
                ClearGrid(dgMusCadastradas);

                ToUpperTextosFormularios(formCadastroMusica);
                var musicas =
                    context.Musicas
                           .Include("Autor")
                           .Include("Interprete")
                           .Include("TipoTrilha")
                           .Where(m => m.CadastradaEm >= dtInicial)
                           .Where(m => m.CadastradaEm < dtFinal)
                           .Where(m => m.Ativo != true)
                           .Where(
                               m =>
                               (int) cbMusTipoTrilha.SelectedValue != 0
                                   ? m.TipoTrilhaID == (int) cbMusTipoTrilha.SelectedValue
                                   : m.TipoTrilhaID > 0);

                if (!string.IsNullOrWhiteSpace(txtMusISRC.Text))
                    musicas = musicas.Where(m => m.ISRC.Contains(txtMusISRC.Text));

                if (!string.IsNullOrWhiteSpace(txtMusTitulo.Text))
                    musicas = musicas.Where(m => m.Titulo.Contains(txtMusTitulo.Text));

                if (!string.IsNullOrWhiteSpace(txtMusAutor.Text))
                    musicas = musicas.Where(m => m.Autor.Nome.Contains(txtMusAutor.Text));

                if (!string.IsNullOrWhiteSpace(txtMusInterprete.Text))
                    musicas = musicas.Where(m => m.Interprete.Nome.Contains(txtMusInterprete.Text));



                dgMusCadastradas.ItemsSource = musicas.ToList();

                dgMusCadastradas.UnselectAllCells();
                dgMusCadastradas.UnselectAll();
                dgMusCadastradas.Visibility = Visibility.Visible;
                dgMusCadastradas.UpdateLayout();

                dgMusCadastradas.Height = ActualHeight - 450;
                dgMusCadastradas.Width = ActualWidth - 100;
                //stackGridsMusicas.Height = dgMusCadastradas.Height + 20;
                _MusicasParaEmail = new List<VetrixMusica>();
                foreach (var musica in musicas.ToList())
                {
                    VetrixMusica vetrixMusica = new VetrixMusica()
                    {
                        Arquivo = musica.NomeArquivo,
                        Musica = musica.Titulo,
                        Autor = musica.Autor.Nome,
                        Interprete = musica.Interprete.Nome,
                        TipoTrilha = musica.TipoTrilha.Descricao,
                        CadastradaEm = musica.CadastradaEm,
                    };

                    _MusicasParaEmail.Add(vetrixMusica);
                }
            }

        }

        private void btnMusProcurarArquivo_Click(object sender, RoutedEventArgs e)
        {
            string arquivo = "";
            var dialog = new OpenFileDialog {DefaultExt = ".mp3", InitialDirectory = "C:\\", RestoreDirectory = true};

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
                arquivo = dialog.FileName;

            var extension = Path.GetExtension(arquivo);
            if (extension != null && (!string.IsNullOrEmpty(arquivo) &&
                                      (extension.Equals(".mp3", StringComparison.CurrentCultureIgnoreCase)))
                )
            {
                TagLib.File mp3 = TagLib.File.Create(arquivo);
                txtMusTitulo.Text = mp3.Tag.Title;
                txtMusAutor.Text = mp3.Tag.FirstComposer;
                txtMusInterprete.Text = mp3.Tag.FirstPerformer;
                txtMusDuracao.Text = mp3.Properties.Duration.ToString();
                txtMusArquivo.Text = mp3.Name;
            }
        }

        private void SelecionaMusicaCadastrada(object sender, SelectedCellsChangedEventArgs e)
        {
            if ((dgMusCadastradas.Items.Count > 0) &&
                (dgMusCadastradas.Columns.Count > 0))
            {
                if (dgMusCadastradas.SelectedIndex >= 0)
                {
                    if (dgMusCadastradas.CurrentCell.Item.ToString() != "{DependencyProperty.UnsetValue}")
                    {
                        _musEditSel = ((Musica) dgMusCadastradas.CurrentCell.Item).MusicaID;

                        using (Context context = new Context())
                        {
                            Musica musica = (from m in context.Musicas
                                             where m.MusicaID == _musEditSel
                                             select m).SingleOrDefault();

                            if (musica != null)
                            {
                                txtMusArquivo.Text = musica.NomeArquivo;
                                txtMusAutor.Text = musica.Autor.Nome;
                                txtMusDuracao.Text = musica.Duracao.ToString();
                                txtMusInterprete.Text = musica.Interprete.Nome;
                                txtMusISRC.Text = musica.ISRC;
                                txtMusTitulo.Text = musica.Titulo;
                                cbMusTipoTrilha.SelectedValue = musica.TipoTrilhaID;
                                _musEditSel = musica.MusicaID;
                            }
                        }
                    }
                }
                else
                {
                    txtMusArquivo.Clear();
                    txtMusAutor.Clear();
                    txtMusDuracao.Clear();
                    txtMusInterprete.Clear();
                    txtMusISRC.Clear();
                    txtMusTitulo.Clear();
                    cbMusTipoTrilha.SelectedValue = 0;
                    cbMusTipoTrilha.SelectedIndex = 0;
                    _musEditSel = -1;
                }
            }
        }

        private void SelecionaExibicao(object sender, SelectedCellsChangedEventArgs e)
        {
            if ((GdgExbDesc.Items.Count > 0) &&
                (GdgExbDesc.Columns.Count > 0))
            {
                if (GdgExbDesc.SelectedIndex >= 0)
                {
                    if (GdgExbDesc.CurrentCell.Item.ToString() != "{DependencyProperty.UnsetValue}")
                    {
                        _exbEditSel = ((TipoExibicao) GdgExbDesc.CurrentCell.Item).TipoExibicaoID;

                        using (Context context = new Context())
                        {
                            TipoExibicao exb = (from m in context.TipoExibicoes
                                                where m.TipoExibicaoID == _exbEditSel
                                                select m).SingleOrDefault();

                            if (exb != null)
                            {
                                txbExbDesc.Text = exb.Descricao;
                                _exbEditSel = exb.TipoExibicaoID;
                            }
                        }
                    }
                }
                else
                {
                    txbExbDesc.Clear();
                    _exbEditSel = -1;
                }
            }


        }

        private void dgECAD_SelectedCellsChanged_1(object sender, SelectedCellsChangedEventArgs e)
        {
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }





        private void LimpaPrograma(object sender, RoutedEventArgs e)
        {
            btProgSalvar.IsEnabled = true;
            ClearGrid(GdgProgDesc);
            GdgProgDesc.Visibility = Visibility.Hidden;
            txbProgDesc.Clear();
            cbxProgGen.SelectedValue = -1;
            txbProgOrdem.Clear();

        }

        private void LimpaTpExb(object sender, RoutedEventArgs e)
        {
            btExbSalvar.IsEnabled = true;
            ClearGrid(GdgExbDesc);
            GdgExbDesc.Visibility = Visibility.Hidden;

            txbExbDesc.Clear();
        }

        private void LimpaGenero(object sender, RoutedEventArgs e)
        {
            ClearGrid(gdgGenDesc);
            gdgGenDesc.Visibility = Visibility.Hidden;

            txbGenNome.Clear();
            lstGen.Items.Clear();
            cbxGenClass.SelectedValue = -1;
        }

        private void LimpaAss(object sender, RoutedEventArgs e)
        {
            ClearGrid(gdgAssDesc);
            gdgAssDesc.Visibility = Visibility.Hidden;

            txbAssNome.Clear();
         
        }
                

        private void LimpaEditora(object sender, RoutedEventArgs e)
        {
            ClearGrid(GdgExbDesc);
            gdgEditoraDesc.Visibility = Visibility.Hidden;

            txbEditoraNome.Clear();
            cbxEditoraAss.SelectedValue = -1;
            txbEditoraRazao.Clear();
            txbEditoraCNPJ.Clear();
            txbEditoraEnd.Clear();
            txbEditoraCep.Clear();
            txbEditoraBairro.Clear();
            txbEditoraNomeC.Clear();
            txbEditoraContato.Clear();
            txbEditoraMail.Clear();
            txbEditoraNomeC1.Clear();
            txbEditoraContato1.Clear();
            txbEditoraMail1.Clear();
            txbEditoraBanco.Clear();
            txbEditoraAg.Clear();
            txbEditoraNomeCPF.Clear();
            txbEditoraCC.Clear();
            txbEditoraCidade.Clear();

        }

        private void SalvarGravadora(object sender, RoutedEventArgs e)
        {
            if (txbGravadoraaNome.Text != "")
            {
                if (!context.TipoExibicoes.Any(m => m.Descricao == txbExbDesc.Text))
                {
                    var gvd = new Gravadora
                        {
                            Nome = txbGravadoraaNome.Text.Normalizar(),
                            RazaoSocial = txbGravadoraRazao.Text.Normalizar(),
                            CNPJ = txbGravadoraCNPJ.Text.ValidacaoCnpj(),
                            CNPJJ = txbGravadoraCNPJ.Text,
                            Endereco = txbGravadoraEnd.Text.Normalizar(),
                            Numero = txbGravadoraN.Text,
                            Bairro = txbGravadoraBairro.Text.Normalizar(),
                            Cep = txbGravadoraCep.Text,
                            Complemento = txbGravadoraComp.Text.Normalizar(),
                            Email = txbGravadoraMail.Text.Normalizar(),
                            Email2 = txbGravadoraMail1.Text.Normalizar(),
                            DDD = txbGravadoraDDD.Text,
                            DDD1 = txbGravadoraDDD1.Text,
                            Fone = txbGravadoraContato.Text,
                            Fone1 = txbGravadoraContato1.Text,
                            Contato = txbGravadoraNomeC.Text.Normalizar(),
                            Contato2 = txbGravadoraNomeC1.Text.Normalizar(),
                            Ativo = chkGravadoraAtivo.IsChecked == true
                        };

                    if (txbGravadoraCNPJ.Text.ValidacaoCnpj())
                    {
                        context.Gravadoras.Add(gvd);
                        context.SaveChanges();

                        MessageBox.Show("Dados salvos com sucesso!");
                        txbGravadoraaNome.Clear();
                        txbGravadoraRazao.Clear();
                        txbGravadoraCNPJ.Clear();
                        txbGravadoraEnd.Clear();
                        txbGravadoraN.Clear();
                        txbGravadoraBairro.Clear();
                        txbGravadoraCep.Clear();
                        txbGravadoraComp.Clear();
                        txbGravadoraMail.Clear();
                        txbGravadoraMail1.Clear();
                        txbGravadoraDDD.Clear();
                        txbGravadoraDDD1.Clear();
                        txbGravadoraContato.Clear();
                        txbGravadoraContato1.Clear();
                    }
                    else
                    {
                        MessageBox.Show("CNPJ Inválido!");
                    }

                }
                else
                {
                    MessageBox.Show("Alerta!\n Gravadora já está cadastrada");

                }
            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
            }
        }



        private void LimpaGravadora(object sender, RoutedEventArgs e)
        {
            ClearGrid(gdgGravadoraDesc);
            gdgGravadoraDesc.Visibility = Visibility.Hidden;
            txbGravadoraaNome.Clear();
            txbGravadoraRazao.Clear();
            txbGravadoraCNPJ.Clear();
            txbGravadoraEnd.Clear();
            txbGravadoraN.Clear();
            txbGravadoraBairro.Clear();
            txbGravadoraCep.Clear();
            txbGravadoraComp.Clear();
            txbGravadoraMail.Clear();
            txbGravadoraMail1.Clear();
            txbGravadoraDDD.Clear();
            txbGravadoraDDD1.Clear();
            txbGravadoraContato.Clear();
            txbGravadoraContato1.Clear();
        }

        private void LimpaPreco(object sender, RoutedEventArgs e)
        {
            ClearGrid(GdgExbDesc);
            gdgPrecoDesc.Visibility = Visibility.Hidden;
            txbPrecoValor.Clear();
            lstPreco.SelectedItem = "";
            cbxPrecoAno.SelectedValue = -1;
            cbxPrecoGen.SelectedValue = -1;
            cbxPrecoClass.SelectedValue = -1;
            cbxPrecoAbr.SelectedValue = -1;
        }



        private void LimpaNovela(object sender, RoutedEventArgs e)
        {
            btNovelaSalvar.IsEnabled = true;
            ClearGrid(GdgNovelasDesc);
            GdgNovelasDesc.Visibility = Visibility.Hidden;
     
            cbNovelaGen.SelectedValue = -1;
            cbNovelaProg.SelectedValue = -1;
            txbNovelaTituloN.Clear();
            txbNovelaTituloO.Clear();
            txbNovelaProdutor.Clear();
            txbNovelaAutor.Clear();
            txbNovelaDiretor.Clear();           
            cbNovelaPais.Text = "";
            dateNovelaInicial.Text = "";
            dateNovelaFinal.Text = "";
        }



        private void LimpaQuadro(object sender, RoutedEventArgs e)
        {
            ClearGrid(GdgExbDesc);
            gdgQuadroDesc.Visibility = Visibility.Hidden;
            txbQuadrosNome.Clear();
            cbQuadroProg.SelectedValue = -1;
            //chkAtivo.IsChecked = false;
            btQuadroSalvar.IsEnabled = true;

        }


        private void LimpaClass(object sender, RoutedEventArgs e)
        {
            ClearGrid(GdgExbDesc);
            gdgClassDesc.Visibility = Visibility.Hidden;

            txbClassNome.Clear();
        }


        private void LimpaTabMusicas(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgMusCadastradas);
            dgMusCadastradas.Visibility = Visibility.Hidden;

            txtMusArquivo.Clear();
            txtMusAutor.Clear();
            txtMusDuracao.Clear();
            txtMusInterprete.Clear();
            txtMusISRC.Clear();
            txtMusTitulo.Clear();
            cbMusTipoTrilha.SelectedValue = 0;
            cbMusTipoTrilha.SelectedIndex = 0;

            dgMusCadastradas.UnselectAll();
            dgMusCadastradas.UnselectAllCells();
            lblMusErro.Content = "";
        }

        private void LimparTabAutorizacao(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgAutProgramas);
            ClearGrid(dgAutMusicas);

            dgAutProgramas.Visibility = Visibility.Hidden;
            dgAutMusicas.Visibility = Visibility.Hidden;

            cbGenero.SelectedIndex = -1;
            cbProgramaAut.SelectedIndex = -1;

            dgAutProgramas.UnselectAll();
            dgAutProgramas.UnselectAllCells();

            dgAutMusicas.UnselectAll();
            dgAutMusicas.UnselectAllCells();
        }

        private void GerarECAD(object sender, RoutedEventArgs e)
        {
            if (cbECADTipo.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Tipo");
                return;
            }
            if (cbECADMes.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Mês");
                return;
            }
            if (cbECADAno.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Ano");
                return;
            }
            ClearGrid(dgECAD);
            UpdateLayout();
            dgECAD.Visibility = Visibility.Hidden;

            if (cbECADMes.SelectedIndex != -1 && cbECADAno.SelectedIndex != -1)
            {
                ShowLoader();

                var unidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
                var mes = (int) cbECADMes.SelectedValue;
                var ano = (int) cbECADAno.SelectedValue;
                var tipoRelatorio = int.Parse(cbECADTipo.SelectedValue.ToString());
                List<RowRelatorioECAD> retorno = new List<RowRelatorioECAD>();

                ThreadStart dataDownloadThread = delegate
                                                     {

                                                         RelatorioECAD ecad = new RelatorioECAD();
                                                         retorno = ecad.PesquisarSonorizacoes(
                                                             unidadeID, mes, ano, tipoRelatorio
                                                             );
                                                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                (EventHandler)
                                                                                delegate
                                                                                    {
                                                                                        dgECAD.ItemsSource = retorno;
                                                                                        dgECAD.Height =
                                                                                            ActualHeight - 360;
                                                                                        dgECAD.Width = ActualWidth -
                                                                                                          100;
                                                                                        HideLoader();
                                                                                        if (retorno != null)
                                                                                        dgECAD.Visibility =
                                                                                            Visibility.Visible;
                                                                                    }, null, null);
                                                     };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);

                _logRepository.WriteLog("Relatório ECAD", LogType.Informacao, _usuario.Login);
            }
        }

        private void GerarPgtoAberto(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgPgtoAberto);
            UpdateLayout();
            dgPgtoAberto.Visibility = Visibility.Hidden;

            if (cbPgAbertGen.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Gênero");
                return;
            }

            var programasTemp =
                ((ListBox) cbRankProg.Template.FindName("lstBox", cbPgAbertProg)).SelectedItems;
            List<Programa> programas = programasTemp.Cast<Programa>().ToList();

            if (programas.Count < 1)
            {
                AlertaSelecaoFiltro("Programa");
                return;
            }

            if (cbPgAbertMes.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Mês");
                return;
            }
            if (cbPgAbertAno.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Ano");
                return;
            }

            if (cbPgAbertMes.SelectedIndex != -1 && cbPgAbertAno.SelectedIndex != -1)
            {
                var unidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
                var mes = (int) cbPgAbertMes.SelectedValue;
                var ano = (int) cbPgAbertAno.SelectedValue;
                var genID = (int) cbPgAbertGen.SelectedValue;

                ShowLoader();

                List<RowRelatorioPgtoAberto> retorno = new List<RowRelatorioPgtoAberto>();

                ThreadStart dataDownloadThread = delegate
                                                     {

                                                         RelatorioPgtoAberto pgtoAberto = new RelatorioPgtoAberto();
                                                         retorno = pgtoAberto.PesquisarSonorizacoes(unidadeID, genID,
                                                                                                    mes, ano,
                                                                                                    programas);
                                                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                (EventHandler)
                                                                                delegate
                                                                                    {
                                                                                        dgPgtoAberto.ItemsSource =
                                                                                            retorno;
                                                                                        dgPgtoAberto.Height =
                                                                                            ActualHeight - 360;
                                                                                        dgPgtoAberto.Width =
                                                                                            ActualWidth -
                                                                                            100;
                                                                                        HideLoader();
                                                                                        if (retorno != null)
                                                                                        dgPgtoAberto.Visibility =
                                                                                            Visibility.Visible;
                                                                                    }, null, null);
                                                     };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);
                _logRepository.WriteLog("Relatório Pagamento Aberto", LogType.Informacao, _usuario.Login);
            }
        }


        private void GerarProvisao(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgProvisao);
            UpdateLayout();
            dgProvisao.Visibility = Visibility.Hidden;
            
            if (cbProvGen.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Gênero");
                return;
            }

            var programasTemp =
                    cbProvProg.SelectedItems;
            List<Programa> programas = programasTemp.ToList();

            if (programas.Count < 1)
            {
                AlertaSelecaoFiltro("Programa");
                return;
            }
            
            if (cbProvTipo.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Tipo");
                return;
            }
            if (cbProvMes.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Mês");
                return;
            }
            if (cbProvAno.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Ano");
                return;
            }

            if (cbProvMes.SelectedIndex != -1 && cbProvAno.SelectedIndex != -1)
            {
                

                var uniID = (int) cbUnidade.SelectedValue;
                var genID = (int) cbProvGen.SelectedValue;
                var mes = int.Parse(cbProvMes.SelectedValue.ToString());
                var ano = int.Parse(cbProvAno.SelectedValue.ToString());
                var tipoRelatorio = int.Parse(cbProvTipo.SelectedValue.ToString());

                ShowLoader();

                if (tipoRelatorio == 1)
                {
                    columnnewProvisaoQntd1.Visibility = Visibility.Hidden;
                    columnnewProvisaoQntd2.Visibility = Visibility.Hidden;
                    columnnewProvisaoQntd3.Visibility = Visibility.Hidden;
                    columnnewProvisaoQntd4.Visibility = Visibility.Hidden;
                    columnnewProvisaoQntd5.Visibility = Visibility.Hidden;
                    columnnewProvisaoQntd6.Visibility = Visibility.Hidden;
                    columnnewProvisaoQntd7.Visibility = Visibility.Hidden;

                    columnnewProvisao14.Visibility = Visibility.Hidden;
                }
                else
                {
                    columnnewProvisaoQntd1.Visibility = Visibility.Visible;
                    columnnewProvisaoQntd2.Visibility = Visibility.Visible;
                    columnnewProvisaoQntd3.Visibility = Visibility.Visible;
                    columnnewProvisaoQntd4.Visibility = Visibility.Visible;
                    columnnewProvisaoQntd5.Visibility = Visibility.Visible;
                    columnnewProvisaoQntd6.Visibility = Visibility.Visible;
                    columnnewProvisaoQntd7.Visibility = Visibility.Visible;

                    columnnewProvisao14.Visibility = Visibility.Visible;
                }

                List<RowRelatorioProvisao> retorno = new List<RowRelatorioProvisao>();

                ThreadStart dataDownloadThread = delegate
                                                     {
                                                         RelatorioProvisao provisao = new RelatorioProvisao();
                                                         if (tipoRelatorio == 1)
                                                             retorno = provisao.PesquisarSonorizacoes(uniID, genID, mes,
                                                                                                      ano,
                                                                                                      programas);
                                                         else
                                                             retorno = provisao.PesquisarSonorizacoesAnalitico(uniID,
                                                                                                               genID,
                                                                                                               mes, ano,
                                                                                                  programas);

                                                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                (EventHandler)
                                                                                delegate
                                                                                    {
                                                                                        dgProvisao.ItemsSource =
                                                                                            retorno;
                                                                                        dgProvisao.Height =
                                                                                            ActualHeight - 360;
                                                                                        dgProvisao.Width =
                                                                                            ActualWidth - 100;
                                                                                        HideLoader();
                                                                                        if (retorno != null)
                                                                                        dgProvisao.Visibility =
                                                                                            Visibility.Visible;
                                                                                    }, null, null);

                                                     };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);
                _logRepository.WriteLog("Relatório Provisão", LogType.Informacao, _usuario.Login);
            }
        }

        private void GerarUtilizacao(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgUtil);
            UpdateLayout();
            dgUtil.Visibility = Visibility.Hidden;
            DateTime dataTeste;

            if (cbUtilGen.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Gênero");
                return;
            }

            var programasTemp =
                ((ListBox) cbUtilProg.Template.FindName("lstBox", cbUtilProg)).SelectedItems;
            List<Programa> programas = programasTemp.Cast<Programa>().ToList();

            if (programas.Count < 1)
            {
                AlertaSelecaoFiltro("Programa");
                return;
            }
            
            if (!dtUtilIni.SelectedDate.HasValue ||
                !DateTime.TryParse(dtUtilIni.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data inicial");
                return;
            }
            if (!dtUtilFin.SelectedDate.HasValue ||
                !DateTime.TryParse(dtUtilFin.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data final");
                return;
            }

            if (dtRankIni.SelectedDate <= dtRankFin.SelectedDate)
            {
                var unidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
                var dataInicial = dtUtilIni.SelectedDate.Value;
                var dataFinal = dtUtilFin.SelectedDate.Value;
                var genID = (int) cbUtilGen.SelectedValue;

                ShowLoader();

                List<RowRelatorioUtilizacao> retorno = new List<RowRelatorioUtilizacao>();

                ThreadStart dataDownloadThread = delegate
                {

                    RelatorioUtilizacao utilizacao = new RelatorioUtilizacao();
                                                         retorno = utilizacao.PesquisarSonorizacoes(unidadeID, genID,
                                                                                                    dataInicial,
                                                                                                    dataFinal,
                                                             programas);

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                           (EventHandler)
                                           delegate
                                           {
                                               dgUtil.ItemsSource =
                                                   retorno;
                                               dgUtil.Height =
                                                   ActualHeight - 360;
                                               dgUtil.Width =
                                                   ActualWidth - 100;
                                               HideLoader();
                                                                                        if (retorno != null)
                                               dgUtil.Visibility =
                                                   Visibility.Visible;
                                           }, null, null);

                };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);
                _logRepository.WriteLog("Relatório Utilização", LogType.Informacao, _usuario.Login);

            }
        }

        private void ShowLoader()
        {
            Loading.Visibility = Visibility.Visible;
            Loading.StartStopLoader(true);
        }

        private void HideLoader()
        {
            Loading.Visibility = Visibility.Collapsed;
            Loading.StartStopLoader(false);
        }


        private void GerarRanking(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgRanking);
            UpdateLayout();
            dgRanking.Visibility = Visibility.Hidden;
            DateTime dataTeste;

            if (cbRankGen.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Gênero");
                return;
            }

            var programasTemp =
                ((ListBox) cbRankProg.Template.FindName("lstBox", cbRankProg)).SelectedItems;
            List<Programa> programas = programasTemp.Cast<Programa>().ToList();

            if (programas.Count < 1)
            {
                AlertaSelecaoFiltro("Programa");
                return;
            }

            if (cbRankTipo.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Tipo");
                return;
            }
            if (cbRankTipTril.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Tipo de Trilha");
                return;
            }
            if (!dtRankIni.SelectedDate.HasValue || 
                !DateTime.TryParse(dtRankIni.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data inicial");
                return;
            }
            if (!dtRankFin.SelectedDate.HasValue ||
                !DateTime.TryParse(dtRankFin.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data final");
                return;
            }
            
            if (dtRankIni.SelectedDate <= dtRankFin.SelectedDate)
            {
                var unidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
                var dataInicial = dtRankIni.SelectedDate.Value;
                var dataFinal = dtRankFin.SelectedDate.Value;
                var tipoTrilha = int.Parse(cbRankTipTril.SelectedValue.ToString());
                var tipoRelatorio = int.Parse(cbRankTipo.SelectedValue.ToString());
                var genID = int.Parse(cbRankGen.SelectedValue.ToString());

                ShowLoader();

                if (tipoRelatorio == 1)
                    //esconde coluna de data
                    columnRankingECAD11.Visibility = Visibility.Hidden;
                else
                    //exibe coluna de data
                    columnRankingECAD11.Visibility = Visibility.Visible;

                List<RowRelatorioRanking> retorno = new List<RowRelatorioRanking>();

                ThreadStart dataDownloadThread = delegate
                                                     {
                                                         RelatorioRanking ranking = new RelatorioRanking();

                                                         if (tipoRelatorio == 1)
                                                         retorno = ranking.PesquisarSonorizacoes(
                                                                 unidadeID, dataInicial, dataFinal,
                                                                 tipoTrilha, genID, programas
                                                                 );
                                                         else
                                                             retorno = ranking.PesquisarSonorizacoesAnalitico(
                                                                 unidadeID, dataInicial, dataFinal,
                                                             tipoTrilha, genID, programas
                                                             );

                                                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                (EventHandler)
                                                                                delegate
                                                                                    {
                                                                                        dgRanking.ItemsSource = retorno;
                                                                                        dgRanking.Height =
                                                                                            ActualHeight - 360;
                                                                                        dgRanking.Width = ActualWidth -
                                                                                                          100;

                                                                                        HideLoader();
                                                                                        if (retorno != null)
                                                                                        dgRanking.Visibility =
                                                                                            Visibility.Visible;

                                                                                    }, null, null);
                                                     };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);
                _logRepository.WriteLog("Relatório Ranking", LogType.Informacao, _usuario.Login);
            }
        }

        private void GerarRankingEditora(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgRanking);
            UpdateLayout();
            dgRankingEdit.Visibility = Visibility.Hidden;
            DateTime dataTeste;

            if (cbRankEditGen.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Gênero");
                return;
            }

            var programasTemp =
                ((ListBox)cbRankEditProg.Template.FindName("lstBox", cbRankEditProg)).SelectedItems;
            List<Programa> programas = programasTemp.Cast<Programa>().ToList();

            if (programas.Count < 1)
            {
                AlertaSelecaoFiltro("Programa");
                return;
            }

            if (cbRankEditTipo.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Tipo");
                return;
            }

            if (!dtRankEditIni.SelectedDate.HasValue ||
                !DateTime.TryParse(dtRankEditIni.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data inicial");
                return;
            }
            if (!dtRankEditFin.SelectedDate.HasValue ||
                !DateTime.TryParse(dtRankEditFin.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data final");
                return;
            }

            if (dtRankEditIni.SelectedDate <= dtRankEditFin.SelectedDate)
            {
                var unidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
                var dataInicial = dtRankEditIni.SelectedDate.Value;
                var dataFinal = dtRankEditFin.SelectedDate.Value;
                var tipoTrilha = 2; // comercial
                var tipoRelatorio = int.Parse(cbRankEditTipo.SelectedValue.ToString());
                var genID = int.Parse(cbRankEditGen.SelectedValue.ToString());

                ShowLoader();

                if (tipoRelatorio == 1)
                    //esconde coluna de data
                    columnRankingEditECAD11.Visibility = Visibility.Hidden;
                else
                    //exibe coluna de data
                    columnRankingEditECAD11.Visibility = Visibility.Visible;

                List<RowRelatorioRankingEditora> retorno = new List<RowRelatorioRankingEditora>();

                ThreadStart dataDownloadThread = delegate
                {
                    RelatorioRankingEditora ranking = new RelatorioRankingEditora();

                    if (tipoRelatorio == 1)
                        retorno = ranking.PesquisarSonorizacoes(
                                unidadeID, dataInicial, dataFinal,
                                tipoTrilha, genID, programas
                                );
                    else
                        retorno = ranking.PesquisarSonorizacoesAnalitico(
                            unidadeID, dataInicial, dataFinal,
                        tipoTrilha, genID, programas
                        );

                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                           (EventHandler)
                                           delegate
                                           {
                                               dgRankingEdit.ItemsSource = retorno;
                                               dgRankingEdit.Height =
                                                   ActualHeight - 360;
                                               dgRankingEdit.Width = ActualWidth -
                                                                 100;

                                               HideLoader();
                                               if (retorno != null)
                                                   dgRankingEdit.Visibility =
                                                       Visibility.Visible;

                                           }, null, null);
                };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);
                _logRepository.WriteLog("Relatório Ranking Editora", LogType.Informacao, _usuario.Login);
            }
        }

        private void AlertaSelecaoFiltro(string nomeCampo)
        {
            MessageBox.Show("Alerta!\n Selecione '" + nomeCampo + "' para poder gerar o relatório!");
        }

        private void GerarCanhoto(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgCanhotos);
            UpdateLayout();
            dgCanhotos.Visibility = Visibility.Hidden;
            DateTime dataTeste;
            if (cbCanhotGen.SelectedIndex == -1)
            {
                AlertaSelecaoFiltro("Gênero");
                return;
            }

            var programasTemp =
                ((ListBox) cbRankProg.Template.FindName("lstBox", cbCanhotProg)).SelectedItems;
            List<Programa> programas = programasTemp.Cast<Programa>().ToList();

            if (programas.Count < 1)
            {
                AlertaSelecaoFiltro("Programa");
                return;
            }

            var editoraTemp =
                ((ListBox) cbCanhotEdit.Template.FindName("lstBox", cbCanhotEdit)).SelectedItems;
            List<Editora> editoras = editoraTemp.Cast<Editora>().ToList();

            if (!editoras.Any())
            {
                AlertaSelecaoFiltro("Editora");
                return;
            }

            if (!dtCanhotIni.SelectedDate.HasValue ||
                !DateTime.TryParse(dtCanhotIni.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data inicial");
                return;
            }
            if (!dtCanhotFin.SelectedDate.HasValue ||
                !DateTime.TryParse(dtCanhotFin.SelectedDate.Value.ToShortDateString(), out dataTeste))
            {
                AlertaSelecaoFiltro("data final");
                return;
            }

            //   if (cbRelCanhotoEditora.SelectedIndex != -1 && dateCanhotoInicial.SelectedDate.HasValue)
            if (dtCanhotIni.SelectedDate <= dtCanhotFin.SelectedDate)
            {
                var unidadeID = int.Parse(cbUnidade.SelectedValue.ToString());
                var genID = int.Parse(cbCanhotGen.SelectedValue.ToString());
                var dataInicial = dtCanhotIni.SelectedDate.Value;
                var dataFinal = dtCanhotFin.SelectedDate.Value;

                ShowLoader();

                List<RowRelatorioCanhoto> retorno = new List<RowRelatorioCanhoto>(); // new List<RowRelatorioCanhoto>();

                ThreadStart dataDownloadThread = delegate
                                                     {
                                                         RelatorioCanhoto canhoto = new RelatorioCanhoto();
                                                         retorno = canhoto.PesquisarCanhoto(
                                                             unidadeID, dataInicial, dataFinal, editoras, genID,
                                                             programas);

                                                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                (EventHandler)
                                                                                delegate
                                                                                    {
                                                                                        dgCanhotos.ItemsSource =
                                                                                            retorno;
                                                                                        dgCanhotos.Height =
                                                                                            ActualHeight - 360;
                                                                                        dgCanhotos.Width =
                                                                                            ActualWidth - 100;

                                                                                        HideLoader();
                                                                                        dgCanhotos.Visibility =
                                                                                            Visibility.Visible;

                                                                                    }, null, null);
                                                     };
                dataDownloadThread.BeginInvoke(
                    delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);
                _logRepository.WriteLog("Relatório Canhoto", LogType.Informacao, _usuario.Login);
            }
        }




        private void PesquisaMusRanking(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgRanking);
            UpdateLayout();

            var result = from mus in context.Musicas
                         select mus;

            if (result.ToList().Count > 0)
            {

                if (!string.IsNullOrWhiteSpace(txbPesquisaMusica.Text))
                    result = result.Where(m => m.Titulo.Contains(txbPesquisaMusica.Text));
            }

            dgRanking.ItemsSource = result.GroupBy(m => m.Titulo).ToList();
        }


        private void cbPagEditora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPagEditora.SelectedIndex != -1)
            {
                var aut = (RowExibicaoAutorizacao) dgAutMusicas.CurrentItem;
                using (Repositorio repositorio = new Repositorio())
                {
                    int anoVigencia =
                        repositorio.Obter<Sincronizacao>(s => s.SincronizacaoID == aut.SincID)
                                   .Select(s => s.Exibicao.Data.Year)
                                   .FirstOrDefault();

                    Editora edit = null;
                    //Obtem valores a partir da editora selecionada e a musica
                    if (cbPagEditora.SelectedIndex != -1)
                    {
                        //Editora
                        edit = (from ed in context.Editoras
                                where ed.EditoraID == (int) cbPagEditora.SelectedValue
                                select ed).SingleOrDefault();
                    }

                    if (edit != null)
                    {
                        var classID =
                            repositorio.Obter<Classificacao>(s => s.Descricao.Contains(aut.Classificacao))
                                       .Select(s => s.ClassificacaoID)
                                       .FirstOrDefault();
                        var genID = (from g in context.Sincronizacoes
                                     where g.SincronizacaoID == aut.SincID
                                     select g.Exibicao.Programa.GeneroID).SingleOrDefault();
                        var precos = (from p in context.Precos
                                      where p.ClassificacaoID == classID
                                            && p.GeneroID == genID
                                            && p.Vigencia == anoVigencia
                                      select p).Distinct().ToList();
                        cbPagValor.ItemsSource = precos;
                    }
                }
            }
        }



        private void HabilitaPoutPourri(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(((TextBox) sender).Text))
            {
                _total = _totalPorcentagem + decimal.Parse(((TextBox) sender).Text);

                if (_total > 100)
                {
                    panelPagPoutPourri.IsEnabled = true; //.Visibility = Visibility.Visible;

                    btnSalvarAutorizacao.IsEnabled = false;
                }
                else
                {
                    chkPagPoutPourri.IsChecked = false;
                    chkPagIncidental.IsChecked = false;
                    panelPagPoutPourri.IsEnabled = false; // Visibility.Hidden;

                    btnSalvarAutorizacao.IsEnabled = true;
                }
            }
        }

        private void CarregaInfoPagamentos(int AutID, int MusicaID, int SincID, int SonID)
        {
            _sincID = SincID;
            _sonID = SonID;
            _musID = MusicaID;

            _genID = (from g in context.Sincronizacoes
                      where g.SincronizacaoID == _sincID
                      select g.Exibicao.Programa.GeneroID).SingleOrDefault();

            var classificacaoSonSelecionada = (from s in context.Sonorizacoes
                                               where s.SonorizacaoID == _sonID
                                               select s.ClassificacaoID).FirstOrDefault();

            var sonorizacoes = from son in context.Sonorizacoes
                               where son.SincronizacaoID == _sincID
                                     && son.ClassificacaoID == classificacaoSonSelecionada
                                     && son.MusicaID == _musID
                               select son;

            var autorizacoes = from aut in context.Autorizacoes
                               where aut.MusicaID == _musID
                               select aut;

            var pagamentosMusSon = (from aut in autorizacoes
                                    join son in sonorizacoes
                                        on aut.SonorizacaoID equals son.SonorizacaoID
                                    join edit in context.Editoras
                                        on aut.EditoraID equals edit.EditoraID
                                    join user in context.Usuarios
                                        on aut.UsuarioID equals user.UsuarioID
                                    select new RowExibicaoPagamento
                                               {
                                                   AutID = aut.AutorizacaoID,
                                                   Vencimento = aut.Vencimento,
                                                   Usuario = user.Login,
                                                   Valor = aut.Valor,
                                                   Porcentagem = aut.Porcentagem,
                                                   Editora = edit.Nome,
                                                   Arquivo = aut.Arquivo
                                               })
                .GroupBy(p => p.AutID)
                .ToList();
            _totalPorcentagem = 0;

            foreach (var pagamentos in pagamentosMusSon)
            {
                _totalPorcentagem += pagamentos.FirstOrDefault().Porcentagem;
            }



            var autSelecionada = (RowExibicaoAutorizacao) dgAutMusicas.CurrentItem;
            if (autSelecionada != null && autSelecionada.SonID > 0)
            {
                _sonID = autSelecionada.SonID;
                _musID = autSelecionada.MusicaID;
            }
            else
            {
                _sonID = 0;
                _musID = 0;
            }

            if (_totalPorcentagem > 100)
            {
                lblPagAdicional.Visibility = Visibility.Visible;
                panelPagPoutPourri.IsEnabled = true;
            }
        }

        private void ProcurarArquivoAutorizacao(object sender, RoutedEventArgs e)
        {
            string arquivo = "";
            var dialog = new OpenFileDialog {DefaultExt = "*.*", RestoreDirectory = true};

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
                arquivo = dialog.FileName;

            if (!string.IsNullOrEmpty(arquivo))
            {
                txtPagArquivo.Text = arquivo;
            }
        }

        private void ChkPagIncidental_OnChecked(object sender, RoutedEventArgs e)
        {
            chkPagPoutPourri.IsChecked = false;
            chkPagPoutPourri.Focus();
            btnSalvarAutorizacao.IsEnabled = true;
        }

        private void ChkPagPoutPourri_OnChecked(object sender, RoutedEventArgs e)
        {
            chkPagIncidental.IsChecked = false;
            btnSalvarAutorizacao.IsEnabled = true;
        }

        private void chkAtivoQuadro_OnChecked(object sender, RoutedEventArgs e)
        {
            chkQuadroAtivo.IsChecked = true;
        }

        private void chkAtivoQuadro_OnUnchecked(object sender, RoutedEventArgs e)
        {
            chkQuadroAtivo.IsChecked = false;
        }

        private void chkAtivoGen_OnChecked(object sender, RoutedEventArgs e)
        {
            chkGenAtivo.IsChecked = true;
        }

        private void chkAtivoGen_OnUnchecked(object sender, RoutedEventArgs e)
        {
            chkGenAtivo.IsChecked = false;
        }



        private void chkAtivoExb_OnChecked(object sender, RoutedEventArgs e)
        {
            chkExbAtivo.IsChecked = true;
        }

        private void chkAtivoExb_OnUnchecked(object sender, RoutedEventArgs e)
        {
            chkExbAtivo.IsChecked = false;
        }

        private void chkAtivoProg_OnChecked(object sender, RoutedEventArgs e)
        {
            chkAtivoClass.IsChecked = true;
        }

        private void chkAtivoProg_OnUnchecked(object sender, RoutedEventArgs e)
        {
            chkAtivoClass.IsChecked = false;
        }

        private void chkAtivoP_OnChecked(object sender, RoutedEventArgs e)
        {
            chkProgAtivo.IsChecked = true;
        }

        private void chkAtivoP_OnUnchecked(object sender, RoutedEventArgs e)
        {
            chkProgAtivo.IsChecked = false;
        }


        private void ChkPagIncidental_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (!chkPagIncidental.IsChecked.Value && !chkPagPoutPourri.IsChecked.Value)
            {
                btnSalvarAutorizacao.IsEnabled = false;
            }
        }

        private void ChkPagPoutPourri_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (!chkPagIncidental.IsChecked.Value && !chkPagPoutPourri.IsChecked.Value)
            {
                btnSalvarAutorizacao.IsEnabled = false;
            }
        }

        private void SalvarAutorizacao(object sender, RoutedEventArgs e)
        {
            if (_total > 100)
            {
                if (!chkPagIncidental.IsChecked.Value && !chkPagPoutPourri.IsChecked.Value)
                {
                    return;
                }
            }
            if (!dtPagVencimento.SelectedDate.HasValue)
            {
                return;
            }
            else if (dtPagVencimento.SelectedDate.Value < DateTime.Now.Date)
            {
                MessageBox.Show("Data de vencimento não deve ser retroativa.");
                return;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(txtPagArquivo.Text))
                {
                    if (!String.IsNullOrWhiteSpace(txtPagPorcentagem.Text) && cbPagValor.SelectedValue != null)
                    {
                        var aut = new Autorizacao();
                        aut.Porcentagem = Decimal.Parse(txtPagPorcentagem.Text);
                        aut.Valor = (Decimal.Parse(cbPagValor.Text)*(aut.Porcentagem/100));
                        aut.Vencimento = DateTime.Parse(dtPagVencimento.Text);
                        aut.SonorizacaoID = _sonID;
                        aut.MusicaID = _musID;
                        aut.UsuarioID =
                            new VerificarUsuario().ObterUsuario(ConfigurationManager.AppSettings["Login"]).UsuarioID;
                        aut.EditoraID = (int) cbPagEditora.SelectedValue;
                        aut.Arquivo = txtPagArquivo.Text;

                        TipoExibicao exibicao = null;
                        using (var repositorio = new Repositorio())
                        {
                            exibicao =
                                repositorio.Obter<Sonorizacao>(s => s.SonorizacaoID == aut.SonorizacaoID)
                                           .FirstOrDefault()
                                           .TipoExibicao;
                        }
                        if (exibicao.Descricao.ToUpper().Contains("REPRISE"))
                        {
                            aut.Valor /= 2;
                        }
                        if (_totalPorcentagem > 100)
                        {

                            //TODO:Implementar no pagamento: Incidental, PoutPourri e Observacoes
                        }

                        context.Autorizacoes.Add(aut);
                        context.SaveChanges();
                    }
                    CarregarComboPagamentosEditoras();

                    var autSelect = (RowExibicaoAutorizacao) dgAutMusicas.CurrentItem;
                    CarregaInfoPagamentos(autSelect.AutID, autSelect.MusicaID, autSelect.SincID, autSelect.SonID);

                    cbPagEditora.SelectedValue = 0;
                    cbPagEditora.SelectedIndex = -1;
                    cbPagEditora.UpdateLayout();

                    cbPagValor.SelectedValue = 0;
                    cbPagValor.SelectedIndex = -1;
                    cbPagValor.UpdateLayout();

                    dtPagVencimento.SelectedDate = DateTime.Now;
                    dtPagVencimento.Text = string.Empty;

                    txtPagPorcentagem.Clear();
                    txtPagArquivo.Clear();

                    CarregarGridMusicasAutorizacao();
                }
            }
        }

        private void SalvarExibicao(object sender, RoutedEventArgs e)
        {
            if (txbExbDesc.Text != "")
            {
                if (!context.TipoExibicoes.Any(m => m.Descricao == txbExbDesc.Text))
                {

                    var exb = new TipoExibicao();
                    exb.Descricao = txbExbDesc.Text.Normalizar();
                    exb.Ativo = chkExbAtivo.IsChecked == true;


                    context.TipoExibicoes.Add(exb);
                    context.SaveChanges();
                    _logRepository.WriteLog("Salvar Tipo de Exibição", LogType.Informacao, _usuario.Login);

                    MessageBox.Show("Dados Salvos com sucesso!");
                    txbExbDesc.Clear();
                }
                else
                {
                    MessageBox.Show("Alerta!\n Tipo de Exibição já está cadastrado");

                }

            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");

            }
        }

        private void SalvarQuadro(object sender, RoutedEventArgs e)
        {
            if (txbQuadrosNome.Text != "" && cbQuadroProg.SelectedIndex != -1)
            {
                if (!context.Quadros.Any(m => m.Descricao == txbQuadrosNome.Text))
                {
            var qda = new Quadro();
            qda.Descricao = txbQuadrosNome.Text.Normalizar();
                qda.ProgramaID = int.Parse(cbQuadroProg.SelectedValue.ToString());
                    qda.Ativo = chkQuadroAtivo.IsChecked == true;

            context.Quadros.Add(qda);
            context.SaveChanges();
            _logRepository.WriteLog("Salvar Quadros", LogType.Informacao, _usuario.Login);

            MessageBox.Show("Dados salvos com sucesso!");
            txbQuadrosNome.Clear();
                    cbQuadroProg.SelectedValue = -1;
            }
            else
            {
                    MessageBox.Show("Alerta!\n Quadro já está cadastrado");
                }
            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
        }
        }

        private void SalvarClass(object sender, RoutedEventArgs e)
        {
            if (txbClassNome.Text != "")
            {
                if (!context.Classificacoes.Any(m => m.Descricao == txbClassNome.Text))
                {
                    var cls = new Classificacao();
                    cls.Descricao = txbClassNome.Text.Normalizar();
                    cls.Ativo = Convert.ToBoolean(chkAtivoClass.IsChecked);

                    context.Classificacoes.Add(cls);
                    context.SaveChanges();
                    _logRepository.WriteLog("Salvar Classificação", LogType.Informacao, _usuario.Login);

                    MessageBox.Show("Dados Salvos com sucesso!");
                    txbClassNome.Text = "";
                }
                else
                {
                    MessageBox.Show("Alerta!\n Classificação já está cadastrada");

                }
            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");

            }
        }

        private void SalvarProgramas(object sender, RoutedEventArgs e)
        {
            if (txbProgDesc.Text != "" && txbProgOrdem.Text != "" && cbxProgGen.SelectedValue != null)
            {
                if (!context.Programas.Any(m => m.Nome == txbProgDesc.Text))
                {
                    var prog = new Programa();
                    prog.GeneroID = int.Parse(cbxProgGen.SelectedValue.ToString());
                    prog.Nome = txbProgDesc.Text.Normalizar();
                    prog.Ordem = txbProgOrdem.Text.Normalizar();
                    prog.Ativo = chkProgAtivo.IsChecked == true;

                    context.Programas.Add(prog);
                    context.SaveChanges();
                    _logRepository.WriteLog("Salvar Programas", LogType.Informacao, _usuario.Login);

                    MessageBox.Show("Dados Salvos com sucesso!");
                    txbProgDesc.Clear();
                    txbProgOrdem.Clear();
                    cbxProgGen.SelectedValue = -1;
                   

                }
                else
                {
                    MessageBox.Show("Alerta!\n Não é possível Salvar. Programa já existe!");

                }


            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");

            }
        }
    

        private void SalvarAss(object sender, RoutedEventArgs e)
        {
            if (txbAssNome.Text != "")
            {
                if (!context.Associacoes.Any(m => m.Nome == txbAssNome.Text))
                {
                    var ass = new Associacao();
                    ass.Nome = txbAssNome.Text.Normalizar();
                    ass.Ativo = chkAssAtivo.IsChecked == true;

                    context.Associacoes.Add(ass);
                    context.SaveChanges();
                    _logRepository.WriteLog("Salvar Associacao", LogType.Informacao, _usuario.Login);

                    MessageBox.Show("Dados Salvos com sucesso!");
                    txbAssNome.Clear();

                }
                else
                {
                    MessageBox.Show("Alerta!\n Associações já está cadastrada");

                }
            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
            }
        }

        private void SalvarEditora(object sender, RoutedEventArgs e)
        {
            if (cbxEditoraAss.SelectedValue != null && txbEditoraNome.Text != "")
            {
                if (!context.Editoras.Any(m => m.Nome == txbEditoraNome.Text))
                {


                    var edt = new Editora
                        {
                            AssociacaoID = int.Parse(cbxEditoraAss.SelectedValue.ToString()),
                            Nome = txbEditoraNome.Text.Normalizar(),
                            RazaoSocial = txbEditoraRazao.Text.Normalizar(),
                            CNPJJ = txbEditoraCNPJ.Text.ValidacaoCnpj(),
                            CNPJ = txbEditoraCNPJ.Text,
                            Endereco = txbEditoraEnd.Text.Normalizar(),
                            Numero = txbEditoraN.Text,
                            Bairro = txbEditoraBairro.Text.Normalizar(),
                            Cep = txbEditoraCep.Text,
                            Complemento = txbEditoraComp.Text.Normalizar(),
                            Email = txbEditoraMail.Text.Normalizar(),
                            Email2 = txbEditoraMail1.Text.Normalizar(),
                            DDD = txbEditoraDDD.Text,
                            DDD1 = txbEditoraDDD1.Text,
                            Fone = txbEditoraContato.Text,
                            Fone1 = txbEditoraContato1.Text,
                            Contato = txbEditoraNomeC.Text.Normalizar(),
                            Contato2 = txbEditoraNomeC1.Text.Normalizar(),
                            Ativo = chkGravadoraAtivo.IsChecked == true
                        };

                    if (txbEditoraCNPJ.Text.ValidacaoCnpj())
                    {

                        context.Editoras.Add(edt);
                        context.SaveChanges();

                        MessageBox.Show("Dados salvos com sucesso!");
                        cbxEditoraAss.SelectedValue = -1;
                        txbEditoraNome.Clear();
                        txbEditoraRazao.Clear();
                        txbEditoraCNPJ.Clear();
                        txbEditoraEnd.Clear();
                        txbEditoraN.Clear();
                        txbEditoraBairro.Clear();
                        txbEditoraCep.Clear();
                        txbEditoraComp.Clear();
                        txbEditoraMail.Clear();
                        txbEditoraMail1.Clear();
                        txbEditoraDDD.Clear();
                        txbEditoraDDD1.Clear();
                        txbEditoraContato.Clear();
                        txbEditoraContato1.Clear();
                    }
                    else
                    {
                        MessageBox.Show("CNPJ Inválido!");
                    }

                }
                else
                {
                    MessageBox.Show("Alerta!\n Editora já está cadastrada");

                }

            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
            }
        }



        private void SalvarGen(object sender, RoutedEventArgs e)
        {
            if (txbGenNome.Text != "" && lstGen.Items.Count != 0)
            {
                bool editado = false;
                var gen = context.Generos.Where(g => g.Descricao == txbGenNome.Text).FirstOrDefault();
                if (gen == null)
                {
                    gen = new Genero();
                    gen.Classificacoes = new List<Classificacao>();
                }
                else
                {
                    editado = true;
                }
                //gen.GeneroID = int.Parse(cbxGenClass.SelectedValue.ToString());
                gen.Descricao = txbGenNome.Text.Normalizar();
                gen.Ativo = chkGenAtivo.IsChecked == true;
                foreach (var item in lstGen.Items)
                {
                    var idCls = ((Classificacao) item).ClassificacaoID;
                    var cls = context.Classificacoes.Where(c => c.ClassificacaoID == idCls).SingleOrDefault();
                    gen.Classificacoes.Add(cls);
                }
                if (!editado)
                    context.Generos.Add(gen);
                else
                    context.Entry<Genero>(gen).State = EntityState.Modified;
                context.SaveChanges();
                _logRepository.WriteLog("Salvar Genero", LogType.Informacao, _usuario.Login);

                MessageBox.Show("Dados salvos com sucesso!");
                txbGenNome.Clear();
                cbxGenClass.SelectedValue = -1;
                lstGen.Items.Clear();
            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
            }
        }

        private void SalvarPreco(object sender, RoutedEventArgs e)
        {
            if (txbPrecoValor.Text != "" && cbxPrecoClass.SelectedIndex != -1)
            {
            var price = new Preco();
            price.AssociacaoID = int.Parse(cbxPrecoAss.SelectedValue.ToString());
            price.ClassificacaoID = int.Parse(cbxPrecoClass.SelectedValue.ToString());
            price.Vigencia = Convert.ToInt32(cbxPrecoAno.SelectedValue);
            price.Abrangencia = cbxPrecoAbr.Text;
            price.Valor = Convert.ToDecimal(txbPrecoValor.Text);
            price.Ativo = chkPrecoAtivo.IsChecked == true;

            foreach (var item in lstPreco.Items)
            {
                    price.GeneroID = ((Genero) item).GeneroID;
                context.Precos.Add(price);
            }

            context.Precos.Add(price);
            context.SaveChanges();
            _logRepository.WriteLog("Salvar Preco", LogType.Informacao, _usuario.Login);

            MessageBox.Show("Dados salvos com sucesso!");

            cbxPrecoClass.SelectedValue = -1;
            cbxPrecoAno.SelectedValue = -1;
            cbxPrecoAbr.SelectedValue = -1;
            txbPrecoValor.Clear();
            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
            }
        }

        private void SalvarNovela(object sender, RoutedEventArgs e)
        {
            if (txbNovelaTituloO.Text != "" && txbNovelaTituloN.Text != "" && dateNovelaInicial.SelectedDate != null &&
                dateNovelaFinal.SelectedDate != null && cbNovelaPais != null)
            {
                var novela = new Novela();

                novela.ProgramaID = int.Parse(cbNovelaProg.SelectedValue.ToString());
                novela.TituloNacional = txbNovelaTituloN.Text.Normalizar();
                novela.TituloOriginal = txbNovelaTituloO.Text.Normalizar();
                novela.Produtor = txbNovelaProdutor.Text.Normalizar();
                novela.Autor = txbNovelaAutor.Text.Normalizar();
                novela.Diretor = txbNovelaDiretor.Text.Normalizar();
                novela.DataInicial = Convert.ToDateTime(dateNovelaInicial.Text);
                novela.DataFinal = Convert.ToDateTime(dateNovelaFinal.Text);
                novela.Ativo = Convert.ToBoolean(chkNovelaAtivo.IsChecked);
                novela.Pais = cbNovelaPais.SelectionBoxItem.ToString();
            
            context.Novela.Add(novela);
            context.SaveChanges();
            _logRepository.WriteLog("Salvar Novela", LogType.Informacao, _usuario.Login);

            MessageBox.Show("Dados salvos com sucesso!");
            cbNovelaGen.SelectedValue = 0;
            cbNovelaProg.SelectedValue = 0;
            txbNovelaTituloN.Clear();
            txbNovelaTituloO.Clear();
            txbNovelaProdutor.Clear();
            txbNovelaAutor.Clear();
            txbNovelaDiretor.Clear();           

            }
            else
            {
                MessageBox.Show("Alerta!\n Não é possível Salvar com os campos em branco!");
            }
        }


        private void AddGen(object sender, RoutedEventArgs e)
        {
            if (cbxGenClass.SelectedValue != null)
            {
                if (!lstGen.Items.Contains(cbxGenClass.SelectedItem))
                {
                    lstGen.Items.Add(cbxGenClass.SelectedItem);
                }
                else
                {
                    MessageBox.Show("Alerta!\n Item já pertencente a lista!");
                }
            }
            else
            {

                MessageBox.Show("Alerta!\n Selecione um item na lista!");
            }
        }

        private void UaddGen(object sender, RoutedEventArgs e)
        {
            if (cbxGenClass.SelectedValue != null)
            {
                lstGen.Items.Remove(cbxGenClass.SelectedItem);
            }

        }


        private void AddGen2(object sender, RoutedEventArgs e)
        {
            if (cbxPrecoGen.SelectedValue != null)
            {
                if (!lstPreco.Items.Contains(cbxPrecoGen.SelectedItem))
                {
                    lstPreco.Items.Add(cbxPrecoGen.SelectedItem);
                }
                else
                {
                    MessageBox.Show("Alerta!\n Item já pertencente a lista!");
                }
            }
            else
            {
                MessageBox.Show("Alerta!\n Selecione um item na lista!");

            }
        }

        private void UaddGen2(object sender, RoutedEventArgs e)
        {
            if (cbxPrecoGen.SelectedValue != null)
            {
                lstPreco.Items.Remove(cbxPrecoGen.SelectedItem);
            }

        }


        private void PesquisarTipoExibicao(object sender, RoutedEventArgs e)
        {
            if (chkExbAtivo.IsChecked == true)
            {
                var ctx = new TipoExibicao();
                var result = from pesq in context.TipoExibicoes
                             where pesq.Ativo == true
                             select pesq;
                if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbExbDesc.Text))
                        result = result.Where(m => m.Descricao.Contains(txbExbDesc.Text));
                }
                GdgExbDesc.Visibility = Visibility.Visible;
                GdgExbDesc.ItemsSource = result.ToList();
                btExbSalvar.IsEnabled = false;

            }
            else
            {
                var ctx = new TipoExibicao();
                var result = from pesq in context.TipoExibicoes
                             where pesq.Ativo == false
                             select pesq;
                if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbExbDesc.Text))
                        result = result.Where(m => m.Descricao.Contains(txbExbDesc.Text));
                }
                GdgExbDesc.Visibility = Visibility.Visible;
                GdgExbDesc.ItemsSource = result.ToList();
                btExbSalvar.IsEnabled = false;
            }

        }

        private void PesquisarClass(object sender, RoutedEventArgs e)
        {
            if (chkAtivoClass.IsChecked == true)
            {
                Classificacao ctx = new Classificacao();
                var result = from cls in context.Classificacoes
                             where cls.Ativo == true
                             select cls;
                if (result.ToList().Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(txbClassNome.Text))
                        result = result.Where(m => m.Descricao.Contains(txbClassNome.Text));
                }
                gdgClassDesc.Visibility = Visibility.Visible;
                gdgClassDesc.ItemsSource = result.ToList();
            }

            else
            {
                Classificacao ctx = new Classificacao();
                var result = from cls in context.Classificacoes
                             where cls.Ativo == false
                             select cls;
                if (result.ToList().Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(txbClassNome.Text))
                        result = result.Where(m => m.Descricao.Contains(txbClassNome.Text));
                }
                gdgClassDesc.Visibility = Visibility.Visible;
                gdgClassDesc.ItemsSource = result.ToList();
            }
        }

        private void PesquisarAss(object sender, RoutedEventArgs e)
        {
           if (chkAssAtivo.IsChecked == true)
            {
                var ctx = new Associacao();
                var result = from cls in context.Associacoes
                             where cls.Ativo == true
                             select cls;
                if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbAssNome.Text))
                        result = result.Where(m => m.Nome.Contains(txbAssNome.Text));
                }

                gdgAssDesc.Visibility = Visibility.Visible;
                gdgAssDesc.ItemsSource = result.ToList();
            }
            else
            {
                var ctx = new Associacao();
                var result = from cls in context.Associacoes
                             where cls.Ativo == false
                             select cls;
                if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbAssNome.Text))
                        result = result.Where(m => m.Nome.Contains(txbAssNome.Text));
                }

                gdgAssDesc.Visibility = Visibility.Visible;
                gdgAssDesc.ItemsSource = result.ToList();

            }
        }

        private void PesquisaEditora(object sender, RoutedEventArgs e)
        {
            if (chkEdtAtivo.IsChecked == true)
            {
                var ctx = new Editora();
                var result = from edt in context.Editoras
                             where edt.Ativo == true
                             select edt;
                if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbEditoraNome.Text))
                        result = result.Where(m => m.Nome.Contains(txbEditoraNome.Text));

                    if (!string.IsNullOrWhiteSpace(txbEditoraRazao.Text))
                        result = result.Where(m => m.RazaoSocial.Contains(txbEditoraRazao.Text));
                    
                    if (!string.IsNullOrWhiteSpace(txbEditoraCNPJ.Text))
                        result = result.Where(m => m.CNPJ.Contains(txbEditoraCNPJ.Text));
                    
                    if (!string.IsNullOrWhiteSpace(txbEditoraEnd.Text))
                        result = result.Where(m => m.Endereco.Contains(txbEditoraEnd.Text));
                    
                    if (!string.IsNullOrWhiteSpace(txbEditoraBairro.Text))
                        result = result.Where(m => m.Bairro.Contains(txbEditoraBairro.Text));

                    if (cbxEditoraAss.SelectedValue != null)
                    {
                        result = result.Where(o => o.AssociacaoID == (int) cbxEditoraAss.SelectedValue);
                    }

                }
                gdgEditoraDesc.Visibility = Visibility.Visible;
                gdgEditoraDesc.ItemsSource = result.ToList();
            }
            else
            {
                var ctx = new Editora();
                var result = from edt in context.Editoras
                             where edt.Ativo == false
                             select edt;
                if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbEditoraNome.Text))
                        result = result.Where(m => m.Nome.Contains(txbEditoraNome.Text));

                    if (!string.IsNullOrWhiteSpace(txbEditoraRazao.Text))
                        result = result.Where(m => m.RazaoSocial.Contains(txbEditoraRazao.Text));

                    if (!string.IsNullOrWhiteSpace(txbEditoraCNPJ.Text))
                        result = result.Where(m => m.CNPJ.Contains(txbEditoraCNPJ.Text));

                    if (!string.IsNullOrWhiteSpace(txbEditoraEnd.Text))
                        result = result.Where(m => m.Endereco.Contains(txbEditoraEnd.Text));

                    if (!string.IsNullOrWhiteSpace(txbEditoraBairro.Text))
                        result = result.Where(m => m.Bairro.Contains(txbEditoraBairro.Text));

                    if (cbxEditoraAss.SelectedValue != null)
                    {
                        result = result.Where(o => o.AssociacaoID == (int) cbxEditoraAss.SelectedValue);
                    }
                }
                gdgEditoraDesc.Visibility = Visibility.Visible;
                gdgEditoraDesc.ItemsSource = result.ToList();

            }
        }

        private void PesquisaGravadora(object sender, RoutedEventArgs e)
        {
            if (chkGravadoraAtivo.IsChecked == true)
            {
                Gravadora ctx = new Gravadora();
                var result = from gvd in context.Gravadoras
                             where gvd.Ativo == true
                             select gvd;
                if (result.ToList().Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(txbGravadoraaNome.Text))
                        result = result.Where(m => m.Nome.Contains(txbGravadoraaNome.Text));
                    if (!string.IsNullOrWhiteSpace(txbGravadoraRazao.Text))
                        result = result.Where(m => m.Nome.Contains(txbGravadoraRazao.Text));
                    if (!string.IsNullOrWhiteSpace(txbGravadoraCNPJ.Text))
                        result = result.Where(m => m.Nome.Contains(txbGravadoraCNPJ.Text));
                   
                }
                gdgGravadoraDesc.Visibility = Visibility.Visible;
                gdgGravadoraDesc.ItemsSource = result.ToList();
            }
            else
            {
                Gravadora ctx = new Gravadora();
                var result = from gvd in context.Gravadoras
                             where gvd.Ativo == false
                             select gvd;
                if (result.ToList().Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(txbGravadoraaNome.Text))
                        result = result.Where(m => m.Nome.Contains(txbGravadoraaNome.Text));
                    
                }
                gdgGravadoraDesc.Visibility = Visibility.Visible;
                gdgGravadoraDesc.ItemsSource = result.ToList();

            }
        }


        private void PesquisarGen(object sender, RoutedEventArgs e)
        {
            var result = context.Generos
                .Include("Classificacoes")
                .Where(g => g.Ativo == chkGenAtivo.IsChecked);

            if (!String.IsNullOrWhiteSpace(txbGenNome.Text))
                {
                    result = result
                        .Where(g => g.Descricao == txbGenNome.Text);
                }
            var ListGenClass = new List<ViewClassificacoes>();
            foreach (var gen in result)
            {
                foreach (var cls in gen.Classificacoes)
                {
                    var vcg = new ViewClassificacoes {Genero = gen, Classificacao = cls};
                    ListGenClass.Add(vcg);
                }
                            }           

            gdgGenDesc.Visibility = Visibility.Visible;
            gdgGenDesc.ItemsSource = ListGenClass.ToList();

        }

        private void PesquisarPreco(object sender, RoutedEventArgs e)
        {
            var result = context.Precos
                         .Include("Associacao")
                         .Include("Genero")
                         .Include("Classificacao").Where(q => q.Ativo == chkPrecoAtivo.IsChecked);
                                          
                if (result.Any())             
                 {                  
                    if (cbxPrecoAno.SelectedValue != null)
                    {
                        var valor = int.Parse(cbxPrecoAno.SelectedValue.ToString());
                    result = result.Where(o => o.Vigencia == valor);
                    }

                    if (cbxPrecoGen.SelectedValue != null)
                    {
                    result = result.Where(o => o.GeneroID == (int) cbxPrecoGen.SelectedValue);
                    }
                                        
                    if (cbxPrecoClass.SelectedValue != null)
                    {
                        result = result.Where(o => o.ClassificacaoID == (int)cbxPrecoClass.SelectedValue);
                }

                    if (cbxPrecoAbr.Text != "")
                    {
                        result = result.Where(o => o.Abrangencia == cbxPrecoAbr.Text);
                    }
                }

                gdgPrecoDesc.Visibility = Visibility.Visible;
                gdgPrecoDesc.ItemsSource = result.ToList();
        }

         private void PesquisarNovela(object sender, RoutedEventArgs e)
        {
                    var result = from nov in context.Novela
                             join prog in context.Programas on nov.ProgramaID equals prog.ProgramaID
                             into joinEmptAut
                             from pgo in joinEmptAut.DefaultIfEmpty()
                              where nov.Ativo == chkNovelaAtivo.IsChecked
                         select new ViewNovela() {Programa = pgo, Novela = nov};
             if (result.Any())
                {
                    if (!string.IsNullOrWhiteSpace(txbNovelaTituloO.Text))
                        result = result.Where(m => m.Novela.TituloOriginal.Contains(txbNovelaTituloO.Text));
              
                    if (!string.IsNullOrWhiteSpace(txbNovelaTituloN.Text))
                        result = result.Where(m => m.Novela.TituloNacional.Contains(txbNovelaTituloN.Text));
               
                    if (!string.IsNullOrWhiteSpace(cbNovelaGen.Text))
                        result = result.Where(m => m.Novela.Programa.Genero.Descricao.Contains(cbNovelaGen.Text));
                    
                //if (!string.IsNullOrWhiteSpace(txbNovelaProdutor.Text))
                //    result = result.Where(m => m.Novela.Produtor.Contains(txbNovelaProdutor.Text));

                //if (!string.IsNullOrWhiteSpace(txbNovelaAutor.Text))
                //    result = result.Where(m => m.Novela.Autor.Contains(txbNovelaAutor.Text));

                //if (!string.IsNullOrWhiteSpace(txbNovelaDiretor.Text))
                //    result = result.Where(m => m.Novela.Diretor.Contains(txbNovelaDiretor.Text));

                    if (cbNovelaProg.SelectedValue != null)
                    {
                    result = result.Where(o => o.Novela.ProgramaID == (int) cbNovelaProg.SelectedValue);
                    }

                if (cbNovelaPais.Text != "")
                {
                    result = result.Where(o => o.Novela.Pais == cbNovelaPais.Text);
                }

            }

                GdgNovelasDesc.Visibility = Visibility.Visible;
                GdgNovelasDesc.ItemsSource = result.ToList();
                btNovelaSalvar.IsEnabled = false;
        }

        private void PesquisarQuadro(object sender, RoutedEventArgs e)
        {
            var result = from qdo in context.Quadros
                         join prog in context.Programas on qdo.ProgramaID equals prog.ProgramaID
                             into joinEmptAut
                         from pgo in joinEmptAut.DefaultIfEmpty()
                         where pgo.Ativo == chkQuadroAtivo.IsChecked
                         select new ViewQuadro() {Programa = pgo, Quadro = qdo};
            if (result.Any())
            {
                if (!string.IsNullOrWhiteSpace(txbQuadrosNome.Text))
                    result = result.Where(m => m.Quadro.Descricao.Contains(txbQuadrosNome.Text));
                if (cbQuadroProg.SelectedValue != null)
                {
                    if ((int) cbQuadroProg.SelectedValue != 0) //--TODOS--
                    {
                        result = result.Where(o => o.Programa.ProgramaID == (int) cbQuadroProg.SelectedValue);
                    }
                }
            }

            gdgQuadroDesc.Visibility = Visibility.Visible;
            gdgQuadroDesc.ItemsSource = result.Where(q => q.Quadro.Ativo == chkQuadroAtivo.IsChecked).ToList();
            btQuadroSalvar.IsEnabled = false;

        }

        private void PesquisarPrograma(object sender, RoutedEventArgs e)
        {
            if (chkProgAtivo.IsChecked == true)
            {
                var ctx = new Programa();
                var result = from pgd in context.Programas
                             where pgd.Ativo == true
                             select pgd;
                if (result.ToList().Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(txbProgDesc.Text))
                        result = result.Where(m => m.Nome.Contains(txbProgDesc.Text));

                    if (!string.IsNullOrWhiteSpace(txbProgOrdem.Text))
                        result = result.Where(o => o.Ordem.Contains(txbProgOrdem.Text));

                    if (cbxProgGen.SelectedValue != null)
                    {
                        result = result.Where(o => o.GeneroID == (int) cbxProgGen.SelectedValue);
                    }

                }

                GdgProgDesc.Visibility = Visibility.Visible;
                GdgProgDesc.ItemsSource = result.ToList();
                btProgSalvar.IsEnabled = false;

                _logRepository.WriteLog("Pesquisar Programas", LogType.Informacao, _usuario.Login);
            }

            else
            {
                var ctx = new Programa();
                var result = from pgd in context.Programas
                             where pgd.Ativo == false
                             select pgd;
                if (result.ToList().Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(txbProgDesc.Text))
                        result = result.Where(m => m.Nome.Contains(txbProgDesc.Text));
                    if (!string.IsNullOrWhiteSpace(txbProgOrdem.Text))
                        result = result.Where(o => o.Ordem.Contains(txbProgOrdem.Text));
                    if (cbxProgGen.SelectedValue != null)
                    {
                        result = result.Where(o => o.GeneroID == (int)cbxProgGen.SelectedValue);
                    }

                }
                GdgProgDesc.Visibility = Visibility.Visible;
                GdgProgDesc.ItemsSource = result.ToList();

            }
        }

        private void CarregarComboPagamentosEditoras()
        {
            cbPagEditora.ItemsSource = (from e in context.Editoras
                                        orderby e.Nome
                                        select e).ToList();
            cbPagEditora.Items.Refresh();
        }

        private void CarregarComboAssociacoes()
        {
            cbxEditoraAss.ItemsSource = (from e in context.Associacoes
                                         orderby e.Nome
                                         select e).ToList();
            cbxEditoraAss.Items.Refresh();

        }

        private void cbProvGenero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbProvProg.DataContext = null;

            //Obtem programas a partir do genero
            if (cbProvGen.SelectedIndex != -1)
            {
                //Programas
                using (Repositorio repositorio = new Repositorio())
                {
                    var genSel = int.Parse(cbProvGen.SelectedValue.ToString());

                    List<Programa> programas;
                    if (genSel == 0)
                        programas = repositorio
                            .Obter<Programa>()
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();
                    else
                        programas = repositorio
                            .Obter<Programa>(p => p.GeneroID == genSel)
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();

                    var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                    programas.Insert(0, defaultValue);
                    cbProvProg.DataContext = null;
                    if (cbProvProg.ItemsSource != null)
                        cbProvProg.ItemsSource.Clear();
                    cbProvProg.ItemsSource = programas;
                    cbECADAno.SelectedValue = 0;
                    cbProvProg.UpdateLayout();
                }
                btnPesquisar.IsEnabled = true;
            }
        }

        private void cbUtilGenero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbUtilProg.DataContext = null;

            //Obtem programas a partir do genero
            if (cbUtilGen.SelectedIndex != -1)
            {
                //Programas
                using (Repositorio repositorio = new Repositorio())
                {
                    var genSel = int.Parse(cbUtilGen.SelectedValue.ToString());

                    List<Programa> programas;
                    if (genSel == 0)
                        programas = repositorio
                            .Obter<Programa>()
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();
                    else
                        programas = repositorio
                            .Obter<Programa>(p => p.GeneroID == genSel)
                            .AsNoTracking()
                            .OrderBy(p => p.Nome)
                            .ToList();

                    var defaultValue = new Programa {Nome = "--Todos--", ProgramaID = 0};
                    programas.Insert(0, defaultValue);
                    cbUtilProg.DataContext = null;
                    cbUtilProg.DataContext = programas;
                    cbUtilProg.SelectedIndex = 0;
                    cbUtilProg.SelectedValue = 0;
                    cbUtilProg.UpdateLayout();
                }
                btnPesquisar.IsEnabled = true;
            }
        }

        private void ExportarErros(object sender, RoutedEventArgs e)
        {
            ExportarErrosVetrix("musicasNaoProcessadas");
        }

        private void ExportarErrosNovela(object sender, RoutedEventArgs e)
        {
            ExportarErrosVetrixNovela("musicasNovelasNaoProcessadas");
        }

        private void ExportarErrosVetrix(string nomeArquivoExportacao)
        {
            string nomeArquivo = nomeArquivoExportacao;
            // creating Excel Application
            _Application app = new Microsoft.Office.Interop.Excel.Application();

            // creating new WorkBook within Excel application
            _Workbook workbook = app.Workbooks.Add(Type.Missing);

            // creating new Excelsheet in workbook
            _Worksheet worksheet = null;

            // see the excel sheet behind the program
            app.Visible = false;

            // get the reference of first sheet. By default its name is Sheet1.
            // store its reference to worksheet
            worksheet = workbook.Sheets["Plan1"];
            worksheet = workbook.ActiveSheet;

            // changing the name of active sheet
            worksheet.Name = "Exported from gridview";
            Export export = new Export();
            var ds = export.CreateDataSet(dgVetrix.ItemsSource.Cast<RowVetrixErro>().ToList());
            if (ds == null)
            {
                MessageBox.Show("Não existem registros para exportação.");
                return;
            }
            var table = ds.Tables[0];
            // storing header part in Excel
            for (int i = 1; i < dgVetrix.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = dgVetrix.Columns[i - 1].Header.ToString();
            }

            // storing Each row and column value to excel sheet
            for (int i = 0; i < table.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dgVetrix.Columns.Count; j++)
                {
                    // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                    var dt = new DateTime();
                    if (table.Rows[i][j].ToString().Contains('/') &&
                        DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                    {
                        string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                        Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                        rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                        worksheet.Cells[i + 2, j + 1] = dataFormatada;
                    }
                    else
                    {
                        worksheet.Cells[i + 2, j + 1] = table.Rows[i][j].ToString();
                    }
                }
            }
            try
            {
                // save the application
                workbook.SaveAs(_pastaExportacao + @"erros importacao\" + nomeArquivo + ".xls", Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                Type.Missing,
                                Type.Missing, Type.Missing);
                MessageBox.Show("Exportado com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                _logRepository.WriteLog(ex);
            }
            finally
            {
                // Exit from the application
                app.Quit();
            }
        }


        private void EmailTrilhasVetrix(object sender, RoutedEventArgs e)
        {
            Export export = new Export();
            if (_MusicasParaEmail.Count > 0)
            {
                string corpoEmail;
                var ds = export.CreateDataSet(_MusicasParaEmail.ToList());
                var table = ds.Tables[0];
                try
                {
                    corpoEmail = export.ConvertDataTable2HTMLString(table);
                    var email = new EnviarEmail();
                    email.Enviar(corpoEmail);
                    MessageBox.Show("Email enviado!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    _logRepository.WriteLog(ex);
                }
            }
            else
            {
                MessageBox.Show("Não existem itens na consulta");
            }
        }
        
        private void ExportarECAD(object sender, RoutedEventArgs e)
        {
            if (dgECAD.ItemsSource != null && dgECAD.ItemsSource.Cast<RowRelatorioECAD>().Any())
            {
                // creating Excel Application
                _Application app = new Microsoft.Office.Interop.Excel.Application();
                var misValue = Type.Missing;
                var endereco = @"\\paladio03\Sistemas\Sincronizacao\Template\TemplateECAD.xlsx";

                // creating new WorkBook within Excel application
                //Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                _Workbook workbook = app.Workbooks.Open(endereco, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue);
                // creating new Excelsheet in workbook
                _Worksheet worksheet = null;

                // see the excel sheet behind the program
                app.Visible = false;

                // get the reference of first sheet. By default its name is Sheet1.
                // store its reference to worksheet
                worksheet = workbook.Sheets["Plan1"];
                worksheet = workbook.ActiveSheet;

                // changing the name of active sheet
                worksheet.Name = "Exported from gridview";
                Export export = new Export();


                var ds = export.CreateDataSet(dgECAD.ItemsSource.Cast<RowRelatorioECAD>().ToList());

                // storing header part in Excel
                //-2 elimina as ultimas duas colunas da grid
                for (int i = 1; i < dgECAD.Columns.Count + 1 - 2; i++)
                {
                    //linha,coluna
                    worksheet.Cells[5, i] = dgECAD.Columns[i - 1].Header.ToString();
                }

                if (ds != null)
                {
                    var table = ds.Tables[0];

                    // storing Each row and column value to excel sheet
                    for (int i = 0; i <= table.Rows.Count - 1; i++)
                    {
                        //-2 elimina as ultimas duas colunas da grid
                        for (int j = 0; j < dgECAD.Columns.Count - 2; j++)
                        {
                            // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                            var dt = new DateTime();
                            if (table.Rows[i][j].ToString().Contains('/') &&
                                DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                            {
                                string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                                Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                                rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                                worksheet.Cells[i + 6, j + 1] = dataFormatada;
                            }
                            else
                            {
                                worksheet.Cells[i + 6, j + 1] = table.Rows[i][j].ToString();
                            }
                        }
                    }

                    try
                    {
                        // save the application
                        workbook.SaveAs(_pastaExportacao + @"relatorios\"
                                        + cbECADTipo.Text
                                        + "_"
                                        + cbECADMes.Text
                                        + "_"
                                        + cbECADAno.Text
                                        + ".xls",
                                        Type.Missing, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing,
                                        XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing);
                        MessageBox.Show("Relatório Exportado!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                        _logRepository.WriteLog(ex);
                    }
                }
                // Exit from the application
                app.Quit();
            }
            else
            {
                MessageBox.Show("A grid não contém dados.");
            }

        }

        private void ExportarProvisao(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProvisao.ItemsSource != null && dgProvisao.ItemsSource.Cast<RowRelatorioProvisao>().Any())
                {
                    // creating Excel Application
                    _Application app = new Microsoft.Office.Interop.Excel.Application();
                    var misValue = Type.Missing;
                    var endereco = @"\\paladio03\Sistemas\Sincronizacao\Template\TemplateECAD.xlsx";

                    // creating new WorkBook within Excel application
                    //Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                    _Workbook workbook = app.Workbooks.Open(endereco, misValue, misValue,
                                                            misValue, misValue, misValue,
                                                            misValue, misValue, misValue,
                                                            misValue, misValue, misValue,
                                                            misValue, misValue, misValue);
                    // creating new Excelsheet in workbook
                    _Worksheet worksheet = null;

                    // see the excel sheet behind the program
                    app.Visible = false;

                    // get the reference of first sheet. By default its name is Sheet1.
                    // store its reference to worksheet
                    worksheet = workbook.Sheets["Plan1"];
                    worksheet = workbook.ActiveSheet;

                    // changing the name of active sheet
                    worksheet.Name = "Exported from gridview";
                    Export export = new Export();


                    var ds = export.CreateDataSet(dgProvisao.ItemsSource.Cast<RowRelatorioProvisao>().ToList());

                    // storing header part in Excel
                    for (int i = 1; i < dgProvisao.Columns.Count + 1; i++)
                    {
                        //linha,coluna
                        worksheet.Cells[5, i] = dgProvisao.Columns[i - 1].Header.ToString();
                    }

                    if (ds != null)
                    {
                        var table = ds.Tables[0];

                        // storing Each row and column value to excel sheet
                        for (int i = 0; i <= table.Rows.Count - 1; i++)
                        {
                            for (int j = 0; j < dgProvisao.Columns.Count; j++)
                            {
                                // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                                var dt = new DateTime();
                                if (table.Rows[i][j].ToString().Contains('/') &&
                                    DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                                {
                                    string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                                    Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                                    rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                                    worksheet.Cells[i + 6, j + 1] = dataFormatada;
                                }
                                else
                                {
                                    worksheet.Cells[i + 6, j + 1] = table.Rows[i][j].ToString();
                                }
                            }
                        }

                        try
                        {
                            // save the application
                            workbook.SaveAs(_pastaExportacao + @"relatorios\"
                                            + "Provisao"
                                            + "_"
                                            + cbProvMes.Text
                                            + "_"
                                            + cbProvAno.Text
                                            + ".xls",
                                            Type.Missing, Type.Missing,
                                            Type.Missing, Type.Missing, Type.Missing,
                                            XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                            Type.Missing, Type.Missing, Type.Missing);
                            MessageBox.Show("Relatório Exportado!");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                            _logRepository.WriteLog(ex);
                        }
                        finally
                        {
                            // Exit from the application
                            app.Quit();
                        }
                    }
                    // Exit from the application
                    app.Quit();
                }
                else
                {
                    MessageBox.Show("A grid não contém dados.");
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
                _logRepository.WriteLog(ex);
            }
        }

        private void ExportarUtilizacao(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgUtil.ItemsSource != null && dgUtil.ItemsSource.Cast<RowRelatorioUtilizacao>().Any())
                {
                    // creating Excel Application
                    _Application app = new Microsoft.Office.Interop.Excel.Application();
                    var misValue = Type.Missing;
                    var endereco = @"\\paladio03\Sistemas\Sincronizacao\Template\TemplateECAD.xlsx";

                    // creating new WorkBook within Excel application
                    //Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                    _Workbook workbook = app.Workbooks.Open(endereco, misValue, misValue,
                                                            misValue, misValue, misValue,
                                                            misValue, misValue, misValue,
                                                            misValue, misValue, misValue,
                                                            misValue, misValue, misValue);
                    // creating new Excelsheet in workbook
                    _Worksheet worksheet = null;

                    // see the excel sheet behind the program
                    app.Visible = false;

                    // get the reference of first sheet. By default its name is Sheet1.
                    // store its reference to worksheet
                    worksheet = workbook.Sheets["Plan1"];
                    worksheet = workbook.ActiveSheet;

                    // changing the name of active sheet
                    worksheet.Name = "Exported from gridview";
                    Export export = new Export();


                    var ds = export.CreateDataSet(dgUtil.ItemsSource.Cast<RowRelatorioUtilizacao>().ToList());

                    // storing header part in Excel
                    for (int i = 1; i < dgUtil.Columns.Count + 1; i++)
                    {
                        //linha,coluna
                        worksheet.Cells[5, i] = dgUtil.Columns[i - 1].Header.ToString();
                    }

                    if (ds != null)
                    {
                        var table = ds.Tables[0];

                        // storing Each row and column value to excel sheet
                        for (int i = 0; i <= table.Rows.Count - 1; i++)
                        {
                            for (int j = 0; j < dgUtil.Columns.Count; j++)
                            {
                                // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                                var dt = new DateTime();
                                if (table.Rows[i][j].ToString().Contains('/') &&
                                    DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                                {
                                    string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                                    Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                                    rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                                    worksheet.Cells[i + 6, j + 1] = dataFormatada;
                                }
                                else
                                {
                                    worksheet.Cells[i + 6, j + 1] = table.Rows[i][j].ToString();
                                }
                            }
                        }

                        try
                        {
                            // save the application
                            workbook.SaveAs(_pastaExportacao + @"relatorios\"
                                            + "Utilizacao"
                                            + "_"
                                            + dtUtilIni.Text.Replace(@"/", @".")
                                            + "_"
                                            + dtUtilFin.Text.Replace(@"/", @".")
                                            + ".xls",
                                            Type.Missing, Type.Missing,
                                            Type.Missing, Type.Missing, Type.Missing,
                                            XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                            Type.Missing, Type.Missing, Type.Missing);
                            MessageBox.Show("Relatório Exportado");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                            _logRepository.WriteLog(ex);
                        }
                        finally
                        {
                            // Exit from the application
                            app.Quit();
                        }
                    }
                    // Exit from the application
                    app.Quit();
                }
                else
                {
                    MessageBox.Show("A grid não contém dados.");
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
                _logRepository.WriteLog(ex);
            }
        }

        private void ExportarRanking(object sender, RoutedEventArgs e)
        {
            if (dgRanking.ItemsSource != null && dgRanking.ItemsSource.Cast<RowRelatorioRanking>().Any())
            {
                // creating Excel Application
                _Application app = new Microsoft.Office.Interop.Excel.Application();
                var misValue = Type.Missing;
                var endereco = @"\\paladio03\Sistemas\Sincronizacao\Template\TemplateECAD.xlsx";

                // creating new WorkBook within Excel application
                //Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                _Workbook workbook = app.Workbooks.Open(endereco, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue);
                // creating new Excelsheet in workbook
                _Worksheet worksheet = null;

                // see the excel sheet behind the program
                app.Visible = false;

                // get the reference of first sheet. By default its name is Sheet1.
                // store its reference to worksheet
                worksheet = workbook.Sheets["Plan1"];
                worksheet = workbook.ActiveSheet;

                // changing the name of active sheet
                worksheet.Name = "Exported from gridview";
                Export export = new Export();


                var ds = export.CreateDataSet(dgRanking.ItemsSource.Cast<RowRelatorioRanking>().ToList());

                // storing header part in Excel
                //-2 elimina as ultimas duas colunas da grid
                for (int i = 1; i < dgRanking.Columns.Count + 1 - 2; i++)
                {
                    //linha,coluna
                    worksheet.Cells[5, i] = dgRanking.Columns[i - 1].Header.ToString();
                }

                if (ds != null)
                {
                    var table = ds.Tables[0];

                    // storing Each row and column value to excel sheet
                    for (int i = 0; i <= table.Rows.Count - 1; i++)
                    {
                        //-2 elimina as ultimas duas colunas da grid
                        for (int j = 0; j < dgRanking.Columns.Count - 2; j++)
                        {
                            // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                            var dt = new DateTime();
                            if (table.Rows[i][j].ToString().Contains('/') &&
                                DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                            {
                                string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                                Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                                rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                                worksheet.Cells[i + 6, j + 1] = dataFormatada;
                            }
                            else
                            {
                                worksheet.Cells[i + 6, j + 1] = table.Rows[i][j].ToString();
                            }
                        }
                    }

                    try
                    {
                        // save the application
                        workbook.SaveAs(_pastaExportacao + @"relatorios\"
                                        + "Ranking"
                                        + "_"
                                        + dtRankIni.Text.Replace(@"/", @".")
                                        + "_"
                                        + dtRankFin.Text.Replace(@"/", @".")
                                        + ".xls",
                                        Type.Missing, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing,
                                        XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing);
                        MessageBox.Show("Relatório Exportado!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                        _logRepository.WriteLog(ex);
                    }
                }
                // Exit from the application
                app.Quit();
            }
            else
            {
                MessageBox.Show("A grid não contém dados.");
            }
        }

        private void ExportarCanhoto(object sender, RoutedEventArgs e)
        {
            if (dgCanhotos.ItemsSource != null &&
                dgCanhotos.ItemsSource.Cast<RowRelatorioCanhoto>().Any())
            {
                // creating Excel Application
                _Application app = new Microsoft.Office.Interop.Excel.Application();
                var misValue = Type.Missing;
                var endereco = @"\\paladio03\Sistemas\Sincronizacao\Template\TemplateECAD.xlsx";

                // creating new WorkBook within Excel application
                //Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                _Workbook workbook = app.Workbooks.Open(endereco, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue);
                // creating new Excelsheet in workbook
                _Worksheet worksheet = null;

                // see the excel sheet behind the program
                app.Visible = false;

                // get the reference of first sheet. By default its name is Sheet1.
                // store its reference to worksheet
                worksheet = workbook.Sheets["Plan1"];
                worksheet = workbook.ActiveSheet;

                // changing the name of active sheet
                worksheet.Name = "Exported from gridview";
                Export export = new Export();


                var ds = export.CreateDataSet(dgCanhotos.ItemsSource.Cast<RowRelatorioCanhoto>().ToList());

                // storing header part in Excel
                //-2 elimina as ultimas duas colunas da grid
                for (int i = 1; i < dgCanhotos.Columns.Count + 1 - 2; i++)
                {
                    //linha,coluna
                    worksheet.Cells[5, i] = dgCanhotos.Columns[i - 1].Header.ToString();
        }

                if (ds != null)
                {
                    var table = ds.Tables[0];

                    // storing Each row and column value to excel sheet
                    for (int i = 0; i <= table.Rows.Count - 1; i++)
                    {
                        //-2 elimina as ultimas duas colunas da grid
                        for (int j = 0; j < dgCanhotos.Columns.Count - 2; j++)
                        {
                            // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                            var dt = new DateTime();
                            if (table.Rows[i][j].ToString().Contains('/') &&
                                DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                            {
                                string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                                Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                                rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                                worksheet.Cells[i + 6, j + 1] = dataFormatada;
                            }
                            else
                            {
                                worksheet.Cells[i + 6, j + 1] = table.Rows[i][j].ToString();
                            }
                        }
                    }

                    try
                    {
                        // save the application
                        workbook.SaveAs(_pastaExportacao + @"relatorios\"
                                        + "Canhoto"
                                        + "_"
                                        + cbCanhotEdit.Text
                                        + "_"
                                        + dtCanhotIni.Text.Replace(@"/", @".")
                                        + "_"
                                        + dtCanhotFin.Text.Replace(@"/", @".")
                                        + ".xls",
                                        Type.Missing, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing,
                                        XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing);
                        MessageBox.Show("Relatório Exportado!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                        _logRepository.WriteLog(ex);
                    }
                }
                // Exit from the application
                app.Quit();
            }
            else
        {
                MessageBox.Show("A grid não contém dados.");
            }

        }

        private void ExportarPgtoAberto(object sender, RoutedEventArgs e)
        {
            if (dgPgtoAberto.ItemsSource != null && dgPgtoAberto.ItemsSource.Cast<RowRelatorioPgtoAberto>().Any())
            {
                // creating Excel Application
                _Application app = new Microsoft.Office.Interop.Excel.Application();
                var misValue = Type.Missing;
                var endereco = @"\\paladio03\Sistemas\Sincronizacao\Template\TemplateECAD.xlsx";

                // creating new WorkBook within Excel application
                //Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
                _Workbook workbook = app.Workbooks.Open(endereco, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue,
                                                        misValue, misValue, misValue);
                // creating new Excelsheet in workbook
                _Worksheet worksheet = null;

                // see the excel sheet behind the program
                app.Visible = false;

                // get the reference of first sheet. By default its name is Sheet1.
                // store its reference to worksheet
                worksheet = workbook.Sheets["Plan1"];
                worksheet = workbook.ActiveSheet;

                // changing the name of active sheet
                worksheet.Name = "Exported from gridview";
                Export export = new Export();


                var ds = export.CreateDataSet(dgPgtoAberto.ItemsSource.Cast<RowRelatorioPgtoAberto>().ToList());

                // storing header part in Excel
                //-2 elimina as ultimas duas colunas da grid
                for (int i = 1; i < dgPgtoAberto.Columns.Count + 1; i++)
                {
                    //linha,coluna
                    worksheet.Cells[5, i] = dgPgtoAberto.Columns[i - 1].Header.ToString();
                }

                if (ds != null)
                {
                    var table = ds.Tables[0];

                    // storing Each row and column value to excel sheet
                    for (int i = 0; i <= table.Rows.Count - 1; i++)
                    {
                        //-2 elimina as ultimas duas colunas da grid
                        for (int j = 0; j < dgPgtoAberto.Columns.Count; j++)
                        {
                            // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                            var dt = new DateTime();
                            if (table.Rows[i][j].ToString().Contains('/') &&
                                DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                            {
                                string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                                Range rg = (Range) worksheet.Cells[i + 2, j + 1];
                                rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                                worksheet.Cells[i + 6, j + 1] = dataFormatada;
                            }
                            else
                            {
                                worksheet.Cells[i + 6, j + 1] = table.Rows[i][j].ToString();
                            }
                        }
                    }

                    try
                    {
                        // save the application
                        workbook.SaveAs(_pastaExportacao + @"relatorios\"
                                        + "PgtoAberto"
                                        + "_"
                                        + cbPgAbertMes.Text
                                        + "_"
                                        + cbPgAbertAno.Text
                                        + ".xls",
                                        Type.Missing, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing,
                                        XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                        Type.Missing, Type.Missing, Type.Missing);
                        MessageBox.Show("Relatório Exportado!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                        _logRepository.WriteLog(ex);
                    }
                }
                // Exit from the application
                app.Quit();
            }
            else
            {
                MessageBox.Show("A grid não contém dados.");
            }
        }


        private void TrataCampo(object sender, TextChangedEventArgs e)
        {
            var box = ((TextBox) sender);
            box.Text = box.Text.Normalizar();
            box.CaretIndex = box.Text.Length;
           // clickAll();
        }

        private void gdgPrecoDesc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ImportarNovela(object sender, RoutedEventArgs e)
        {
            ClearGrid(dgVetrixNovela);

            string arquivo = null;
            var dialog = new OpenFileDialog {RestoreDirectory = true};

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
                arquivo = dialog.FileName;

            var extension = Path.GetExtension(arquivo);
            if (extension != null && (!string.IsNullOrEmpty(arquivo) &&
                                      (extension.Equals(".xls", StringComparison.CurrentCultureIgnoreCase)
                                       || extension.Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase)
                                      )))
            {
                var confirma = MessageBox.Show(
                    "Gostaria de iniciar a importação do arquivo " + Path.GetExtension(arquivo) + "?",
                    "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                if (confirma)
                {
                    try
                    {
                        List<RowVetrixErroNovela> erros = null;
                        ShowLoader();
                        ThreadStart dataDownloadThread = delegate
                                                             {

                                                                 var ext = Path.GetExtension(arquivo);
                                                                 if (ext != null &&
                                                                     (ext.Equals(".xls",
                                                                                        StringComparison
                                                                                            .CurrentCultureIgnoreCase)
                                                                      ||
                                                                      ext.Equals(".xlsx",
                                                                                        StringComparison
                                                                                            .CurrentCultureIgnoreCase)))
                                                                     erros =
                                                                         new ImportacaoFiliais().ImportarNovela(arquivo,
                                                                                                          out
                                                                                                              _totalImportadasMusicas,
                                                                                                          out
                                                                                                              _totalImportadasSucesso,
                                                                                                          out
                                                                                                              _totalImportadasErros);

                                                                 Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                                        (EventHandler)
                                                                                        delegate
                                                                                            {
                                                                                                HideLoader();
                                                                                                dgVetrixNovela.ItemsSource =
                                                                                                    erros;
                                                                                           
                                                                                                MessageBox.Show(
                                                                                                    "Resultado:" +
                                                                                                    Environment.NewLine +
                                                                                                    "Musicas encontradas: " +
                                                                                                    _totalImportadasMusicas +
                                                                                                    Environment.NewLine +
                                                                                                    "Importadas com sucesso: " +
                                                                                                    _totalImportadasSucesso +
                                                                                                    Environment.NewLine +
                                                                                                    "Não importadas: " +
                                                                                                    _totalImportadasErros +
                                                                                                    Environment.NewLine,
                                                                                                    "Importação");
                                                                                            }, null, null);
                                                             };
                        dataDownloadThread.BeginInvoke(
                            delegate(IAsyncResult aysncResult) { dataDownloadThread.EndInvoke(aysncResult); }, null);

                        _logRepository.WriteLog("Importação Novela", LogType.Informacao, _usuario.Login);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Importação Novela");
                        _logRepository.WriteLog(ex);
                    }

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(arquivo))
                    MessageBox.Show("Arquivo inválido!");
            }
        }

        private void ExportarErrosVetrixNovela(string nomeArquivoExportacao)
        {
            string nomeArquivo = nomeArquivoExportacao;
            // creating Excel Application
            _Application app = new Microsoft.Office.Interop.Excel.Application();

            // creating new WorkBook within Excel application
            _Workbook workbook = app.Workbooks.Add(Type.Missing);

            // creating new Excelsheet in workbook
            _Worksheet worksheet = null;

            // see the excel sheet behind the program
            app.Visible = false;

            // get the reference of first sheet. By default its name is Sheet1.
            // store its reference to worksheet
            worksheet = workbook.Sheets["Plan1"];
            worksheet = workbook.ActiveSheet;

            // changing the name of active sheet
            worksheet.Name = "Exported from gridview";
            Export export = new Export();
            var ds = export.CreateDataSet(dgVetrixNovela.ItemsSource.Cast<RowVetrixErroNovela>().ToList());
            if (ds == null)
            {
                MessageBox.Show("Não existem registros para exportação.");
                return;
            }
            var table = ds.Tables[0];
            // storing header part in Excel
            for (int i = 1; i < dgVetrixNovela.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = dgVetrixNovela.Columns[i - 1].Header.ToString();
            }

            // storing Each row and column value to excel sheet
            for (int i = 0; i < table.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dgVetrixNovela.Columns.Count; j++)
                {
                    // var x = ((DataTable) dgVetrix.ItemsSource).Rows;
                    var dt = new DateTime();
                    if (table.Rows[i][j].ToString().Contains('/') &&
                        DateTime.TryParse(table.Rows[i][j].ToString(), out dt))
                    {
                        string dataFormatada = String.Format("{0:dd/MM/yyyy}", dt);
                        Range rg = (Range)worksheet.Cells[i + 2, j + 1];
                        rg.EntireColumn.NumberFormat = "MM/DD/YYYY";
                        worksheet.Cells[i + 2, j + 1] = dataFormatada;
                    }
                    else
                    {
                        worksheet.Cells[i + 2, j + 1] = table.Rows[i][j].ToString();
                    }
                }
            }
            try
            {
                // save the application
                workbook.SaveAs(_pastaExportacao + @"erros importacao\" + nomeArquivo + ".xls", Type.Missing,
                                Type.Missing, Type.Missing,
                                Type.Missing, Type.Missing,
                                XlSaveAsAccessMode.xlExclusive, Type.Missing,
                                Type.Missing,
                                Type.Missing, Type.Missing);
                MessageBox.Show("Exportado com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao salvar o arquivo: " + ex.Message);
                _logRepository.WriteLog(ex);
            }
            finally
            {
                // Exit from the application
                app.Quit();
            }
        }

        private void dgMusCadastradas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void tabTotalRel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof (TabControl))
            {
                //ECAD
                ClearGrid(dgECAD);
                LimparCombos(panelECAD);
                LimparDatas(panelECAD);
                //Provisao
                ClearGrid(dgProvisao);
                LimparCombos(panelProv1);
                LimparDatas(panelProv1);
                LimparCombos(panelProv2);
                LimparDatas(panelProv2);
                //Ranking
                ClearGrid(dgRanking);
                LimparCombos(panelRank1);
                LimparDatas(panelRank1);
                LimparCombos(panelRank2);
                LimparDatas(panelRank2);
                //Canhotos
                ClearGrid(dgCanhotos);
                LimparCombos(panelCanhot1);
                LimparDatas(panelCanhot1);
                LimparCombos(panelCanhot2);
                LimparDatas(panelCanhot2);
                //PgtoAberto
                ClearGrid(dgPgtoAberto);
                LimparCombos(panelPgAbert1);
                LimparDatas(panelPgAbert1);
                LimparCombos(panelPgAbert2);
                LimparDatas(panelPgAbert2);
                //Utilizacao
                ClearGrid(dgUtil);
                LimparCombos(panelUtil1);
                LimparDatas(panelUtil1);
                LimparCombos(panelUtil2);
                LimparDatas(panelUtil2);

                e.Handled = true;
            }
        }


        private void tabTotalCadastros_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TabControl))
            {
                //PROGRAMAS
                ClearGrid(GdgProgDesc);
                txbProgDesc.Clear();
                txbProgOrdem.Clear();
                cbxProgGen.SelectedValue = -1;

                //NOVELA
                ClearGrid(GdgNovelasDesc);
                cbNovelaGen.SelectedValue = 0;
                cbNovelaProg.SelectedValue = 0;
                txbNovelaTituloN.Clear();
                txbNovelaTituloO.Clear();
                txbNovelaProdutor.Clear();
                txbNovelaAutor.Clear();
                txbNovelaDiretor.Clear();
                dateNovelaInicial.Text = "";
               
                //TIPO DE EXIBIÇÃO
                ClearGrid(GdgExbDesc);
                txbExbDesc.Clear();


                //CLASSIFICAÇÃO
                ClearGrid(gdgClassDesc);
                txbClassNome.Clear();

                //QUADRO
                ClearGrid(gdgQuadroDesc);
                txbQuadrosNome.Clear();

                //GÊNEROS
                ClearGrid(gdgGenDesc);
                txbGenNome.Clear();
                cbxGenClass.SelectedValue = -1;
                lstGen.Items.Clear();

                //EMPRESAS
                ClearGrid(gdgAssDesc);
                ClearGrid(gdgEditoraDesc);
                ClearGrid(gdgGravadoraDesc);
                txbAssNome.Clear();
                cbxEditoraAss.SelectedValue = -1;
                txbEditoraNome.Clear();
                txbEditoraRazao.Clear();
                txbEditoraCNPJ.Clear();
                txbEditoraEnd.Clear();
                txbEditoraN.Clear();
                txbEditoraBairro.Clear();
                txbEditoraCep.Clear();
                txbEditoraComp.Clear();
                txbEditoraMail.Clear();
                txbEditoraMail1.Clear();
                txbEditoraDDD.Clear();
                txbEditoraDDD1.Clear();
                txbEditoraContato.Clear();
                txbEditoraContato1.Clear();
                txbGravadoraaNome.Clear();
                txbGravadoraRazao.Clear();
                txbGravadoraCNPJ.Clear();
                txbGravadoraEnd.Clear();
                txbGravadoraN.Clear();
                txbGravadoraBairro.Clear();
                txbGravadoraCep.Clear();
                txbGravadoraComp.Clear();
                txbGravadoraMail.Clear();
                txbGravadoraMail1.Clear();
                txbGravadoraDDD.Clear();
                txbGravadoraDDD1.Clear();
                txbGravadoraContato.Clear();
                txbGravadoraContato1.Clear();

                //PREÇOS
                ClearGrid(gdgPrecoDesc);
                cbxPrecoGen.SelectedValue = -1;
                lstPreco.SelectedItem = -1;
                cbxPrecoClass.SelectedValue = -1;
                cbxPrecoAno.SelectedValue = -1;
                cbxPrecoAss.SelectedValue = -1;
                cbxPrecoAbr.SelectedValue = -1;
                txbPrecoValor.Clear();
               
                //MÚSICAS
                ClearGrid(dgMusCadastradas);
                txtMusTitulo.Clear();
                txtMusAutor.Clear();
                txtMusArquivo.Clear();
                txtMusInterprete.Clear();
                txtMusDuracao.Clear();
                txtMusISRC.Clear();
                lblMusErro.Content = "";


                e.Handled = true;
            }
        }


        /// <summary>
        /// Atribui indice -1 a todos combox do formulario passado por parametro
        /// </summary>
        /// <param name="formulario"></param>
        private void LimparCombos(DependencyObject formulario)
        {
            foreach (ComboBox comboBox in FindVisualChildren<ComboBox>(formulario))
            {
                comboBox.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Atribui data/hora atual a todos datepicker do formulario passado por parametro
        /// </summary>
        /// <param name="formulario"></param>
        private void LimparDatas(DependencyObject formulario)
        {
            foreach (DatePicker date in FindVisualChildren<DatePicker>(formulario))
            {
                date.SelectedDate = DateTime.Now;
            }
        }

        private void gdgClassDesc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void clickAll()
        {
            btProgSalvar.IsEnabled = true;
            btNovelaSalvar.IsEnabled = true;
            btExbSalvar.IsEnabled = true;
            btClassSalvar.IsEnabled = true;
            btGenSalvar.IsEnabled = true;
            btAssSalvar.IsEnabled = true;
            btPrecoSalvar.IsEnabled = true;
            btnMusSalvar.IsEnabled = true;
        }
    }
}