using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Implementation of the IRevitDocumentService interface.
    /// </summary>
    public class RevitDocumentService : IRevitDocumentService
    {
        private readonly UIApplication _uiApplication;

        /// <summary>
        /// Initializes a new instance of the RevitDocumentService class.
        /// </summary>
        /// <param name="uiApplication">The Revit UI application.</param>
        public RevitDocumentService(UIApplication uiApplication)
        {
            _uiApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
        }

        /// <summary>
        /// Gets the current Revit document.
        /// </summary>
        /// <returns>The current Revit document.</returns>
        public Document GetCurrentDocument()
        {
            return _uiApplication.ActiveUIDocument.Document;
        }

        /// <summary>
        /// Gets information about the current document.
        /// </summary>
        /// <returns>A string containing document information.</returns>
        public string GetDocumentInfo()
        {
            Document doc = GetCurrentDocument();
            return $"Document Title: {doc.Title}, Path: {doc.PathName}";
        }

        /// <summary>
        /// Gets all sheets in the document asynchronously.
        /// </summary>
        /// <returns>A list of ViewSheet objects.</returns>
        public async Task<List<ViewSheet>> GetSheetsAsync()
        {
            Document doc = GetCurrentDocument();
            return await Task.Run(() =>
            {
                Logger.LogThreadInfo("Get Sheets Method");
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSheet))
                    .Select(p => (ViewSheet)p).ToList();
            });
        }

        /// <summary>
        /// Renames all sheets in the document.
        /// </summary>
        /// <param name="namePrefix">The prefix to use for sheet names.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        public bool RenameSheets(string namePrefix)
        {
            Logger.LogThreadInfo("Sheet Rename Method");
            Document doc = GetCurrentDocument();

            // Get sheets
            List<ViewSheet> sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Select(p => (ViewSheet)p).ToList();

            try
            {
                // Rename all the sheets
                using (Transaction t = new Transaction(doc, "Rename Sheets"))
                {
                    Logger.LogThreadInfo("Sheet Rename Transaction");
                    t.Start();

                    foreach (ViewSheet sheet in sheets)
                    {
                        sheet.LookupParameter("Sheet Name")?.Set(namePrefix);
                    }

                    t.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return false;
            }
        }

        /// <summary>
        /// Gets information about walls in the document asynchronously.
        /// </summary>
        /// <returns>A string containing wall information.</returns>
        public async Task<string> GetWallInfoAsync()
        {
            Document doc = GetCurrentDocument();
            return await Task.Run(() =>
            {
                Logger.LogThreadInfo("Wall Count Method");

                // Get all walls in the document
                ICollection<Wall> walls = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType()
                    .Select(p => (Wall)p).ToList();

                // Format the message to show the number of walls in the project
                return $"There are {walls.Count} Walls in the project";
            });
        }
    }
}
