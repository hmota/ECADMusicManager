using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using SincronizacaoMusical.Domain.Business;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Domain.Repositories;
using SincronizacaoMusical.Domain.ViewModels;
using SincronizacaoMusical.Domain;
using DataGrid = System.Windows.Controls.DataGrid;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace SincronizacaoMusical.UI
{
    /// <summary>
    /// Interaction logic for AutorizacaoWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private Context context = null;
        //private int _musSonID = 0;
        private int _musID = 0;
        private int _autID = 0;
        private int _sincID = 0;
        private int _sonID = 0;
        private int _classID = -1;
        private int _genID = -1;
        //private int generoID = -1;
        private decimal _totalPorcentagem = 0;
        private decimal _total;
        private readonly Usuario _usuario = new VerificarUsuario().ObterUsuario(ConfigurationManager.AppSettings["Login"]);

        public PaymentWindow()
        {
            InitializeComponent();
            context = SingletonContext.Instance.Context;
        }

        public PaymentWindow(int autID, int musID, int sincID, int sonID)
        {
            _musID = musID;
            _sincID = sincID;
            _autID = autID;
            _totalPorcentagem = 0;
            _sonID = sonID;

            InitializeComponent();
            context = SingletonContext.Instance.Context;
            CarregaGridPagamentos();

            ObterMusica();

            CarregarComboPagamentosEditoras();

            dtPagVencimento.DisplayDateStart = DateTime.Now;
        }

        private void CarregarComboPagamentosEditoras()
        {
            cbPagEditora.ItemsSource = (from e in context.Editoras
                                        orderby e.Nome
                                        select e).ToList();
            cbPagEditora.Items.Refresh();
        }

        private void CarregaGridPagamentos()
        {
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

            dgPagamentos.ItemsSource = pagamentosMusSon;
            dgPagamentos.Items.Refresh();

            _totalPorcentagem = 0;
            foreach (var pagamentos in pagamentosMusSon)
            {
                _totalPorcentagem += pagamentos.FirstOrDefault().Porcentagem;
            }

            if (_totalPorcentagem > 100)
            {
                lblPagAdicional.Visibility = Visibility.Visible;
                panelPagPoutPourri.Visibility = Visibility.Visible;
            }
        }

        private void ObterMusica()
        {
            Sonorizacao mus = (from ms in context.Sonorizacoes
                                     where ms.SincronizacaoID == _sincID
                                     && ms.MusicaID == _musID
                                     select ms).FirstOrDefault();
            if (mus != null)
            {
                _classID = mus.ClassificacaoID;
            }

            lblMusicaPag.Content = mus.Musica.Titulo + " - "+ mus.Musica.Autor.Nome;
        }

        private void SalvarAutorizacao(object sender, RoutedEventArgs e)
        {
            if (_total>100)
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
                        using (Repositorio repositorio = new Repositorio())
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
                    CarregaGridPagamentos();

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
                }
            }
        }

        private void dgPagamentos_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private void cbPagEditora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int anoVigencia = (from s in context.Sincronizacoes
                               where s.SincronizacaoID == _sincID
                               select s.Exibicao.Data.Year).FirstOrDefault(); 

            Editora edit = null;
            //Obtem valores a partir da editora selecionada e a musica
            if (cbPagEditora.SelectedIndex != -1)
            {
                //Editora
                edit = (from ed in context.Editoras
                        where ed.EditoraID == (int)cbPagEditora.SelectedValue
                        select ed).SingleOrDefault();
            }

            if (edit != null)
            {
                //TODO: Filtrar Precos por genero
                var precos = (from p in context.Precos
                              where  p.ClassificacaoID == _classID
                                     && p.GeneroID == _genID
                                     && p.Vigencia == anoVigencia
                              select p).Distinct().ToList();
                if (precos != null)
                {
                    cbPagValor.ItemsSource = precos;
                }
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {

        }

        private void HabilitaPoutPourri(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(((TextBox)sender).Text))
            {
                _total = _totalPorcentagem + decimal.Parse(((TextBox) sender).Text);
                
                if (_total > 100)
                {
                    panelPagPoutPourri.Visibility = Visibility.Visible;

                    btnSalvarAutorizacao.IsEnabled = false;
                }
                else
                {
                    chkPagPoutPourri.IsChecked = false;
                    chkPagIncidental.IsChecked = false;
                    panelPagPoutPourri.Visibility = Visibility.Hidden;

                    btnSalvarAutorizacao.IsEnabled = true;
                }
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

        #region DatePicker Events

        private void datePicker_TextChanged(object sender)
        {
            DateTime dt;
            DatePicker dp = (sender as DatePicker);
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
                //if (cbGenero.SelectedIndex != -1 && datePickerInicial.SelectedDate.HasValue)
                //{
                //    PesquisarExibicoes();
                //}
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var link = e.OriginalSource as Hyperlink;

            string target = link.NavigateUri.LocalPath;
            //target = Path.Combine(Directory.GetCurrentDirectory() + target);
            var fileInfo = new FileInfo(target);
            if (!fileInfo.Exists)
            {
                MessageBox.Show("O arquivo não foi encontrado: " + target);
            }
            else
            {
                try
                {
                    Process.Start(target);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message);
                }
            }
        }

        #endregion DatePicker Events

        private void DeletarPagamento_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;

            if (e == null || Key.Delete == e.Key )
            {

                if (_usuario.Administrador || _usuario.Supervisor)
                {
                    var result = MessageBox.Show("Deseja excluir os pagamentos selecionados?", "Deletar pagamentos",
                                                 MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        dgPagamentos.CanUserDeleteRows = true;
                        foreach (IGrouping<int, RowRelatorioCanhoto> pgtos in grid.SelectedItems)
                        {

                            Repositorio repositorio = new Repositorio();
                            int autID = pgtos.Key;
                            var autorizacaoSelecionada =
                                repositorio.Obter<Autorizacao>(a => a.AutorizacaoID == autID).Single();
                            if (DateTime.Now < autorizacaoSelecionada.Vencimento.AddDays(1))
                            {
                                repositorio.Remover(autorizacaoSelecionada);
                            }
                            else
                            {
                                MessageBox.Show("Autorização não pode ser excluida. Pagamento já foi efetuado.", "Deletar pagamentos");
                            }
                        }
                    }
                }
            }
        }

        private void DeletarPagamento(object sender, RoutedEventArgs e)
        {
            if (_usuario.Administrador || _usuario.Supervisor)
            {
                DeletarPagamento_PreviewKeyDown(dgPagamentos, null);
                CarregaGridPagamentos();
            }
        }

        private void btnPagProcurarArquivo_Click(object sender, RoutedEventArgs e)
        {
            string arquivo = "";
            var dialog = new OpenFileDialog { DefaultExt = "*.*", RestoreDirectory = true };

            DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
                arquivo = dialog.FileName;

            //TODO: Retirar linha de codigo de teste
            if (!string.IsNullOrEmpty(arquivo))
            {
                txtPagArquivo.Text = arquivo;
            }
        }

        
    }
}
