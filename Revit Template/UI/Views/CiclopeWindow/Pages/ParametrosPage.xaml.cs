using System.Windows.Controls;
using System.Windows.Data;
using Wpf.Ui;
using Wpf.Ui.Controls;
using RevitTemplate.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Extensions;
using RevitTemplate.Models;
using RevitTemplate.Utils;
using Autodesk.Revit.DB;
using TextBox = System.Windows.Controls.TextBox;

namespace RevitTemplate.UI.Views.Pages
{
    /// <summary>
    /// Representa a página de parâmetros na interface do usuário.
    /// </summary>
    public partial class ParametrosPage : Page
    {
        public ParametrosPageViewModel ViewModel { get; } = new ParametrosPageViewModel();

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="ParametrosPage"/>.
        /// </summary>
        public ParametrosPage()
        {
            InitializeComponent();
            DataContext = this;

            // Inicializa serviços
            ViewModel._snackbarService.SetSnackbarPresenter(Snackbar);
            ViewModel._dialogService.SetDialogHost(RootContentDialog);
        }

        /// <summary>
        /// Manipulador de evento que é chamado quando uma edição de célula é finalizada
        /// </summary>
        private void ElementosDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    // Obter o caminho de binding da coluna
                    var bindingPath = (column.Binding as System.Windows.Data.Binding)?.Path.Path;
                    if (!string.IsNullOrEmpty(bindingPath))
                    {
                        // Obter o elemento que está sendo editado
                        var elementInfo = e.Row.Item as ElementInfo;
                        if (elementInfo != null)
                        {
                            // Obter o valor inserido pelo usuário
                            var textBox = e.EditingElement as TextBox;
                            if (textBox != null)
                            {
                                string novoValor = textBox.Text;
                                string paramName = string.Empty;
                                
                                // Identificar qual parâmetro CICLOPE está sendo editado e definir o nome para o Revit
                                switch (bindingPath)
                                {
                                    case "ValorBase":
                                        paramName = "Base";
                                        elementInfo.ValorBase = novoValor; // Isso vai disparar o evento ParametroCiclopeAlterado
                                        break;
                                    case "ValorEstado":
                                        paramName = "Estado";
                                        elementInfo.ValorEstado = novoValor; // Isso vai disparar o evento ParametroCiclopeAlterado
                                        break;
                                    case "ValorCodigo":
                                        paramName = "Codigo";
                                        elementInfo.ValorCodigo = novoValor; // Isso vai disparar o evento ParametroCiclopeAlterado
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Exibe um diálogo de conteúdo assíncrono.
        /// </summary>
        public async Task ShowContentDialogAsync(ContentDialog dialog, IContentDialogService dialogService)
        {
            await dialogService.ShowAsync(dialog, CancellationToken.None);
        }

        /// <summary>
        /// Exibe uma mensagem de snackbar.
        /// </summary>
        public void ShowSnackbar(string message, string action, ISnackbarService snackbarService)
        {
            snackbarService.Show(message, action);
        }
    }
}
