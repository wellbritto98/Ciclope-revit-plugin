using Autodesk.Revit.UI;
using RevitTemplate.Infrastructure;
using RevitTemplate.Utils;
using System;
using System.Windows.Input;

namespace RevitTemplate.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the main window.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly UIApplication _uiApplication;
        private readonly StringParameterEventHandler _stringEventHandler;
        private readonly ViewEventHandler _viewEventHandler;

        private string _logText = string.Empty;
        private bool _documentDataSelected;
        private bool _sheetDataSelected;
        private bool _wallDataSelected;

        /// <summary>
        /// Initializes a new instance of the MainWindowViewModel class.
        /// </summary>
        /// <param name="uiApplication">The Revit UI application.</param>
        /// <param name="stringEventHandler">The string parameter event handler.</param>
        /// <param name="viewEventHandler">The view event handler.</param>
        public MainWindowViewModel(
            UIApplication uiApplication,
            StringParameterEventHandler stringEventHandler,
            ViewEventHandler viewEventHandler)
        {
            _uiApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
            _stringEventHandler = stringEventHandler ?? throw new ArgumentNullException(nameof(stringEventHandler));
            _viewEventHandler = viewEventHandler ?? throw new ArgumentNullException(nameof(viewEventHandler));

            // Initialize commands
            ExecuteStringCommand = new RelayCommand(ExecuteStringCommandAction);
            ExecuteViewCommand = new RelayCommand(ExecuteViewCommandAction);
            ExecuteMethodACommand = new RelayCommand(ExecuteMethodAAction);
            ExecuteMethodBCommand = new RelayCommand(ExecuteMethodBAction);
            ExecuteMethodCCommand = new RelayCommand(ExecuteMethodCAction);
        }

        /// <summary>
        /// Gets or sets the log text.
        /// </summary>
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether document data is selected.
        /// </summary>
        public bool DocumentDataSelected
        {
            get => _documentDataSelected;
            set => SetProperty(ref _documentDataSelected, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether sheet data is selected.
        /// </summary>
        public bool SheetDataSelected
        {
            get => _sheetDataSelected;
            set => SetProperty(ref _sheetDataSelected, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether wall data is selected.
        /// </summary>
        public bool WallDataSelected
        {
            get => _wallDataSelected;
            set => SetProperty(ref _wallDataSelected, value);
        }

        /// <summary>
        /// Gets the command for executing a string parameter event.
        /// </summary>
        public ICommand ExecuteStringCommand { get; }

        /// <summary>
        /// Gets the command for executing a view parameter event.
        /// </summary>
        public ICommand ExecuteViewCommand { get; }

        /// <summary>
        /// Gets the command for executing method A.
        /// </summary>
        public ICommand ExecuteMethodACommand { get; }

        /// <summary>
        /// Gets the command for executing method B.
        /// </summary>
        public ICommand ExecuteMethodBCommand { get; }

        /// <summary>
        /// Gets the command for executing method C.
        /// </summary>
        public ICommand ExecuteMethodCCommand { get; }

        /// <summary>
        /// Appends text to the log.
        /// </summary>
        /// <param name="text">The text to append.</param>
        public void AppendToLog(string text)
        {
            LogText += $"\n{DateTime.Now.ToLongTimeString()}\t{text}";
        }

        private void ExecuteStringCommandAction(object parameter)
        {
            try
            {
                // Raise external event with a string argument
                _stringEventHandler.Raise($"Title: {_uiApplication.ActiveUIDocument.Document.Title}");
                AppendToLog("String Command executed");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                AppendToLog($"Error: {ex.Message}");
            }
        }

        private void ExecuteViewCommandAction(object parameter)
        {
            try
            {
                // Raise external event with the view as an argument
                _viewEventHandler.Raise(parameter as Views.MainWindow);
                AppendToLog("View Command executed");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                AppendToLog($"Error: {ex.Message}");
            }
        }

        private void ExecuteMethodAAction(object parameter)
        {
            try
            {
                AppendToLog("Method A executed");
                // This is a non-external method, so it can be executed directly
                System.Windows.MessageBox.Show("Non-External Method Executed Successfully", "Method A");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                AppendToLog($"Error: {ex.Message}");
            }
        }

        private void ExecuteMethodBAction(object parameter)
        {
            try
            {
                AppendToLog("Method B executed");
                // This is a non-external method, so it can be executed directly
                System.Windows.MessageBox.Show("Non-External Method Executed Successfully", "Method B");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                AppendToLog($"Error: {ex.Message}");
            }
        }

        private void ExecuteMethodCAction(object parameter)
        {
            try
            {
                AppendToLog("Method C executed");
                // This is a non-external method, so it can be executed directly
                System.Windows.MessageBox.Show("Non-External Method Executed Successfully", "Method C");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                AppendToLog($"Error: {ex.Message}");
            }
        }
    }
}
