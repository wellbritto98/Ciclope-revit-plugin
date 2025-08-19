using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RevitTemplate.Services;
using System.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;


namespace RevitTemplate.UI.Views.Pages
{    /// <summary>
    /// Página de console de logs
    /// </summary>
    public partial class LogPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<LogEntry> _logs;
        private int _logCount;
        private readonly TokenService _tokenService;
        private readonly HttpService _httpService;
        private readonly HubService _hubService;
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets or sets the collection of logs.
        /// </summary>
        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                OnPropertyChanged(nameof(Logs));
                UpdateLogCount();
            }
        }

        /// <summary>
        /// Gets or sets the log count.
        /// </summary>
        public int LogCount
        {
            get => _logCount;
            set
            {
                _logCount = value;
                OnPropertyChanged(nameof(LogCount));
            }
        }

        /// <summary>
        /// Initializes a new instance of the LogPage class.
        /// </summary>
        /// <param name="tokenService">The token service for managing authentication tokens.</param>
        public LogPage(TokenService tokenService, HttpService httpService, HubService hubService, IServiceProvider serviceProvider)
        {
            _tokenService = tokenService;
            _httpService = httpService;
            _hubService = hubService;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            InitializeComponent();
            DataContext = this;

            // Conecta aos logs do serviço
            Logs = LogService.Logs;

            // Inscreve-se no evento de novos logs para auto-scroll
            LogService.LogAdded += OnLogAdded;
            // Emite logs de teste ao entrar na página
            LogService.LogInfo("Autenticado com Sucesso.");

            // Exibe informações sobre o token atual
            string tokenInfo = _tokenService?.GetTokenInfo() ?? "Token não disponível";
            LogService.LogInfo($"Token Status: {tokenInfo}");

            LogService.LogDebug("LogPage inicializada.");

            this.Loaded += LogPage_Loaded;


            UpdateLogCount();
        }
        private void LogPage_Loaded(object sender, RoutedEventArgs e)
        {
            var tokenData = _tokenService?.GetSavedToken();
            var token = tokenData?.Token;

            // Inicializar o HubService com os parâmetros necessários
            _hubService.Initialize("https://developer-hub-hml.olimpo.app.br/revitHub/", token);

            //conectar no hub
            Task.WhenAll(_hubService.ConectarAoHubAsync(token));

            _ = _hubService.EnviarMensagemAsync("Plugin Revit conectado ao servidor CICLOPE");
            
        }

       


        private void OnLogAdded(object sender, LogEntry logEntry)
        {
            // Auto-scroll para o final quando um novo log é adicionado
            // No contexto de plugin do Revit, usamos o Dispatcher da janela
            this.Dispatcher.Invoke(() =>
            {
                UpdateLogCount();
                LogScrollViewer.ScrollToEnd();
            });
        }

        private void UpdateLogCount()
        {
            LogCount = Logs?.Count ?? 0;
        }
        private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
        {
            LogService.ClearLogs();
            UpdateLogCount();
        }
        private void TestLogsButton_Click(object sender, RoutedEventArgs e)
        {
            // Executa testes do LogService
            RevitTemplate.Utils.LogServiceTest.RunTests();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {                // Remove o token salvo
                bool tokenCleared = _tokenService?.ClearToken() ?? false;

                if (tokenCleared)
                {
                    LogService.LogInfo("Logout realizado com sucesso - token removido");
                }
                else
                {
                    LogService.LogWarning("Logout realizado, mas houve problema ao remover o token");
                }


                    _httpService.ClearAuthorizationToken();
                    LogService.LogInfo("Token removido do HttpClient");


                // Navega de volta para a página de login
                if (!NavService.NavigateFromControl(this, new LoginPage(_httpService, _tokenService, _serviceProvider)))
                {
                    LogService.LogError("Falha na navegação para a página de login após logout");
                }
            }
            catch (System.Exception ex)
            {
                LogService.LogError($"Erro durante logout: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Cleanup quando a página é removida
        ~LogPage()
        {
            LogService.LogAdded -= OnLogAdded;
        }
    }
}
