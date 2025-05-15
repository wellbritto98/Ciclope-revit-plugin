using System;
using System.Windows;

namespace RevitTemplate.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Gets a value indicating whether document data is selected.
        /// </summary>
        public bool DocumentDataSelected => CbDocumentData.IsChecked ?? false;

        /// <summary>
        /// Gets a value indicating whether sheet data is selected.
        /// </summary>
        public bool SheetDataSelected => CbSheetData.IsChecked ?? false;

        /// <summary>
        /// Gets a value indicating whether wall data is selected.
        /// </summary>
        public bool WallDataSelected => CbWallData.IsChecked ?? false;

        /// <summary>
        /// Appends text to the debug log.
        /// </summary>
        /// <param name="text">The text to append.</param>
        public void AppendToLog(string text)
        {
            if (TbDebug != null)
            {
                TbDebug.Text += $"\n{DateTime.Now.ToLongTimeString()}\t{text}";
                TbDebug.ScrollToEnd();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
