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
        }        // Método para inicializar a conexão
        public void Initialize(string hubUrl, string token)
        {
            try
            {
                _workerServiceProxy.On<string>("RevitProjetoElementos", async (msg) =>
                {
                    LogService.LogInfo($"Solicitação dos Elementos do projeto recebida.");
                    
                    try
                    {
                        // Check if UIApplication is available
                        if (UIApplicationProvider.Instance.UIApplication == null)
                        {
                            LogService.LogError("UIApplication não está disponível. Certifique-se de que a aplicação Revit está ativa.");
                            _workerServiceProxy.Invoke("RevitProjetoElementosResponse", 
                                JsonConvert.SerializeObject(new { error = "UIApplication não disponível" }));
                            return;
                        }

                        var elements = await _revitDocumentService.GetElementInfoAsync();
                        
                        // Serialize elements to JSON
                        string elementsJson = JsonConvert.SerializeObject(elements);

                        _workerServiceProxy.Invoke("RevitProjetoElementosResponse", elementsJson);
                        
                        LogService.LogInfo($"Enviados {elements.Count} elementos para o SignalR hub.");
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError($"Erro ao obter elementos do projeto: {ex.Message}");
                        _workerServiceProxy.Invoke("RevitProjetoElementosResponse", 
                            JsonConvert.SerializeObject(new { error = ex.Message }));
                    }
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

        /// <summary>
        /// Exemplo de como obter informações do documento Revit
        /// </summary>
        /// <returns>Informações do documento em formato JSON</returns>
        public async Task<string> GetDocumentInfoAsync()
        {
            try
            {
                // Check if UIApplication is available
                if (UIApplicationProvider.Instance.UIApplication == null)
                {
                    throw new InvalidOperationException("UIApplication não está disponível. Execute o comando a partir do Revit.");
                }

                // Get basic document info
                var docInfo = _revitDocumentService.GetDocumentInfo();
                
                // Get elements info
                var elements = await _revitDocumentService.GetElementInfoAsync();
                
                // Get family instances
                var familyInstances = _revitDocumentService.GetAllFamilyInstances();
                
                // Get sheets
                var sheets = await _revitDocumentService.GetSheetsAsync();
                
                // Get wall info
                var wallInfo = await _revitDocumentService.GetWallInfoAsync();

                var result = new
                {
                    DocumentInfo = docInfo,
                    ElementsCount = elements.Count,
                    Elements = elements.Take(5), // Apenas os primeiros 5 para exemplo
                    FamilyInstancesCount = familyInstances.Count,
                    SheetsCount = sheets.Count,
                    WallInfo = wallInfo
                };

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao obter informações do documento: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exemplo de como obter apenas elementos específicos
        /// </summary>
        /// <returns>Lista de elementos específicos</returns>
        public async Task<string> GetSpecificElementsAsync()
        {
            try
            {
                // Check if UIApplication is available
                if (UIApplicationProvider.Instance.UIApplication == null)
                {
                    throw new InvalidOperationException("UIApplication não está disponível.");
                }

                var elements = await _revitDocumentService.GetElementInfoAsync();
                
                // Filtrar apenas elementos de parede (exemplo)
                var wallElements = elements.Where(e => e.Category != null && 
                    e.Category.ToLower().Contains("wall")).ToList();

                LogService.LogInfo($"Encontrados {wallElements.Count} elementos de parede.");

                return JsonConvert.SerializeObject(wallElements, Formatting.Indented);
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro ao obter elementos específicos: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            ((IDisposable)_workerServiceProxy).Dispose();
        }
    }
}