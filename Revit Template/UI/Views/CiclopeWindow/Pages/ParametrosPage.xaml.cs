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
        }        /// <summary>
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
