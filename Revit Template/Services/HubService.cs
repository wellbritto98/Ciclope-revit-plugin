using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using RevitTemplate.Utils;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using RevitTemplate.Core.Services;
using Newtonsoft.Json;

namespace RevitTemplate.Services
{
    public class HubService : IDisposable
    {
        public HubConnection HubConnection { get; private set; }
        public IHubProxy HubProxy { get; private set; }
        private readonly WorkerServiceProxy _workerServiceProxy;
        private readonly IRevitDocumentService _revitDocumentService;

        // Construtor sem parâmetros para DI
        public HubService(WorkerServiceProxy workerServiceProxy, IRevitDocumentService revitDocumentService )
        {
            _workerServiceProxy = workerServiceProxy;
            _revitDocumentService = revitDocumentService;
        }

        // Método para inicializar a conexão
        public void Initialize(string hubUrl, string token)
        {
            try
            {
                _workerServiceProxy.On<string>("RevitProjetoElementos", (msg) =>
                {
                    LogService.LogInfo($"Solicitação dos Elementos do projeto recebida.");
                    var elements = _revitDocumentService.GetElementInfoAsync();
                    //serialize in a json
                    string elementsJson = JsonConvert.SerializeObject(elements);

                    _workerServiceProxy.Invoke("RevitProjetoElementosResponse", elementsJson);

                });
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao criar conexão do hub: {ex.Message}");
                throw;
            }
        }

        public async Task ConectarAoHubAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return;

                _workerServiceProxy.Start(token);

                return;
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao conectar ao hub: {ex.Message}");
                return;
            }
        }

        public async Task EnviarMensagemAsync(string mensagem)
        {
            try
            {
                _workerServiceProxy.Invoke("RevitProjetoElementosResponse", mensagem);
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao enviar mensagem: {ex.Message}");
            }
        }

        public async void Invoke(string method, string message)
        {

            _workerServiceProxy.Invoke(method, message);

        }

        

        public void Dispose()
        {
            ((IDisposable)_workerServiceProxy).Dispose();
        }
    }
}