using System.Windows;
using System.Windows.Controls;
using RevitTemplate.Models;
namespace RevitTemplate.UI.Views.Pages
{
    /// <summary>
    /// Representa a página de orçamento na interface do usuário.
    /// </summary>
    public partial class OrcamentoPage : Page
    {
        private int _counter = 0;

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="OrcamentoPage"/>.
        /// </summary>
        public OrcamentoPage()
        {
            InitializeComponent();
            DataContext = this;

            // Atualiza o contador depois que a página for carregada
            Loaded += (sender, e) =>
            {
                CounterTextBlock.Text = _counter.ToString();
            };
        }
        /// <summary>
        /// Manipulador de eventos para o clique do botão de orçamento.
        /// </summary>
        /// <param name="sender">O objeto que disparou o evento.</param>
        /// <param name="e">Os dados do evento.</param>
        private void OnBaseButtonClick(object sender, RoutedEventArgs e)
        {
            _counter++;
            CounterTextBlock.Text = _counter.ToString();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}