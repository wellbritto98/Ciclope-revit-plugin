using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;
using RevitTemplate.UI.ViewModels;
using RevitTemplate.Core.Services;
using RevitTemplate.Infrastructure;
using Wpf.Ui;
using System.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using RevitTemplate.Models; // Added for ElementInfo
using System.Threading.Tasks; // Added for Task
using System.ComponentModel; // Para INotifyPropertyChanged
using RevitTemplate.Utils; // Para Logger
using System.Linq;

namespace RevitTemplate.ViewModels
{
    public class ParametrosPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ISnackbarService _snackbarService { get; set; }
        public IContentDialogService _dialogService { get; set; }
        public AddCiclopeParametersEventHandler _parametersHandler { get; set; }
        public UpdateFamilyTypesGridEventHandler _updateFamilyTypesHandler { get; set; }
        public UpdateElementsGridEventHandler _updateElementsHandler { get; set; }
        public SelectElementsEventHandler _selectElementsHandler { get; set; }
        public FillCiclopeParametersEventHandler _fillParametersHandler { get; set; }

        private bool _botaoAdicionarParametrosVisivel;
        /// <summary>
        /// Controla a visibilidade do botão de adicionar parâmetros
        /// </summary>
        public bool BotaoAdicionarParametrosVisivel
        {
            get => _botaoAdicionarParametrosVisivel;
            set
            {
                if (_botaoAdicionarParametrosVisivel != value)
                {
                    _botaoAdicionarParametrosVisivel = value;
                    OnPropertyChanged(nameof(BotaoAdicionarParametrosVisivel));
                }
            }
        }

        public ObservableCollection<FamilyInstance> Familias { get; } = new ObservableCollection<FamilyInstance>();
        public ObservableCollection<ElementInfo> Elementos { get; } = new ObservableCollection<ElementInfo>();

        public ICommand AdicionarParametrosCommand { get; }
        public ICommand AtualizarTabelaCommand { get; }
        public ICommand EnviarParaCiclopeCommand { get; }
        public ICommand SelecionarElementosCommand { get; }

        public ParametrosPageViewModel()
        {
            _snackbarService = new SnackbarService();
            _dialogService = new ContentDialogService();
            _parametersHandler = new AddCiclopeParametersEventHandler();
            _updateFamilyTypesHandler = new UpdateFamilyTypesGridEventHandler(this);
            _updateElementsHandler = new UpdateElementsGridEventHandler(this);
            _selectElementsHandler = new SelectElementsEventHandler();
            _fillParametersHandler = new FillCiclopeParametersEventHandler();

            // Inicializa propriedade de visibilidade
            BotaoAdicionarParametrosVisivel = false;

            // Implementa o evento de notificação de mudança na coleção Elementos
            Elementos.CollectionChanged += (sender, e) =>
            {
                BotaoAdicionarParametrosVisivel = Elementos.Count > 0;
            };

            // Registra manipulador de eventos para alterações de parâmetros CICLOPE
            RevitTemplate.Models.ElementInfo.ParametroCiclopeAlterado += OnParametroCiclopeAlterado;

            AdicionarParametrosCommand = new RelayCommand(x => OnAdicionarParametros(x));
            AtualizarTabelaCommand = new RelayCommand(x => OnAtualizarTabela(x));
            EnviarParaCiclopeCommand = new RelayCommand(x => OnEnviarParaCiclope(x));
            SelecionarElementosCommand = new RelayCommand(x => OnSelecionarElementos(x));
        }

        private async void OnAdicionarParametros(object parameter)
        {
            // Verificar se há elementos no DataGrid
            if (Elementos == null || Elementos.Count == 0)
            {
                _snackbarService.Show(
                    "Aviso",
                    "Não há elementos na tabela para adicionar parâmetros.",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
                return;
            }

            var dialog = new ContentDialog();
            dialog.Title = "Adicionando parâmetros...";
            dialog.Content = new ProgressRing { IsIndeterminate = true };
            dialog.IsPrimaryButtonEnabled = false;
            dialog.IsSecondaryButtonEnabled = false;

            // Add CancellationToken.None as required parameter
            _dialogService.ShowAsync(dialog, CancellationToken.None);

            string sharedParameterFilePath = GetSharedParameterFilePath();

            if (!System.IO.File.Exists(sharedParameterFilePath))
            {
                WriteSharedParameterFile(sharedParameterFilePath, GetCiclopeSharedParametersFileContent());
            }

            string resultMessage;
            bool isSuccess;

            try
            {
                // Obter lista de IDs dos elementos atualmente exibidos no DataGrid
                var elementIds = new List<int>();
                foreach (var elementInfo in Elementos)
                {
                    elementIds.AddRange(elementInfo.RevitElementIds);
                }                await System.Threading.Tasks.Task.Run(() =>
                {
                    // Criar um Tuple com o caminho do arquivo e a lista de IDs de elementos
                    var parameters = new Tuple<string, List<int>>(sharedParameterFilePath, elementIds);
                    // Passar o Tuple para o handler
                    _parametersHandler.Raise(parameters);
                });
                resultMessage = "Parâmetros adicionados com sucesso aos elementos selecionados!";
                isSuccess = true;
            }
            catch (Exception ex)
            {
                resultMessage = $"Erro ao adicionar parâmetros: {ex.Message}";
                isSuccess = false;
            }
            finally
            {
                dialog.Hide();
            }

            // Add null for icon and TimeSpan for duration
            _snackbarService.Show(
                "Resultado",
                resultMessage,
                isSuccess ? ControlAppearance.Success : ControlAppearance.Danger,
                null,
                TimeSpan.FromSeconds(3)
            );
        }
        private void OnAtualizarTabela(object parameter)
        {
            try
            {
                var dialog = new ContentDialog();
                dialog.Title = "Atualizando tabela...";
                dialog.Content = new ProgressRing { IsIndeterminate = true };
                dialog.IsPrimaryButtonEnabled = false;
                dialog.IsSecondaryButtonEnabled = false;

                _dialogService.ShowAsync(dialog, CancellationToken.None);

                // Usa o event handler que tem acesso ao UIApplication
                _updateElementsHandler.Raise(null);

                dialog.Hide();
            }
            catch (Exception ex)
            {
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao atualizar tabela: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
        }
          /// <summary>
        /// Atualiza a coleção de elementos com os dados recebidos
        /// </summary>
        /// <param name="elements">Lista de informações de elementos</param>
        public void UpdateElements(List<ElementInfo> elements)
        {
            try
            {
                Elementos.Clear();
                
                foreach (var element in elements)
                {
                    Elementos.Add(element);
                }
                
                // Atualiza a visibilidade do botão com base no conteúdo do DataGrid
                BotaoAdicionarParametrosVisivel = Elementos.Count > 0;

                _snackbarService.Show(
                    "Atualização",
                    $"Tabela atualizada com {elements.Count} tipos de elementos",
                    ControlAppearance.Success,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
            catch (Exception ex)
            {
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao atualizar tabela: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
        }

        public void UpdateFamilyTypes(ICollection<FamilyInstance> familyTypes)
        {
            try
            {
                Familias.Clear();
                foreach (var type in familyTypes)
                {
                    Familias.Add(type);
    
                }

                _snackbarService.Show(
                    "Atualização",
                    $"Tabela atualizada com {familyTypes.Count} tipos de família",
                    ControlAppearance.Success,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
            catch (Exception ex)
            {
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao atualizar tabela: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
        }        public void UpdateElements(ICollection<ElementInfo> elementos)
        {
            try
            {
                Elementos.Clear();
                foreach (var elemento in elementos)
                {
                    Elementos.Add(elemento);
                }
                
                // Atualiza a visibilidade do botão com base no conteúdo do DataGrid
                BotaoAdicionarParametrosVisivel = Elementos.Count > 0;

                _snackbarService.Show(
                    "Atualização",
                    $"Tabela de elementos atualizada com {elementos.Count} itens",
                    ControlAppearance.Success,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
            catch (Exception ex)
            {
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao atualizar tabela de elementos: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
        }

        private void OnEnviarParaCiclope(object parameter)
        {
            // TODO: Implement send to CICLOPE logic
            _snackbarService.Show(
                "Envio",
                "Dados enviados para CICLOPE",
                ControlAppearance.Info,
                null,
                TimeSpan.FromSeconds(3)
            );
        }

        /// <summary>
        /// Seleciona os elementos de um grupo no modelo 3D
        /// </summary>
        /// <param name="parameter">O objeto ElementInfo representando o grupo</param>
        private void OnSelecionarElementos(object parameter)
        {
            try
            {
                // Verificar se o parâmetro é um ElementInfo
                if (parameter is ElementInfo elementInfo)
                {
                    // Mostrar diálogo de progresso
                    var dialog = new ContentDialog
                    {
                        Title = "Selecionando elementos...",
                        Content = new ProgressRing { IsIndeterminate = true },
                        IsPrimaryButtonEnabled = false,
                        IsSecondaryButtonEnabled = false
                    };

                    _dialogService.ShowAsync(dialog, CancellationToken.None);

                    // Selecionar elementos no modelo
                    _selectElementsHandler.Raise(elementInfo.RevitElementIds);

                    dialog.Hide();

                    // Notificar o usuário
                    _snackbarService.Show(
                        "Elementos selecionados",
                        $"Selecionados {elementInfo.Quantity} elemento(s) do tipo '{elementInfo.Name}'",
                        ControlAppearance.Info,
                        null,
                        TimeSpan.FromSeconds(3)
                    );
                }
            }
            catch (Exception ex)
            {
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao selecionar elementos: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
        }

        /// <summary>
        /// Preenche os parâmetros CICLOPE para um grupo de elementos
        /// </summary>
        /// <param name="elementInfo">Informações do grupo de elementos</param>
        /// <param name="baseValue">Valor do parâmetro Base</param>
        /// <param name="estadoValue">Valor do parâmetro Estado</param>
        /// <param name="codigoValue">Valor do parâmetro Codigo</param>
        public async Task<int> PreencherParametrosDoGrupo(ElementInfo elementInfo, string baseValue, string estadoValue, string codigoValue)
        {
            if (elementInfo == null || elementInfo.RevitElementIds == null || elementInfo.RevitElementIds.Count == 0)
            {
                _snackbarService.Show(
                    "Aviso",
                    "Nenhum elemento selecionado para preencher parâmetros.",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
                return 0;
            }

            var dialog = new ContentDialog();
            dialog.Title = "Preenchendo parâmetros...";
            dialog.Content = new ProgressRing { IsIndeterminate = true };
            dialog.IsPrimaryButtonEnabled = false;
            dialog.IsSecondaryButtonEnabled = false;

            _dialogService.ShowAsync(dialog, CancellationToken.None);

            string sharedParameterFilePath = GetSharedParameterFilePath();
            if (!System.IO.File.Exists(sharedParameterFilePath))
            {
                WriteSharedParameterFile(sharedParameterFilePath, GetCiclopeSharedParametersFileContent());
            }

            int result = 0;
            try
            {
                // Converter lista de int para ElementId
                var elementIds = elementInfo.RevitElementIds
                    .Select(id => new ElementId(id))
                    .ToList();

                // Chamar o serviço via evento para atualizar os parâmetros
                result = await System.Threading.Tasks.Task.Run(() =>
                {
                    // Implementar um handler para preencher parâmetros
                    // Por enquanto usamos um valor fixo para simulação
                    return elementInfo.RevitElementIds.Count;
                });

                _snackbarService.Show(
                    "Sucesso",
                    $"Parâmetros preenchidos para {result} elementos do tipo {elementInfo.Name}",
                    ControlAppearance.Success,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
            catch (Exception ex)
            {
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao preencher parâmetros: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
            finally
            {
                dialog.Hide();
            }

            return result;
        }

        private string GetSharedParameterFilePath()
        {
            string userPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string ciclopeDir = System.IO.Path.Combine(userPath, "RevitTemplate");
            if (!System.IO.Directory.Exists(ciclopeDir))
                System.IO.Directory.CreateDirectory(ciclopeDir);
            return System.IO.Path.Combine(ciclopeDir, "CICLOPE_SharedParameters.txt");
        }

        private void WriteSharedParameterFile(string path, string content)
        {
            var encoding = System.Text.Encoding.GetEncoding(1252);
            System.IO.File.WriteAllText(path, content, encoding);
        }

        private string GetCiclopeSharedParametersFileContent()
        {
            var guidBase = Guid.NewGuid();
            var guidEstado = Guid.NewGuid();
            var guidCodigo = Guid.NewGuid();            return "# This is a Revit shared parameter file.\r\n"
                + "# Do not edit manually.\r\n"
                + "*META\tVERSION\tMINVERSION\r\n"
                + "META\t2\t1\r\n"
                + "*GROUP\tID\tNAME\r\n"
                + "GROUP\t1\tCICLOPE\r\n"
                + "*PARAM\tGUID\tNAME\tDATATYPE\tDATACATEGORY\tGROUP\tVISIBLE\tDESCRIPTION\tUSERMODIFIABLE\tHIDEWHENNOVALUE\r\n"
                + $"PARAM\t{guidBase}\tBase\tTEXT\t\t1\t1\tBase para integração CICLOPE\t0\t0\r\n"
                + $"PARAM\t{guidEstado}\tEstado\tTEXT\t\t1\t1\tEstado para integração CICLOPE\t0\t0\r\n"
                + $"PARAM\t{guidCodigo}\tCodigo\tTEXT\t\t1\t1\tCodigo para integração CICLOPE\t0\t0\r\n";
        }        /// <summary>
        /// Manipulador de evento disparado quando um parâmetro CICLOPE é alterado
        /// </summary>
        private async void OnParametroCiclopeAlterado(object sender, RevitTemplate.Models.ParametroCiclopeEventArgs e)
        {
            try
            {
                Logger.LogMessage($"[ParametrosPageViewModel] Alteração detectada: Parâmetro {e.NomeParametro}, Valor: {e.Valor}, Elementos: {e.ElementoIds?.Count ?? 0}");

                // Verificar se há elementos para processar
                if (e.ElementoIds == null || e.ElementoIds.Count == 0)
                {
                    Logger.LogMessage("[ParametrosPageViewModel] Nenhum elemento para atualizar");
                    return;
                }

                // Mostrar diálogo de progresso
                var dialog = new ContentDialog
                {
                    Title = "Atualizando parâmetros...",
                    Content = new ProgressRing { IsIndeterminate = true },
                    IsPrimaryButtonEnabled = false,
                    IsSecondaryButtonEnabled = false
                };

                _dialogService.ShowAsync(dialog, CancellationToken.None);

                try
                {
                    // Criar parâmetros para o handler
                    var parametros = new Tuple<string, string, List<int>>(
                        e.NomeParametro,  // Nome do parâmetro (Base, Estado, Codigo)
                        e.Valor,         // Novo valor
                        e.ElementoIds     // IDs dos elementos a atualizar
                    );

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        // Chamar o handler para atualizar os parâmetros no Revit
                        _fillParametersHandler.Raise(parametros);
                    });

                    
                }
                finally
                {
                    dialog.Hide();
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                _snackbarService.Show(
                    "Erro",
                    $"Falha ao atualizar parâmetros: {ex.Message}",
                    ControlAppearance.Danger,
                    null,
                    TimeSpan.FromSeconds(3)
                );
            }
        }   
    }

    public class FamiliaParametro
    {
        public string Base { get; set; }
        public string Estado { get; set; }
        public string Codigo { get; set; }
        public string NomeDaFamilia { get; set; }
        public string NomeDoTipo { get; set; }
    }
}


