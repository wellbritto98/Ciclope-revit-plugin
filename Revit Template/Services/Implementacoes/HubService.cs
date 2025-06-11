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
using RevitTemplate.Infrastructure;
using RevitTemplate.Models;

namespace RevitTemplate.Services
{
    public class HubService : IDisposable
    {
        public HubConnection HubConnection { get; private set; }
        public IHubProxy HubProxy { get; private set; }
        private readonly WorkerServiceProxy _workerServiceProxy;
        private readonly IRevitDocumentService _revitDocumentService;
        private readonly SelectElementsEventHandler _selectElementsEventHandler;

        // Construtor sem parâmetros para DI
        public HubService(WorkerServiceProxy workerServiceProxy, IRevitDocumentService revitDocumentService, SelectElementsEventHandler selectElementsEventHandler)
        {
            _workerServiceProxy = workerServiceProxy;
            _revitDocumentService = revitDocumentService;
            _selectElementsEventHandler = selectElementsEventHandler;
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
                          FilterRevitElements filter = null;
                        
                        // Try to deserialize the filter if the message is not empty
                        if (!string.IsNullOrWhiteSpace(msg))
                        {
                            try
                            {
                                filter = JsonConvert.DeserializeObject<FilterRevitElements>(msg);
                            }
                            catch (JsonException ex)
                            {
                                LogService.LogError($"Erro ao deserializar filtro: {ex.Message}");
                                // Continue without filter if deserialization fails
                            }
                        }

                        List<ElementInfo> elements;
                        
                        // Use filtered or unfiltered method based on filter presence
                        if (filter != null && !string.IsNullOrWhiteSpace(filter.Campo) && !string.IsNullOrWhiteSpace(filter.Valor))
                        {
                            elements = await _revitDocumentService.GetElementInfoAsync(filter);
                            LogService.LogInfo($"Aplicado filtro: {filter.Campo} = {filter.Valor}");
                        }
                        else
                        {
                            elements = await _revitDocumentService.GetElementInfoAsync();
                            LogService.LogInfo("Nenhum filtro aplicado, retornando todos os elementos");
                        }



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

                _workerServiceProxy.On<string>("RevitVisualizarElemento", async (msg) =>
                {
                    LogService.LogInfo($"Solicitação de Visualizar Elemento");

                    try
                    {
                        // Check if UIApplication is available
                        if (UIApplicationProvider.Instance.UIApplication == null)
                        {
                            LogService.LogError("UIApplication não está disponível. Certifique-se de que a aplicação Revit está ativa.");
                            _workerServiceProxy.Invoke("RevitProjetoElementosResponse",
                                JsonConvert.SerializeObject(new { error = "UIApplication não disponível" }));
                            return;
                        }                        // Convert string element ID to integer
                                                 //deserializar msg em uma lista de strings

                        if (string.IsNullOrWhiteSpace(msg))
                        {
                            LogService.LogError("ID do elemento não pode ser nulo ou vazio.");
                            return;
                        }

                        List<string> strings = JsonConvert.DeserializeObject<List<string>>(msg);

                        if (strings == null || strings.Count == 0)
                        {
                            LogService.LogError("Lista de IDs de elementos está vazia ou inválida.");
                            return;
                        }

                        //converter cada string em int
                        List<int> elementIds = new List<int>();

                        foreach (var str in strings)
                        {
                            if (int.TryParse(str, out int elementId))
                            {
                                elementIds.Add(elementId);
                            }
                            else
                            {
                                LogService.LogError($"ID do elemento inválido: {str}");
                                return;
                            }
                        }

                        _selectElementsEventHandler.Raise(elementIds);

                        LogService.LogInfo($"Elemento {msg} Selecionado ");
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError($"Erro ao obter elementos do projeto: {ex.Message}");
                        _workerServiceProxy.Invoke("RevitProjetoElementosResponse",
                            JsonConvert.SerializeObject(new { error = ex.Message }));                    }
                });

                _workerServiceProxy.On<string>("GetCategoryNames", async (msg) =>
                {
                    LogService.LogInfo($"Solicitação dos nomes das categorias recebida.");

                    try
                    {
                        // Check if UIApplication is available
                        if (UIApplicationProvider.Instance.UIApplication == null)
                        {
                            LogService.LogError("UIApplication não está disponível. Certifique-se de que a aplicação Revit está ativa.");
                            _workerServiceProxy.Invoke("GetCategoryNamesResponse",
                                JsonConvert.SerializeObject(new { error = "UIApplication não disponível" }));
                            return;
                        }

                        var categoryNames = await _revitDocumentService.GetCategoryNamesAsync();

                        string categoryNamesJson = JsonConvert.SerializeObject(categoryNames);

                        _workerServiceProxy.Invoke("GetCategoryNamesResponse", categoryNamesJson);

                        LogService.LogInfo($"Enviados {categoryNames.Count} nomes de categorias para o SignalR hub.");
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError($"Erro ao obter nomes das categorias: {ex.Message}");
                        _workerServiceProxy.Invoke("GetCategoryNamesResponse",
                            JsonConvert.SerializeObject(new { error = ex.Message }));
                    }
                });                _workerServiceProxy.On<string>("GetFamilyNames", async (msg) =>
                {
                    LogService.LogInfo($"Solicitação dos nomes das famílias recebida.");

                    try
                    {
                        // Check if UIApplication is available
                        if (UIApplicationProvider.Instance.UIApplication == null)
                        {
                            LogService.LogError("UIApplication não está disponível. Certifique-se de que a aplicação Revit está ativa.");
                            _workerServiceProxy.Invoke("GetFamilyNamesResponse",
                                JsonConvert.SerializeObject(new { error = "UIApplication não disponível" }));
                            return;
                        }

                        List<string> familyNames;

                        // Check if a category filter is provided
                        if (!string.IsNullOrWhiteSpace(msg) || !string.Equals(msg, "FamilyNames"))
                        {
                            // Filter by category
                            familyNames = await _revitDocumentService.GetFamilyNamesByCategoryAsync(msg);
                            LogService.LogInfo($"Aplicado filtro de categoria: {msg}. Encontradas {familyNames.Count} famílias.");
                        }
                        else
                        {
                            // Get all family names
                            familyNames = await _revitDocumentService.GetFamilyNamesAsync();
                            LogService.LogInfo("Nenhum filtro de categoria aplicado, retornando todas as famílias.");
                        }

                        string familyNamesJson = JsonConvert.SerializeObject(familyNames);

                        _workerServiceProxy.Invoke("GetFamilyNamesResponse", familyNamesJson);

                        LogService.LogInfo($"Enviados {familyNames.Count} nomes de famílias para o SignalR hub.");
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError($"Erro ao obter nomes das famílias: {ex.Message}");
                        _workerServiceProxy.Invoke("GetFamilyNamesResponse",
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
        /// Exemplo de como obter apenas elementos específicos
        /// </summary>
        /// <returns>Lista de elementos específicos</returns>


        public void Dispose()
        {
            ((IDisposable)_workerServiceProxy).Dispose();
        }
    }
}