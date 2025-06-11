using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RevitTemplate.Services
{
    public class HttpService
    {
        public HttpClient HttpClient { get; private set; } = 
            new HttpClient
                (
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    }
                )
                {
                    BaseAddress = new Uri("https://localhost:6102/api/")
                };

        private readonly IServiceProvider _serviceProvider;

        public HttpService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            ConfigureHttpClientWithToken();
        }


        public void ConfigureHttpClientWithToken()
        {

            try
            {
                var tokenService = _serviceProvider.GetRequiredService<TokenService>();
                var tokenData = tokenService.GetSavedToken();
                if (tokenData != null && tokenData.Authenticated && tokenData.Expiration > DateTime.Now)
                {
                    SetAuthorizationToken(tokenData.Token);
                    LogService.LogInfo("HttpClient configurado com token JWT existente");
                }
                else
                {
                    LogService.LogInfo("Nenhum token JWT válido encontrado para configurar o HttpClient");
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao configurar token no HttpClient: {ex.Message}");
            }
        }

        /// <summary>
        /// Define o token de autorização no HttpClient
        /// </summary>
        /// <param name="token">Token JWT para autorização</param>
        public void SetAuthorizationToken(string token)
        {
            try
            {
                if (HttpClient != null && !string.IsNullOrEmpty(token))
                {
                    HttpClient.DefaultRequestHeaders.Remove("Authorization");
                    HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    LogService.LogInfo("Token de autorização configurado no HttpClient");
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao definir token de autorização: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove o token de autorização do HttpClient
        /// </summary>
        public void ClearAuthorizationToken()
        {
            try
            {
                if (HttpClient != null)
                {
                    HttpClient.DefaultRequestHeaders.Remove("Authorization");
                    LogService.LogInfo("Token de autorização removido do HttpClient");
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao remover token de autorização: {ex.Message}");
            }
        }
    }
}
