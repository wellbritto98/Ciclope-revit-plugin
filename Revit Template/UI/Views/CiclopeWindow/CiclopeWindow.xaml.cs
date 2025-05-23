using System.Windows;
using RevitTemplate.UI.ViewModels;
using System;
using System.Data.Linq; // Adiciona a referência ao MahApps.Metro
using RevitTemplate.UI.Views.Pages; // Certifique-se de que o namespace está correto para ParametrosPage

namespace RevitTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for CiclopeWindow.xaml
    /// </summary>
    public partial class CiclopeWindow 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CiclopeWindow"/> class.
        /// </summary>
        public CiclopeWindow()
        {
            InitializeComponent();
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
            DataContext = new CiclopeViewModel();

            // Navega para a página inicial (ParametrosPage) após o carregamento da janela
            Loaded += (_, __) => NavigationView.Navigate(typeof(ParametrosPage));
        }

        private void ContentFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
