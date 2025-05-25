using System.Windows;
using System;
using System.Data.Linq;
using RevitTemplate.UI.Views.Pages;
using RevitTemplate.Services;

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
            
            // Inicializa o LogService com o Dispatcher desta janela
            LogService.Initialize(this.Dispatcher);

            // Registra esta janela no NavigationService
            NavService.RegisterMainWindow(this);
            
            // Navega para a página de login quando a janela é carregada
            this.Loaded += CiclopeWindow_Loaded;
            
            // Desregistra quando a janela é fechada
            this.Closed += CiclopeWindow_Closed;
        }
        private bool CheckSavedToken()
        {
            try
            {
                var savedToken = TokenService.GetSavedToken();
                if (savedToken != null && TokenService.HasValidToken())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao verificar token salvo: {ex.Message}");
                return false;
            }
        }
        private void CiclopeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Define a página inicial como LoginPage
            if(CheckSavedToken())
            {
                ContentFrame.Navigate(new LogPage());
            }
            else { 
                ContentFrame.Navigate(new LoginPage());
            }
        }

        private void CiclopeWindow_Closed(object sender, EventArgs e)
        {
            // Desregistra a janela do NavigationService
            NavService.Unregister();
        }

        private void ContentFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Você pode adicionar lógica aqui quando a navegação ocorrer
        }
        
        // Método helper para navegar para outras páginas
        public void NavigateToPage(object page)
        {
            ContentFrame.Navigate(page);
        }
    }
}