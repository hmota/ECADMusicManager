using System;
using System.Collections.Generic;
using System.Configuration;
using System.Deployment.Application;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using SincronizacaoMusical.Domain.Entities;
using SincronizacaoMusical.Data.Repositories;
using SincronizacaoMusical.Domain;

namespace SincronizacaoMusical.UI
{
    /// <summary>
    /// Interaction logic for Unidade.xaml
    /// </summary>
    public partial class SelecaoUnidade : Window
    {
        private List<Unidade> Unidades = null;

        public SelecaoUnidade()
        {
            InitializeComponent();
            Unidades = new UnidadeRepository().GetUnidades();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbUnidade.DataContext = Unidades;
            var user = Environment.UserName;
            ConfigurationManager.AppSettings["Login"] = user;
            using (Context context = new Context())
            {
                ConfigurationManager.AppSettings["Debug"] = context.Configuracoes.FirstOrDefault(c => c.Chave == "Debug").Valor;
            }
            Version v = GetRunningVersion();

            var About = string.Format(CultureInfo.InvariantCulture, @"Versão {0}.{1}.{2} (r{3})", v.Major, v.Minor,
                                      v.Build, v.Revision);

            ConfigurationManager.AppSettings["Versao"] = About;
            
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (cbUnidade.SelectedIndex != -1)
            {
                Properties.Settings.Default.Unidade = cbUnidade.SelectedValue.ToString();
                var main = new MainWindow();
                Hide();
                main.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Selecione uma unidade.");
            }
        }

        private Version GetRunningVersion()
        {
            try
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

    }
}
