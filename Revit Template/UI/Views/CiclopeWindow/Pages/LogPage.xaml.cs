using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using RevitTemplate.Services;
using System.Net.WebSockets;
using Microsoft.Extensions.Options;


namespace RevitTemplate.UI.Views.Pages
{
    /// <summary>
    /// Página de console de logs
    /// </summary>
    public partial class LogPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<LogEntry> _logs;
        private int _logCount;
        private HubConnection _hubConnection;

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

        public int LogCount
        {
            get => _logCount;
            set
            {
                _logCount = value;
                OnPropertyChanged(nameof(LogCount));
            }
        }

        public LogPage()
        {
            InitializeComponent();
            DataContext = this;

            // Conecta aos logs do serviço
            Logs = LogService.Logs;

            // Inscreve-se no evento de novos logs para auto-scroll
            LogService.LogAdded += OnLogAdded;
            // Emite logs de teste ao entrar na página
            LogService.LogInfo("Autenticado com Sucesso.");

            // Exibe informações sobre o token atual
            string tokenInfo = TokenService.GetTokenInfo();
            LogService.LogInfo($"Token Status: {tokenInfo}");

            LogService.LogDebug("LogPage inicializada.");

            this.Loaded += LogPage_Loaded;


            UpdateLogCount();
        }

        private void LogPage_Loaded(object sender, RoutedEventArgs e)
        {
            var tokenData = TokenService.GetSavedToken();
            var token = tokenData.Token;
            //conectar no hub
            _ = ConectarAoHubAsync(token);

        }

        private async Task ConectarAoHubAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return;

                _hubConnection = CriarHubConnection(token);

                _hubConnection.On<string>("RevitProjetoElementos", async (msg) => LogService.LogInfo($"Solicitação dos Elementos do projeto recebida."));
                await _hubConnection.StartAsync();

                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    LogService.LogInfo($"Plugin revit conectado ao servidor CICLOPE");
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao conectar ao hub: {ex.Message}");
            }
        }

        private HubConnection CriarHubConnection(string token)
        {
            try
            {
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                return new HubConnectionBuilder()
                    .WithUrl("https://localhost:6102/revitHub", options =>
                    {
                        options.HttpMessageHandlerFactory = _ => httpClientHandler;
                        options.Headers.Add("Authorization", $"Bearer {token}");
                    })
                    .WithAutomaticReconnect()
                    .Build();
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao criar conexão do hub: {ex.Message}");
                throw;
            }
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
            {
                // Remove o token salvo
                bool tokenCleared = TokenService.ClearToken();

                if (tokenCleared)
                {
                    LogService.LogInfo("Logout realizado com sucesso - token removido");
                }
                else
                {
                    LogService.LogWarning("Logout realizado, mas houve problema ao remover o token");
                }

                // Remove o token do HttpClient também
                if (RevitApp.Instance != null)
                {
                    RevitApp.Instance.ClearAuthorizationToken();
                    LogService.LogInfo("Token removido do HttpClient");
                }

                // Navega de volta para a página de login
                if (!NavService.NavigateFromControl(this, new LoginPage()))
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
