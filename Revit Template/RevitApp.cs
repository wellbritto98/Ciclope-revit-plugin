using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;
using RevitTemplate.Core.Services;
using RevitTemplate.Infrastructure;
using RevitTemplate.Services;
using RevitTemplate.UI.Views;
using RevitTemplate.UI.Views.Pages;
using RevitTemplate.Utils;

namespace RevitTemplate
{
    /// <summary>
    /// Main application class that implements IExternalApplication.
    /// </summary>
    public class RevitApp : IExternalApplication
    {
        public static RevitApp Instance { get; set; } 
        private IServiceProvider _serviceProvider;
        private CiclopeWindow _window;
        private Thread _uiThread;        
        /// <summary>

        public static AddCiclopeParametersEventHandler CiclopeParametersEventHandler { get; private set; }
        
        /// <summary>
        /// Gets the service provider for dependency injection.
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Called when Revit starts up.
        /// </summary>
        /// <param name="application">The UI controlled application.</param>
        /// <returns>The result of the startup.</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Instance = this;
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                _serviceProvider = serviceCollection.BuildServiceProvider();


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
        }        private void ConfigureServices(IServiceCollection services)
        {
            // Use the centralized configuration
            DependencyInjectionConfig.ConfigureServices(services);
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
                _window = null;

                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return Result.Failed;
            }
        }        /// <summary>
        /// Shows the CICLOPE window using dependency injection.
        /// </summary>
        /// <param name="uiApplication">The Revit UI application.</param>
        public void ShowCiclopeWindow(UIApplication uiApplication)
        {
            try
            {
                // Set the UIApplication in the singleton provider so services can access it
                UIApplicationProvider.Instance.SetUIApplication(uiApplication);
                
                var tokenService = _serviceProvider.GetRequiredService<TokenService>();
                _window = new CiclopeWindow(tokenService,_serviceProvider);
                _window.Show();
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
        }        /// <summary>
        /// Configura o HttpClient com o token JWT se disponível
        /// </summary>
        
    }
}
