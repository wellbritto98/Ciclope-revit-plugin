using System.Windows.Controls;
using System.Windows;
using System.Net.Http;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RevitTemplate.Models;
using RevitTemplate.Services;
using Wpf.Ui.Controls; // Necessário para PasswordBox e Button do Wpf.Ui
using System.ComponentModel;

namespace RevitTemplate.UI.Views.Pages
{
    /// <summary>
    /// Página de login com email e senha.
    /// </summary>
    public partial class LoginPage : Page, INotifyPropertyChanged
    {
        private bool _isLoading = false;
        
        public LoginModel LoginModel { get; set; } = new LoginModel();
        public string ErrorMessage { get; set; }
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
        
        public bool IsNotLoading => !IsLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        public LoginPage()
        {
            InitializeComponent();
            DataContext = this;

        }


        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Ativa o estado de loading
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                // Sincroniza a senha do PasswordBox com o modelo
                LoginModel.Senha = PasswordBox.Password;
                var tokenData = await OnLoginSubmit();

                if (!tokenData.Authenticated)
                {
                    ErrorMessage = tokenData.Message;
                    // Log do erro de login
                    LogService.LogError($"Falha no login para o usuário: {LoginModel.Email}. Erro: {tokenData.Message}");
                }                else
                {
                    ErrorMessage = string.Empty;
                    
                    // Salva o token para uso futuro
                    bool tokenSaved = TokenService.SaveToken(tokenData);
                    if (tokenSaved)
                    {
                        LogService.LogInfo("Token salvo com sucesso para sessões futuras");
                    }
                    else
                    {
                        LogService.LogWarning("Falha ao salvar token - login funcionará apenas nesta sessão");
                    }
                    
                    // Configura o token no HttpClient para requisições autenticadas
                    if (RevitApp.Instance != null && !string.IsNullOrEmpty(tokenData.Token))
                    {
                        RevitApp.Instance.SetAuthorizationToken(tokenData.Token);
                        LogService.LogInfo("Token configurado no HttpClient para requisições autenticadas");
                    }
                    
                    // Log do sucesso do login
                    LogService.LogInfo($"Login realizado com sucesso para o usuário: {LoginModel.Email}");
                    
                    // Navega para a LogPage após login bem-sucedido usando o NavigationService
                    if (!NavService.NavigateFromControl(this, new LogPage()))
                    {
                        LogService.LogError("Falha na navegação para a página de logs");
                    }
                }
            }
            finally
            {
                // Desativa o estado de loading sempre, independente do resultado
                IsLoading = false;
                
                // Força atualização do DataContext para refletir mudanças nas propriedades
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public async Task<TokenData> OnLoginSubmit()
        {
            string errorMessage = "Ocorreu um erro inesperado.";
            try
            {
                HttpResponseMessage response;
                try
                {
                    var json = JsonConvert.SerializeObject(LoginModel);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await RevitApp.Instance.HttpClient.PostAsync("Autoriza/Login", content);
                }
                catch (HttpRequestException)
                {
                    return new TokenData
                    {
                        Authenticated = false,
                        Expiration = DateTime.Now,
                        Message = "Erro de conexão com o servidor.",
                        Token = null
                    };
                }
                catch (Exception)
                {
                    return new TokenData
                    {
                        Authenticated = false,
                        Expiration = DateTime.Now,
                        Message = errorMessage,
                        Token = null
                    };
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<TokenData>(responseString);
                    return tokenData;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                    errorMessage = "O servidor demorou muito para responder.";
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    errorMessage = "Credenciais inválidas.";
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    errorMessage = "Requisição inválida.";

                return new TokenData
                {
                    Authenticated = false,
                    Expiration = DateTime.Now,
                    Message = errorMessage,
                    Token = null
                };
            }
            catch (Exception)
            {
                return new TokenData
                {
                    Authenticated = false,
                    Expiration = DateTime.Now,
                    Message = errorMessage,
                    Token = null
                };
            }
        }
    }
}
