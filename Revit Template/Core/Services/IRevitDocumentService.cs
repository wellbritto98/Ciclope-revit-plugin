using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Service interface for Revit document operations.
    /// </summary>
    public interface IRevitDocumentService
    {
        /// <summary>
        /// Gets the current Revit document.
        /// </summary>
        /// <returns>The current Revit document.</returns>
        Document GetCurrentDocument();

        /// <summary>
        /// Gets information about the current document.
        /// </summary>
        /// <returns>A string containing document information.</returns>
        string GetDocumentInfo();

        /// <summary>
        /// Gets all sheets in the document.
        /// </summary>
        /// <returns>A list of ViewSheet objects.</returns>
        Task<List<ViewSheet>> GetSheetsAsync();

        /// <summary>
        /// Renames all sheets in the document.
        /// </summary>
        /// <param name="namePrefix">The prefix to use for sheet names.</param>
        /// <returns>True if the operation was successful, false otherwise.</returns>
        bool RenameSheets(string namePrefix);

        /// <summary>
        /// Gets information about walls in the document.
        /// </summary>
        /// <returns>A string containing wall information.</returns>
        Task<string> GetWallInfoAsync();
    }
}
