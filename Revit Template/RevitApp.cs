using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using RevitTemplate.Core.Services;
using RevitTemplate.Infrastructure;
using RevitTemplate.UI.ViewModels;
using RevitTemplate.UI.Views;
using RevitTemplate.Utils;

namespace RevitTemplate
{
    /// <summary>
    /// Main application class that implements IExternalApplication.
    /// </summary>
    public class RevitApp : IExternalApplication
    {
        private MainWindow _mainWindow;
        private Thread _uiThread;

        /// <summary>
        /// Gets the singleton instance of the application.
        /// </summary>
        public static RevitApp Instance { get; private set; }
        public static AddCiclopeParametersEventHandler CiclopeParametersEventHandler { get; private set; }

        /// <summary>
        /// Called when Revit starts up.
        /// </summary>
        /// <param name="application">The UI controlled application.</param>
        /// <returns>The result of the startup.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Set the static instance
                Instance = this;

                // Add event handlers
                application.ApplicationClosing += OnApplicationClosing;
                application.Idling += OnIdling;

                // Create the ribbon panel
                RibbonPanel panel = CreateRibbonPanel(application);
                
                // Add buttons to the ribbon panel
                AddRibbonButtons(panel);
                CiclopeParametersEventHandler = new AddCiclopeParametersEventHandler();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return Result.Failed;
            }
        }

        /// <summary>
        /// Called when Revit shuts down.
        /// </summary>
        /// <param name="application">The UI controlled application.</param>
        /// <returns>The result of the shutdown.</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                // Clean up resources
                _mainWindow = null;
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return Result.Failed;
            }
        }


        /// <summary>
        /// Shows the main window in a single thread.
        /// </summary>
        /// <param name="uiApplication">The Revit UI application.</param>
        public void ShowMainWindow(UIApplication uiApplication)
        {
            try
            {
                // If the window is already open, return
                if (_mainWindow != null)
                {
                    _mainWindow.Activate();
                    return;
                }

                // Create services
                IRevitDocumentService documentService = new RevitDocumentService(uiApplication);

                // Create event handlers
                StringParameterEventHandler stringEventHandler = new StringParameterEventHandler();
                ViewEventHandler viewEventHandler = new ViewEventHandler(documentService);

                // Create view model
                MainWindowViewModel viewModel = new MainWindowViewModel(
                    uiApplication,
                    stringEventHandler,
                    viewEventHandler);

                // Create and show the window
                _mainWindow = new MainWindow
                {
                    DataContext = viewModel
                };

                _mainWindow.Closed += (s, e) => _mainWindow = null;
                _mainWindow.Show();
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows the main window in a separate thread.
        /// </summary>
        /// <param name="uiApplication">The Revit UI application.</param>
        public void ShowMainWindowSeparateThread(UIApplication uiApplication)
        {
            try
            {
                // If the thread is already running, return
                if (_uiThread != null && _uiThread.IsAlive)
                {
                    return;
                }

                // Create services
                IRevitDocumentService documentService = new RevitDocumentService(uiApplication);

                // Create event handlers
                StringParameterEventHandler stringEventHandler = new StringParameterEventHandler();
                ViewEventHandler viewEventHandler = new ViewEventHandler(documentService);

                // Start a new thread for the UI
                _uiThread = new Thread(() =>
                {
                    try
                    {
                        // Set up the synchronization context
                        SynchronizationContext.SetSynchronizationContext(
                            new DispatcherSynchronizationContext(
                                Dispatcher.CurrentDispatcher));

                        // Create view model
                        MainWindowViewModel viewModel = new MainWindowViewModel(
                            uiApplication,
                            stringEventHandler,
                            viewEventHandler);

                        // Create and show the window
                        _mainWindow = new MainWindow
                        {
                            DataContext = viewModel
                        };

                        _mainWindow.Closed += (s, e) =>
                        {
                            _mainWindow = null;
                            Dispatcher.CurrentDispatcher.InvokeShutdown();
                        };

                        _mainWindow.Show();
                        Dispatcher.Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.HandleError(ex);
                    }
                });

                _uiThread.SetApartmentState(ApartmentState.STA);
                _uiThread.IsBackground = true;
                _uiThread.Start();
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
            }
        }

        public void ShowCiclopeWindow(UIApplication uiApplication)
        {
            try
            {
                var window = new CiclopeWindow();
                window.Show();
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
            }
        }

        private RibbonPanel CreateRibbonPanel(UIControlledApplication application)
        {
            // Tab name
            string tabName = "Template";
            
            // Try to create the ribbon tab
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
            }

            // Try to create the ribbon panel
            try
            {
                application.CreateRibbonPanel(tabName, "Develop");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
            }

            // Find the panel
            RibbonPanel panel = null;
            List<RibbonPanel> panels = application.GetRibbonPanels(tabName);
            foreach (RibbonPanel p in panels.Where(p => p.Name == "Develop"))
            {
                panel = p;
                break;
            }

            return panel;
        }

        private void AddRibbonButtons(RibbonPanel panel)
        {
            if (panel == null)
            {
                return;
            }

            // Get the assembly path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Add button for single-threaded mode
            if (panel.AddItem(
                new PushButtonData(
                    "SingleThreadCommand",
                    "WPF Template",
                    assemblyPath,
                    "RevitTemplate.Commands.SingleThreadCommand")) is PushButton button1)
            {
                button1.ToolTip = "Launch the application in a single thread.";
                Uri uriImage = new Uri("pack://application:,,,/RevitTemplate;component/Resources/code-small.png");
                BitmapImage largeImage = new BitmapImage(uriImage);
                button1.LargeImage = largeImage;
            }

            // Add button for multi-threaded mode
            if (panel.AddItem(
                new PushButtonData(
                    "MultiThreadCommand",
                    "WPF Template\nMulti-Thread",
                    assemblyPath,
                    "RevitTemplate.Commands.MultiThreadCommand")) is PushButton button2)
            {
                button2.ToolTip = "Launch the application in a separate thread.";
                Uri uriImage = new Uri("pack://application:,,,/RevitTemplate;component/Resources/code-small.png");
                BitmapImage largeImage = new BitmapImage(uriImage);
                button2.LargeImage = largeImage;
            }

            // Add button for CICLOPE mode
            if (panel.AddItem(
                new PushButtonData(
                    "CiclopeCommand",
                    "CICLOPE",
                    assemblyPath,
                    "RevitTemplate.Commands.CiclopeCommand")) is PushButton button3)
            {
                button3.ToolTip = "Abre a janela CICLOPE com funcionalidades específicas.";
                Uri uriImage = new Uri("pack://application:,,,/RevitTemplate;component/Resources/building.png");
                BitmapImage largeImage = new BitmapImage(uriImage);
                button3.LargeImage = largeImage;
            }
        }

        private void OnIdling(object sender, IdlingEventArgs e)
        {
            // This method is called when Revit is idle
            // You can use it to perform background tasks
        }

        private void OnApplicationClosing(object sender, ApplicationClosingEventArgs e)
        {
            // This method is called when Revit is closing
            // You can use it to clean up resources
        }
    }
}
