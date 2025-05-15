using Autodesk.Revit.UI;
using RevitTemplate.Core.Services;
using RevitTemplate.UI.Views;
using RevitTemplate.Utils;
using System;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// Event handler for operations that require a View parameter.
    /// </summary>
    public class ViewEventHandler : RevitEventHandler<MainWindow>
    {
        private readonly IRevitDocumentService _documentService;

        /// <summary>
        /// Initializes a new instance of the ViewEventHandler class.
        /// </summary>
        /// <param name="documentService">The document service.</param>
        public ViewEventHandler(IRevitDocumentService documentService)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        /// <summary>
        /// Executes the event handler with the specified View parameter.
        /// </summary>
        /// <param name="app">The Revit UI application.</param>
        /// <param name="window">The main window passed to the event handler.</param>
        protected override void Execute(UIApplication app, MainWindow window)
        {
            Logger.LogMessage("View Event Handler executed");

            // Check which operations to perform based on UI selections
            bool documentDataSelected = false;
            bool sheetDataSelected = false;
            bool wallDataSelected = false;

            window.Dispatcher.Invoke(() =>
            {
                documentDataSelected = window.DocumentDataSelected;
                sheetDataSelected = window.SheetDataSelected;
                wallDataSelected = window.WallDataSelected;
            });

            // Perform operations based on selections
            if (documentDataSelected)
            {
                string docInfo = _documentService.GetDocumentInfo();
                window.Dispatcher.Invoke(() => window.AppendToLog(docInfo));
            }

            if (sheetDataSelected)
            {
                bool result = _documentService.RenameSheets("TEST");
                window.Dispatcher.Invoke(() => window.AppendToLog($"Sheet rename operation: {(result ? "Successful" : "Failed")}"));
            }

            if (wallDataSelected)
            {
                var wallInfoTask = _documentService.GetWallInfoAsync();
                wallInfoTask.ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        window.Dispatcher.Invoke(() => window.AppendToLog(task.Result));
                    }
                    else if (task.IsFaulted && task.Exception != null)
                    {
                        Logger.HandleError(task.Exception);
                    }
                });
            }
        }
    }
}
