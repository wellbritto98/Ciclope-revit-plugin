# Guia Passo a Passo: Como Adicionar uma Nova Funcionalidade

Este guia explica como adicionar uma nova funcionalidade à arquitetura do plugin Revit que criamos. Vamos usar como exemplo a adição de uma funcionalidade para extrair e exibir informações sobre elementos de um modelo Revit.

## 1. Definir o Modelo de Dados

**Passo 1:** Crie uma classe de modelo na pasta `Core/Models`

```csharp
// Revit Template/Core/Models/ElementInfo.cs
using System;

namespace RevitTemplate.Core.Models
{
    /// <summary>
    /// Representa informações sobre um elemento do Revit
    /// </summary>
    public class ElementInfo
    {
        /// <summary>
        /// Obtém ou define o ID do elemento
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Obtém ou define o nome do elemento
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Obtém ou define a categoria do elemento
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Obtém ou define o nível do elemento
        /// </summary>
        public string Level { get; set; }
        
        /// <summary>
        /// Obtém ou define parâmetros adicionais do elemento
        /// </summary>
        public string Parameters { get; set; }
    }
}
```

## 2. Criar a Interface de Serviço

**Passo 2:** Defina uma interface de serviço na pasta `Core/Services`

```csharp
// Revit Template/Core/Services/IElementService.cs
using Autodesk.Revit.DB;
using RevitTemplate.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Interface para operações relacionadas a elementos do Revit
    /// </summary>
    public interface IElementService
    {
        /// <summary>
        /// Obtém informações sobre todos os elementos de uma categoria específica
        /// </summary>
        /// <param name="categoryName">Nome da categoria</param>
        /// <returns>Lista de informações sobre elementos</returns>
        Task<List<ElementInfo>> GetElementsByCategoryAsync(string categoryName);
        
        /// <summary>
        /// Obtém informações sobre um elemento específico
        /// </summary>
        /// <param name="elementId">ID do elemento</param>
        /// <returns>Informações sobre o elemento</returns>
        ElementInfo GetElementById(ElementId elementId);
        
        /// <summary>
        /// Obtém uma lista de todas as categorias disponíveis no documento
        /// </summary>
        /// <returns>Lista de nomes de categorias</returns>
        List<string> GetAvailableCategories();
    }
}
```

## 3. Implementar o Serviço

**Passo 3:** Implemente a interface de serviço

```csharp
// Revit Template/Core/Services/ElementService.cs
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.Core.Models;
using RevitTemplate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Implementação do serviço de elementos
    /// </summary>
    public class ElementService : IElementService
    {
        private readonly UIApplication _uiApplication;
        
        /// <summary>
        /// Inicializa uma nova instância da classe ElementService
        /// </summary>
        /// <param name="uiApplication">Aplicação UI do Revit</param>
        public ElementService(UIApplication uiApplication)
        {
            _uiApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
        }
        
        /// <summary>
        /// Obtém o documento atual do Revit
        /// </summary>
        /// <returns>O documento atual</returns>
        private Document GetCurrentDocument()
        {
            return _uiApplication.ActiveUIDocument.Document;
        }
        
        /// <inheritdoc/>
        public async Task<List<ElementInfo>> GetElementsByCategoryAsync(string categoryName)
        {
            Document doc = GetCurrentDocument();
            
            return await Task.Run(() =>
            {
                Logger.LogThreadInfo($"Getting elements for category: {categoryName}");
                
                try
                {
                    // Encontrar a categoria pelo nome
                    BuiltInCategory builtInCategory = GetBuiltInCategoryByName(categoryName);
                    
                    // Coletar elementos da categoria
                    FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .OfCategory(builtInCategory)
                        .WhereElementIsNotElementType();
                    
                    // Converter para ElementInfo
                    return collector
                        .Select(element => new ElementInfo
                        {
                            Id = element.Id.IntegerValue.ToString(),
                            Name = element.Name,
                            Category = element.Category?.Name ?? "Sem categoria",
                            Level = GetElementLevel(element),
                            Parameters = GetElementParameters(element)
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.HandleError(ex);
                    return new List<ElementInfo>();
                }
            });
        }
        
        /// <inheritdoc/>
        public ElementInfo GetElementById(ElementId elementId)
        {
            Document doc = GetCurrentDocument();
            Element element = doc.GetElement(elementId);
            
            if (element == null)
                return null;
                
            return new ElementInfo
            {
                Id = element.Id.IntegerValue.ToString(),
                Name = element.Name,
                Category = element.Category?.Name ?? "Sem categoria",
                Level = GetElementLevel(element),
                Parameters = GetElementParameters(element)
            };
        }
        
        /// <inheritdoc/>
        public List<string> GetAvailableCategories()
        {
            Document doc = GetCurrentDocument();
            
            // Obter todas as categorias do documento
            Categories categories = doc.Settings.Categories;
            
            // Converter para lista de strings
            return categories
                .Cast<Category>()
                .Select(c => c.Name)
                .OrderBy(name => name)
                .ToList();
        }
        
        /// <summary>
        /// Obtém o nível associado a um elemento
        /// </summary>
        private string GetElementLevel(Element element)
        {
            // Tentar obter o parâmetro de nível
            Parameter levelParam = element.LookupParameter("Level");
            
            if (levelParam != null && levelParam.HasValue)
            {
                return levelParam.AsValueString();
            }
            
            // Tentar obter o nível de outra forma para elementos hospedados
            if (element is HostObject hostObject)
            {
                ElementId levelId = hostObject.LevelId;
                if (levelId != null && levelId != ElementId.InvalidElementId)
                {
                    Element level = element.Document.GetElement(levelId);
                    return level?.Name ?? "Desconhecido";
                }
            }
            
            return "Sem nível";
        }
        
        /// <summary>
        /// Obtém uma string formatada com os parâmetros principais do elemento
        /// </summary>
        private string GetElementParameters(Element element)
        {
            // Obter alguns parâmetros comuns
            var parameters = new List<string>();
            
            // Adicionar alguns parâmetros comuns
            AddParameterIfExists(element, "Família", parameters);
            AddParameterIfExists(element, "Tipo", parameters);
            AddParameterIfExists(element, "Volume", parameters);
            AddParameterIfExists(element, "Área", parameters);
            
            return string.Join(", ", parameters);
        }
        
        /// <summary>
        /// Adiciona um parâmetro à lista se ele existir
        /// </summary>
        private void AddParameterIfExists(Element element, string paramName, List<string> parameters)
        {
            Parameter param = element.LookupParameter(paramName);
            if (param != null && param.HasValue)
            {
                parameters.Add($"{paramName}: {param.AsValueString()}");
            }
        }
        
        /// <summary>
        /// Converte um nome de categoria para BuiltInCategory
        /// </summary>
        private BuiltInCategory GetBuiltInCategoryByName(string categoryName)
        {
            // Mapeamento de alguns nomes comuns para BuiltInCategory
            switch (categoryName.ToLower())
            {
                case "paredes":
                case "walls":
                    return BuiltInCategory.OST_Walls;
                case "portas":
                case "doors":
                    return BuiltInCategory.OST_Doors;
                case "janelas":
                case "windows":
                    return BuiltInCategory.OST_Windows;
                case "pisos":
                case "floors":
                    return BuiltInCategory.OST_Floors;
                case "tetos":
                case "ceilings":
                    return BuiltInCategory.OST_Ceilings;
                case "mobiliário":
                case "furniture":
                    return BuiltInCategory.OST_Furniture;
                default:
                    return BuiltInCategory.OST_GenericModel;
            }
        }
    }
}
```

## 4. Criar o Event Handler

**Passo 4:** Crie um event handler para operações que precisam de um contexto válido da API do Revit

```csharp
// Revit Template/Infrastructure/ElementEventHandler.cs
using Autodesk.Revit.UI;
using RevitTemplate.Core.Services;
using RevitTemplate.UI.Views;
using RevitTemplate.Utils;
using System;
using System.Collections.Generic;
using RevitTemplate.Core.Models;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// Event handler para operações relacionadas a elementos
    /// </summary>
    public class ElementEventHandler : RevitEventHandler<ElementEventArgs>
    {
        private readonly IElementService _elementService;
        
        /// <summary>
        /// Inicializa uma nova instância da classe ElementEventHandler
        /// </summary>
        /// <param name="elementService">Serviço de elementos</param>
        public ElementEventHandler(IElementService elementService)
        {
            _elementService = elementService ?? throw new ArgumentNullException(nameof(elementService));
        }
        
        /// <summary>
        /// Executa o event handler com os argumentos especificados
        /// </summary>
        protected override void Execute(UIApplication app, ElementEventArgs args)
        {
            Logger.LogMessage("Element Event Handler executed");
            
            switch (args.Operation)
            {
                case ElementOperation.GetCategories:
                    HandleGetCategories(args.Window);
                    break;
                    
                case ElementOperation.GetElements:
                    HandleGetElements(args.Window, args.CategoryName);
                    break;
                    
                default:
                    Logger.LogMessage($"Unknown operation: {args.Operation}");
                    break;
            }
        }
        
        /// <summary>
        /// Manipula a operação de obter categorias
        /// </summary>
        private void HandleGetCategories(MainWindow window)
        {
            try
            {
                List<string> categories = _elementService.GetAvailableCategories();
                
                window.Dispatcher.Invoke(() => 
                {
                    window.AppendToLog($"Encontradas {categories.Count} categorias");
                    window.UpdateCategories(categories);
                });
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                window.Dispatcher.Invoke(() => 
                {
                    window.AppendToLog($"Erro ao obter categorias: {ex.Message}");
                });
            }
        }
        
        /// <summary>
        /// Manipula a operação de obter elementos
        /// </summary>
        private void HandleGetElements(MainWindow window, string categoryName)
        {
            try
            {
                var elementsTask = _elementService.GetElementsByCategoryAsync(categoryName);
                
                elementsTask.ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        List<ElementInfo> elements = task.Result;
                        
                        window.Dispatcher.Invoke(() => 
                        {
                            window.AppendToLog($"Encontrados {elements.Count} elementos na categoria {categoryName}");
                            window.UpdateElements(elements);
                        });
                    }
                    else if (task.IsFaulted && task.Exception != null)
                    {
                        Logger.HandleError(task.Exception);
                        window.Dispatcher.Invoke(() => 
                        {
                            window.AppendToLog($"Erro ao obter elementos: {task.Exception.Message}");
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                window.Dispatcher.Invoke(() => 
                {
                    window.AppendToLog($"Erro ao obter elementos: {ex.Message}");
                });
            }
        }
    }
    
    /// <summary>
    /// Argumentos para o ElementEventHandler
    /// </summary>
    public class ElementEventArgs
    {
        /// <summary>
        /// Obtém ou define a janela principal
        /// </summary>
        public MainWindow Window { get; set; }
        
        /// <summary>
        /// Obtém ou define a operação a ser executada
        /// </summary>
        public ElementOperation Operation { get; set; }
        
        /// <summary>
        /// Obtém ou define o nome da categoria (opcional)
        /// </summary>
        public string CategoryName { get; set; }
    }
    
    /// <summary>
    /// Operações disponíveis para elementos
    /// </summary>
    public enum ElementOperation
    {
        /// <summary>
        /// Obter todas as categorias
        /// </summary>
        GetCategories,
        
        /// <summary>
        /// Obter elementos de uma categoria
        /// </summary>
        GetElements
    }
}
```

## 5. Atualizar o ViewModel

**Passo 5:** Crie ou atualize o ViewModel para a nova funcionalidade

```csharp
// Revit Template/UI/ViewModels/ElementViewModel.cs
using RevitTemplate.Core.Models;
using RevitTemplate.Infrastructure;
using RevitTemplate.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RevitTemplate.UI.ViewModels
{
    /// <summary>
    /// ViewModel para a funcionalidade de elementos
    /// </summary>
    public class ElementViewModel : ViewModelBase
    {
        private readonly ElementEventHandler _elementEventHandler;
        
        private ObservableCollection<string> _categories;
        private ObservableCollection<ElementInfo> _elements;
        private string _selectedCategory;
        private ElementInfo _selectedElement;
        
        /// <summary>
        /// Inicializa uma nova instância da classe ElementViewModel
        /// </summary>
        /// <param name="elementEventHandler">Event handler para elementos</param>
        public ElementViewModel(ElementEventHandler elementEventHandler)
        {
            _elementEventHandler = elementEventHandler ?? throw new ArgumentNullException(nameof(elementEventHandler));
            
            // Inicializar coleções
            Categories = new ObservableCollection<string>();
            Elements = new ObservableCollection<ElementInfo>();
            
            // Inicializar comandos
            LoadCategoriesCommand = new RelayCommand(LoadCategoriesAction);
            LoadElementsCommand = new RelayCommand(LoadElementsAction, CanLoadElements);
        }
        
        /// <summary>
        /// Obtém ou define a coleção de categorias
        /// </summary>
        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }
        
        /// <summary>
        /// Obtém ou define a coleção de elementos
        /// </summary>
        public ObservableCollection<ElementInfo> Elements
        {
            get => _elements;
            set => SetProperty(ref _elements, value);
        }
        
        /// <summary>
        /// Obtém ou define a categoria selecionada
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    // Atualizar comandos quando a seleção mudar
                    (LoadElementsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
        
        /// <summary>
        /// Obtém ou define o elemento selecionado
        /// </summary>
        public ElementInfo SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }
        
        /// <summary>
        /// Obtém o comando para carregar categorias
        /// </summary>
        public ICommand LoadCategoriesCommand { get; }
        
        /// <summary>
        /// Obtém o comando para carregar elementos
        /// </summary>
        public ICommand LoadElementsCommand { get; }
        
        /// <summary>
        /// Atualiza a lista de categorias
        /// </summary>
        /// <param name="categories">Lista de categorias</param>
        public void UpdateCategories(List<string> categories)
        {
            Categories.Clear();
            
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        
        /// <summary>
        /// Atualiza a lista de elementos
        /// </summary>
        /// <param name="elements">Lista de elementos</param>
        public void UpdateElements(List<ElementInfo> elements)
        {
            Elements.Clear();
            
            foreach (var element in elements)
            {
                Elements.Add(element);
            }
        }
        
        /// <summary>
        /// Ação para carregar categorias
        /// </summary>
        private void LoadCategoriesAction(object parameter)
        {
            try
            {
                var window = parameter as Views.MainWindow;
                
                if (window != null)
                {
                    var args = new ElementEventArgs
                    {
                        Window = window,
                        Operation = ElementOperation.GetCategories
                    };
                    
                    _elementEventHandler.Raise(args);
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
            }
        }
        
        /// <summary>
        /// Ação para carregar elementos
        /// </summary>
        private void LoadElementsAction(object parameter)
        {
            try
            {
                var window = parameter as Views.MainWindow;
                
                if (window != null && !string.IsNullOrEmpty(SelectedCategory))
                {
                    var args = new ElementEventArgs
                    {
                        Window = window,
                        Operation = ElementOperation.GetElements,
                        CategoryName = SelectedCategory
                    };
                    
                    _elementEventHandler.Raise(args);
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
            }
        }
        
        /// <summary>
        /// Verifica se é possível carregar elementos
        /// </summary>
        private bool CanLoadElements(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedCategory);
        }
    }
}
```

## 6. Atualizar a Interface do Usuário

**Passo 6:** Adicione uma nova aba na interface do usuário para a funcionalidade

```xml
<!-- Adicione isso dentro do TabControl em UI/Views/MainWindow.xaml -->
<TabItem x:Name="TabElements" Padding="5,0,5,0">
    <TabItem.Header>
        <StackPanel Orientation="Horizontal">
            <Image Source="../../Resources/building.png" VerticalAlignment="Center" HorizontalAlignment="Center" Height="13" />
            <Label Content="Elementos" />
        </StackPanel>
    </TabItem.Header>
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>
        
        <!-- Seleção de categoria -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="0,10,0,10">
            <Label Content="Categoria:" VerticalAlignment="Center" />
            <ComboBox x:Name="CbCategories" Width="200" Margin="5,0,10,0" 
                      ItemsSource="{Binding Categories}" 
                      SelectedItem="{Binding SelectedCategory}" />
            <Button x:Name="BtnLoadCategories" Content="Carregar Categorias" Width="120" Margin="5,0,5,0"
                    Command="{Binding LoadCategoriesCommand}" 
                    CommandParameter="{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Window}}}" />
            <Button x:Name="BtnLoadElements" Content="Carregar Elementos" Width="120" Margin="5,0,5,0"
                    Command="{Binding LoadElementsCommand}" 
                    CommandParameter="{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Window}}}" />
        </StackPanel>
        
        <!-- Lista de elementos -->
        <DataGrid Grid.Row="2" x:Name="DgElements" Margin="0,10,0,0" 
                  ItemsSource="{Binding Elements}" 
                  SelectedItem="{Binding SelectedElement}"
                  AutoGenerateColumns="False" 
                  IsReadOnly="True"
                  SelectionMode="Single"
                  CanUserAddRows="False"
                  CanUserDeleteRows="False"
                  CanUserReorderColumns="True"
                  CanUserResizeColumns="True"
                  CanUserSortColumns="True">
            <DataGrid.Columns>
                <DataGridTextColumn Header="ID" Binding="{Binding Id}" Width="80" />
                <DataGridTextColumn Header="Nome" Binding="{Binding Name}" Width="150" />
                <DataGridTextColumn Header="Categoria" Binding="{Binding Category}" Width="120" />
                <DataGridTextColumn Header="Nível" Binding="{Binding Level}" Width="100" />
                <DataGridTextColumn Header="Parâmetros" Binding="{Binding Parameters}" Width="*" />
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</TabItem>
```

**Passo 7:** Atualize a classe MainWindow para suportar a nova funcionalidade

```csharp
// Adicione estes métodos à classe MainWindow em UI/Views/MainWindow.xaml.cs
/// <summary>
/// Atualiza a lista de categorias
/// </summary>
/// <param name="categories">Lista de categorias</param>
public void UpdateCategories(List<string> categories)
{
    var viewModel = DataContext as MainWindowViewModel;
    viewModel?.ElementViewModel.UpdateCategories(categories);
}

/// <summary>
/// Atualiza a lista de elementos
/// </summary>
/// <param name="elements">Lista de elementos</param>
public void UpdateElements(List<Core.Models.ElementInfo> elements)
{
    var viewModel = DataContext as MainWindowViewModel;
    viewModel?.ElementViewModel.UpdateElements(elements);
}
```

## 7. Atualizar o MainWindowViewModel

**Passo 8:** Atualize o MainWindowViewModel para incluir o novo ElementViewModel

```csharp
// Adicione isso à classe MainWindowViewModel em UI/ViewModels/MainWindowViewModel.cs

// Propriedade para o ElementViewModel
private ElementViewModel _elementViewModel;

/// <summary>
/// Obtém o ViewModel para a funcionalidade de elementos
/// </summary>
public ElementViewModel ElementViewModel
{
    get => _elementViewModel;
    private set => SetProperty(ref _elementViewModel, value);
}

// Adicione no construtor da classe MainWindowViewModel
_elementViewModel = new ElementViewModel(elementEventHandler);
```

## 8. Registrar o Serviço e Event Handler

**Passo 9:** Atualize a classe RevitApp para registrar o novo serviço e event handler

```csharp
// Adicione isso ao método ShowMainWindow e ShowMainWindowSeparateThread em RevitApp.cs

// Criar serviços
IRevitDocumentService documentService = new RevitDocumentService(uiApplication);
IElementService elementService = new ElementService(uiApplication);

// Criar event handlers
StringParameterEventHandler stringEventHandler = new StringParameterEventHandler();
ViewEventHandler viewEventHandler = new ViewEventHandler(documentService);
ElementEventHandler elementEventHandler = new ElementEventHandler(elementService);

// Criar view model
MainWindowViewModel viewModel = new MainWindowViewModel(
    uiApplication,
    stringEventHandler,
    viewEventHandler,
    elementEventHandler);
```

## 9. Testar a Nova Funcionalidade

**Passo 10:** Compile o projeto e teste a nova funcionalidade

1. Abra o Visual Studio e compile o projeto
2. Abra o Revit 2024
3. Carregue um modelo com elementos
4. Clique no botão "WPF Template" na aba "Template" da faixa de opções
5. Vá para a aba "Elementos" na janela do aplicativo
6. Clique em "Carregar Categorias" para ver as categorias disponíveis
7. Selecione uma categoria e clique em "Carregar Elementos" para ver os elementos

## Resumo

Seguindo estes passos, você adicionou uma nova funcionalidade completa ao plugin Revit, respeitando a arquitetura em camadas:

1. **Modelo**: Definiu a classe ElementInfo para representar os dados
2. **Serviço**: Criou a interface IElementService e sua implementação
3. **Event Handler**: Implementou o ElementEventHandler para operações da API Revit
4. **ViewModel**: Criou o ElementViewModel para gerenciar a lógica da UI
5. **View**: Atualizou a interface do usuário para exibir a nova funcionalidade
6. **Integração**: Registrou os novos componentes na aplicação principal

Esta abordagem mantém a separação de responsabilidades e facilita a manutenção e extensão do código no futuro.