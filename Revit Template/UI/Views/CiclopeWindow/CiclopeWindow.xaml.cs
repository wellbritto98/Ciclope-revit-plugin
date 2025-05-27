using System.Windows;
using System;
using System.Data.Linq;
using RevitTemplate.UI.Views.Pages;
using RevitTemplate.Services;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace RevitTemplate.UI.Views
{    /// <summary>
    /// Interaction logic for CiclopeWindow.xaml
    /// </summary>
    public partial class CiclopeWindow : FluentWindow
    {
        private readonly TokenService _tokenService;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CiclopeWindow"/> class.
        /// </summary>
        public CiclopeWindow(TokenService tokenService, IServiceProvider serviceProvider)
        {
            _tokenService = tokenService ;
            _serviceProvider = serviceProvider;
            
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
        }        private bool CheckSavedToken()
        {
            try
            {
                if (_tokenService == null) return false;
                
                var savedToken = _tokenService.GetSavedToken();
                if (savedToken != null && savedToken.Authenticated && savedToken.Expiration > DateTime.Now)
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
        }        private void CiclopeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var httpService = _serviceProvider.GetRequiredService<HttpService>();
            var hubService = _serviceProvider.GetRequiredService<HubService>();
            // Define a página inicial como LoginPage
            if (CheckSavedToken())
            {
                var logPage = new LogPage(_tokenService, httpService, hubService, _serviceProvider);
                ContentFrame.Navigate(logPage);
            }
            else { 
                var loginPage = new LoginPage(httpService, _tokenService, _serviceProvider);
                ContentFrame.Navigate(loginPage);
            }
        }

        private void CiclopeWindow_Closed(object sender, EventArgs e)
        {
            // Desregistra a janela do NavigationService
            NavService.Unregister();
            var hub = _serviceProvider.GetRequiredService<HubService>();
            hub.Dispose();
        }

        private void ContentFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Você pode adicionar lógica aqui quando a navegação ocorrer
        }
        
        /// <summary>
        /// Helper method to navigate to other pages.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        public void NavigateToPage(object page)
        {
            ContentFrame.Navigate(page);
        }
    }
}