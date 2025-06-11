using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RevitTemplate.Models;
using RevitTemplate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevitTemplate.Core.Services
{    /// <summary>
     /// Implementation of the IRevitDocumentService interface.
     /// </summary>
    public class RevitDocumentService : IRevitDocumentService
    {
        private readonly IUIApplicationProvider _uiApplicationProvider;

        /// <summary>
        /// Initializes a new instance of the RevitDocumentService class.
        /// </summary>
        /// <param name="uiApplicationProvider">The UI application provider.</param>
        public RevitDocumentService(IUIApplicationProvider uiApplicationProvider)
        {
            _uiApplicationProvider = uiApplicationProvider ?? throw new ArgumentNullException(nameof(uiApplicationProvider));
        }        /// <summary>
                 /// Gets the current Revit document.
                 /// </summary>
                 /// <returns>The current Revit document.</returns>
        public Document GetCurrentDocument()
        {
            if (_uiApplicationProvider.UIApplication == null)
                throw new InvalidOperationException("UIApplication is not available. Make sure the UIApplicationProvider is properly initialized.");

            return _uiApplicationProvider.UIApplication.ActiveUIDocument.Document;
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


        /// <summary>
        /// Gets all family instances in the current Revit document
        /// </summary>
        /// <returns>Collection of Revit family instances</returns>
        public ICollection<FamilyInstance> GetAllFamilyInstances()
        {
            try
            {
                Logger.LogThreadInfo("Get Family Instances Method");
                Document doc = GetCurrentDocument();

                // Return FamilyInstance (actual instances placed in the model)
                return new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<FamilyInstance>();
            }
        }

        /// <summary>
        /// Gets grouped element information with calculated properties
        /// </summary>
        /// <returns>List of ElementInfo objects containing grouped element data</returns>
        public async Task<List<ElementInfo>> GetElementInfoAsync()
        {
            Document doc = GetCurrentDocument();
            Logger.LogThreadInfo("Get Element Info Method");
            try
            {
                var allElements = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .Where(e =>
                        e.Category != null &&
                        e.Category.CategoryType == CategoryType.Model &&
                        e.Location != null &&
                        !IsAnnotationOrViewElement(e) &&
                        IsConstructionElement(e))
                    .ToList();

                var elementInfoList = allElements.Select(e => new ElementInfo
                {
                    ElementId = e.Id.ToString(),
                    FamilyName = e is FamilyInstance fi ? fi.Symbol.FamilyName : e.Name,
                    Category = e.Category.Name,
                    FamilyType = e.Name,
                    Area = GetElementArea(e),
                    Volume = GetElementVolume(e),
                    Perimeter = GetElementPerimeter(e)
                }).ToList();

                return elementInfoList;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<ElementInfo>();
            }
        }

        /// <summary>
        /// Gets filtered element information with calculated properties
        /// </summary>
        /// <param name="filter">Filter criteria to apply to elements</param>
        /// <returns>List of ElementInfo objects containing filtered element data</returns>
        public async Task<List<ElementInfo>> GetElementInfoAsync(FilterRevitElements filter)
        {
            Document doc = GetCurrentDocument();
            Logger.LogThreadInfo("Get Filtered Element Info Method");
            try
            {
                var allElements = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .Where(e =>
                        e.Category != null &&
                        e.Category.CategoryType == CategoryType.Model &&
                        e.Location != null &&
                        !IsAnnotationOrViewElement(e) &&
                        IsConstructionElement(e))
                    .ToList();

                var elementInfoList = allElements.Select(e => new ElementInfo
                {
                    ElementId = e.Id.ToString(),
                    FamilyName = e is FamilyInstance fi ? fi.Symbol.FamilyName : e.Name,
                    Category = e.Category.Name,
                    FamilyType = e.Name,
                    Area = GetElementArea(e),
                    Volume = GetElementVolume(e),
                    Perimeter = GetElementPerimeter(e)
                }).ToList();

                // Apply filter if provided
                if (filter != null && !string.IsNullOrWhiteSpace(filter.Campo) && !string.IsNullOrWhiteSpace(filter.Valor))
                {
                    elementInfoList = ApplyFilter(elementInfoList, filter);
                    Logger.LogMessage($"Applied filter: {filter.Campo} = {filter.Valor}. Found {elementInfoList.Count} matching elements.");
                }

                return elementInfoList;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<ElementInfo>();
            }
        }

        /// <summary>
        /// Applies filter criteria to the element list
        /// </summary>
        /// <param name="elements">List of elements to filter</param>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Filtered list of elements</returns>
        private List<ElementInfo> ApplyFilter(List<ElementInfo> elements, FilterRevitElements filter)
        {
            try
            {
                switch (filter.Campo.ToLowerInvariant())
                {
                    case "category":
                        return elements.Where(e => 
                            string.Equals(e.Category, filter.Valor, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    
                    case "familyname":
                        return elements.Where(e => 
                            string.Equals(e.FamilyName, filter.Valor, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    
                    default:
                        Logger.LogMessage($"Campo de filtro não suportado: {filter.Campo}. Campos suportados: Category, FamilyName");
                        return elements; // Return unfiltered list if field is not supported
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return elements; // Return unfiltered list on error
            }
        }

        private double GetElementArea(Element element)
        {
            Parameter areaParam = element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
            return areaParam != null && areaParam.HasValue ? UnitUtils.ConvertFromInternalUnits(areaParam.AsDouble(), UnitTypeId.SquareMeters) : 0;
        }

        private double GetElementVolume(Element element)
        {
            Parameter volumeParam = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
            return volumeParam != null && volumeParam.HasValue ? UnitUtils.ConvertFromInternalUnits(volumeParam.AsDouble(), UnitTypeId.CubicMeters) : 0;
        }

        private double GetElementPerimeter(Element element)
        {
            Parameter perimeterParam = element.LookupParameter("Perimeter");
            return perimeterParam != null && perimeterParam.HasValue ? UnitUtils.ConvertFromInternalUnits(perimeterParam.AsDouble(), UnitTypeId.Meters) : 0;
        }

        /// <summary>
        /// Verifica se o elemento é um elemento de anotação, visualização ou interface
        /// </summary>
        /// <param name="element">O elemento a ser verificado</param>
        /// <returns>True se o elemento for de anotação ou visualização</returns>
        private bool IsAnnotationOrViewElement(Element element)
        {
            // Verificar elementos específicos que queremos excluir
            if (element is View ||
                element is Viewport ||
                element is TextNote ||
                element is Group ||
                element is ViewSheet ||
                element is SketchPlane ||
                element is ReferencePlane ||
                element is RoomTagType ||
                element is Room ||
                element is Grid ||
                element is Level)
            {
                return true;
            }

            // Verificar categorias específicas para excluir
            if (element.Category != null)
            {
                // Lista de nomes de categorias a serem excluídos
                string[] excludedCategoryNames = new string[]
                {
                    "Views",
                    "Cameras",
                    "View Reference",
                    "View Titles",
                    "Viewports",
                    "Legend",
                    "Legend Components",
                    "Text Notes",
                    "Annotation Symbols",
                    "Callouts",
                    "Elevations",
                    "Sections",
                    "Scope Boxes",
                    "Sheets",
                    "Matchline",
                    "Grids",
                    "Levels",
                    "Reference Planes",
                    "Boundary Conditions",
                    "Area Boundary",
                    "Room Separation",
                    "Sketch",
                    "<Room Separation>",
                    "<Area Boundary>",
                    "<Sketch>"
                };

                // Verificar se o nome da categoria está na lista de exclusão
                if (excludedCategoryNames.Contains(element.Category.Name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se o elemento é um elemento de construção real
        /// </summary>
        /// <param name="element">O elemento a ser verificado</param>
        /// <returns>True se o elemento for um elemento de construção</returns>
        private bool IsConstructionElement(Element element)
        {
            // Lista de categorias que representam construções físicas
            if (element.Category != null)
            {
                // Categorias que são construção real
                BuiltInCategory[] constructionCategories = new BuiltInCategory[]
                {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_Ceilings,
                    BuiltInCategory.OST_Doors,
                    BuiltInCategory.OST_Windows,
                    BuiltInCategory.OST_Stairs,
                    BuiltInCategory.OST_Roofs,
                    BuiltInCategory.OST_Columns,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_StructuralFoundation,
                    BuiltInCategory.OST_Furniture,
                    BuiltInCategory.OST_FurnitureSystems,
                    BuiltInCategory.OST_Planting,
                    BuiltInCategory.OST_PlumbingFixtures,
                    BuiltInCategory.OST_MechanicalEquipment,
                    BuiltInCategory.OST_ElectricalEquipment,
                    BuiltInCategory.OST_ElectricalFixtures,
                    BuiltInCategory.OST_LightingFixtures,
                    BuiltInCategory.OST_Casework,
                    BuiltInCategory.OST_SpecialityEquipment,
                    BuiltInCategory.OST_GenericModel
                };                // Verificar se a categoria do elemento está na lista de categorias de construção
                long catId = element.Category.Id.Value;
                foreach (BuiltInCategory builtInCat in constructionCategories)
                {
                    if (catId == (int)builtInCat)
                    {
                        return true;
                    }
                }

                // Para elementos que não correspondem exatamente às categorias embutidas,
                // verificamos se eles têm parâmetros físicos (geometria)
                if (element.get_Geometry(new Options()) != null)
                {
                    // Verificar se tem área, volume ou outros parâmetros físicos
                    if (element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED) != null ||
                        element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED) != null ||
                        element.LookupParameter("Area") != null ||
                        element.LookupParameter("Volume") != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all unique category names from construction elements in the document
        /// </summary>
        /// <returns>List of category names</returns>
        public async Task<List<string>> GetCategoryNamesAsync()
        {
            Document doc = GetCurrentDocument();
            Logger.LogThreadInfo("Get Category Names Method");
            try
            {
                return await Task.Run(() =>
                {
                    var allElements = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .WhereElementIsViewIndependent()
                        .Where(e =>
                            e.Category != null &&
                            e.Category.CategoryType == CategoryType.Model &&
                            e.Location != null &&
                            !IsAnnotationOrViewElement(e) &&
                            IsConstructionElement(e))
                        .ToList();

                    var categoryNames = allElements
                        .Select(e => e.Category.Name)
                        .Distinct()
                        .OrderBy(name => name)
                        .ToList();

                    Logger.LogMessage($"Found {categoryNames.Count} unique categories");
                    return categoryNames;
                });
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets all unique family names from construction elements in the document
        /// </summary>
        /// <returns>List of family names</returns>
        public async Task<List<string>> GetFamilyNamesAsync()
        {
            Document doc = GetCurrentDocument();
            Logger.LogThreadInfo("Get Family Names Method");
            try
            {
                return await Task.Run(() =>
                {
                    var allElements = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .WhereElementIsViewIndependent()
                        .Where(e =>
                            e.Category != null &&
                            e.Category.CategoryType == CategoryType.Model &&
                            e.Location != null &&
                            !IsAnnotationOrViewElement(e) &&
                            IsConstructionElement(e))
                        .ToList();

                    var familyNames = allElements
                        .Select(e => e is FamilyInstance fi ? fi.Symbol.FamilyName : e.Name)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct()
                        .OrderBy(name => name)
                        .ToList();

                    Logger.LogMessage($"Found {familyNames.Count} unique family names");
                    return familyNames;
                });
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets all unique family names from construction elements in the document filtered by category
        /// </summary>
        /// <param name="categoryName">The category name to filter by</param>
        /// <returns>List of family names for the specified category</returns>
        public async Task<List<string>> GetFamilyNamesByCategoryAsync(string categoryName)
        {
            Document doc = GetCurrentDocument();
            Logger.LogThreadInfo("Get Family Names By Category Method");
            try
            {
                return await Task.Run(() =>
                {
                    var allElements = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .WhereElementIsViewIndependent()
                        .Where(e =>
                            e.Category != null &&
                            e.Category.CategoryType == CategoryType.Model &&
                            e.Location != null &&
                            !IsAnnotationOrViewElement(e) &&
                            IsConstructionElement(e) &&
                            string.Equals(e.Category.Name, categoryName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var familyNames = allElements
                        .Select(e => e is FamilyInstance fi ? fi.Symbol.FamilyName : e.Name)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct()
                        .OrderBy(name => name)
                        .ToList();

                    Logger.LogMessage($"Found {familyNames.Count} unique family names for category '{categoryName}'");
                    return familyNames;
                });
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<string>();
            }
        }
    }
}
