using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Threading.Tasks;
using RevitTemplate.Models;

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

        /// <summary>
        /// Gets all family instances in the current Revit document
        /// </summary>
        /// <returns>Collection of Revit family instances</returns>
        ICollection<FamilyInstance> GetAllFamilyInstances();

        /// <summary>
        /// Gets grouped element information with calculated properties
        /// </summary>
        /// <returns>List of ElementInfo objects containing grouped element data</returns>
        Task<List<ElementInfo>> GetElementInfoAsync();

        /// <summary>
        /// Gets filtered element information with calculated properties
        /// </summary>
        /// <param name="filter">Filter criteria to apply to elements</param>
        /// <returns>List of ElementInfo objects containing filtered element data</returns>
        Task<List<ElementInfo>> GetElementInfoAsync(FilterRevitElements filter);

        /// <summary>
        /// Gets all unique category names from construction elements in the document
        /// </summary>
        /// <returns>List of category names</returns>
        Task<List<string>> GetCategoryNamesAsync();

        /// <summary>
        /// Gets all unique family names from construction elements in the document
        /// </summary>
        /// <returns>List of family names</returns>
        Task<List<string>> GetFamilyNamesAsync();

        /// <summary>
        /// Gets all unique family names from construction elements in the document filtered by category
        /// </summary>
        /// <param name="categoryName">The category name to filter by</param>
        /// <returns>List of family names for the specified category</returns>
        Task<List<string>> GetFamilyNamesByCategoryAsync(string categoryName);
    }
}
