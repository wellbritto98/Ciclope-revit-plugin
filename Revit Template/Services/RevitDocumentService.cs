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
                // Get all elements in the document (filtering out types, views, annotations, etc.)
                var allElements = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .Where(e => 
                        e.Category != null && 
                        e.Category.CategoryType == CategoryType.Model && 
                        e.Location != null &&
                        // Excluir elementos que não são construção física
                        !IsAnnotationOrViewElement(e) &&
                        // Verifica se o elemento tem um valor de categoria válido para construção
                        IsConstructionElement(e))
                    .ToList();
               // Group by Category and Family/Type
                var groupedElements = allElements
                    .GroupBy(e => new
                    {
                        CategoryName = e.Category.Name,
                        TypeName = e.GetTypeId() != null ? doc.GetElement(e.GetTypeId())?.Name ?? "Unknown" : "Unknown",
                        FamilyName = (e is FamilyInstance) ? ((FamilyInstance)e).Symbol.FamilyName : e.Name
                    })
                    .Select(group => 
                    {
                        var elementInfo = new ElementInfo
                        {
                            ElementId = $"{group.Key.CategoryName}_{group.Key.FamilyName}_{group.Key.TypeName}",
                            Category = group.Key.CategoryName,
                            Name = group.Key.FamilyName,
                            Type = group.Key.TypeName,
                            Quantity = group.Count(),
                            Area = CalculateTotalArea(group.ToList()),
                            Volume = CalculateTotalVolume(group.ToList()),
                            Perimeter = CalculateTotalPerimeter(group.ToList()),
                            RevitElementIds = group.Select(e => e.Id.IntegerValue).ToList()
                        };
                        return elementInfo;
                    })
                    .OrderBy(e => e.Category)
                    .ThenBy(e => e.Name)
                    .ToList();

                return groupedElements;
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                return new List<ElementInfo>();
            }

        }
           
               
            /// <summary>
        /// Calculates the total area for a collection of elements in square meters
        /// </summary>
        private double CalculateTotalArea(IList<Element> elements)
        {
            double totalArea = 0;
            Document doc = GetCurrentDocument();
            
            foreach (var element in elements)
            {
                try
                {
                    // Try to get area from element parameters
                    Parameter areaParam = element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                    if (areaParam == null || areaParam.StorageType != StorageType.Double)
                    {
                        // Try alternative area parameters
                        areaParam = element.LookupParameter("Area");
                    }
                    
                    if (areaParam != null && areaParam.StorageType == StorageType.Double)
                    {
                        // Convert from square feet to square meters
                        totalArea += UnitUtils.ConvertFromInternalUnits(areaParam.AsDouble(), UnitTypeId.SquareMeters);
                    }
                }
                catch
                {
                    // Skip this element if area calculation fails
                }
            }
            
            return totalArea;
        }        /// <summary>
        /// Calculates the total volume for a collection of elements in cubic meters
        /// </summary>
        private double CalculateTotalVolume(IList<Element> elements)
        {
            double totalVolume = 0;
            Document doc = GetCurrentDocument();
            
            foreach (var element in elements)
            {
                try
                {
                    // Try to get volume from element parameters
                    Parameter volumeParam = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
                    if (volumeParam == null || volumeParam.StorageType != StorageType.Double)
                    {
                        // Try alternative volume parameters
                        volumeParam = element.LookupParameter("Volume");
                    }
                    
                    if (volumeParam != null && volumeParam.StorageType == StorageType.Double)
                    {
                        // Convert from cubic feet to cubic meters
                        totalVolume += UnitUtils.ConvertFromInternalUnits(volumeParam.AsDouble(), UnitTypeId.CubicMeters);
                    }
                }
                catch
                {
                    // Skip this element if volume calculation fails
                }
            }
            
            return totalVolume;
        }        /// <summary>
        /// Calculates the total perimeter for a collection of elements in meters
        /// </summary>
        private double CalculateTotalPerimeter(IList<Element> elements)
        {
            double totalPerimeter = 0;
            Document doc = GetCurrentDocument();
            
            foreach (var element in elements)
            {
                try
                {
                    // Try to get perimeter from element parameters
                    Parameter perimeterParam = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                    if (perimeterParam == null || perimeterParam.StorageType != StorageType.Double)
                    {
                        // Try alternative perimeter parameters
                        perimeterParam = element.LookupParameter("Perimeter");
                        if (perimeterParam == null || perimeterParam.StorageType != StorageType.Double)
                        {
                            perimeterParam = element.LookupParameter("Length");
                        }
                    }
                    
                    if (perimeterParam != null && perimeterParam.StorageType == StorageType.Double)
                    {
                        // Convert from feet to meters
                        totalPerimeter += UnitUtils.ConvertFromInternalUnits(perimeterParam.AsDouble(), UnitTypeId.Meters);
                    }
                }
                catch
                {
                    // Skip this element if perimeter calculation fails
                }
            }
            
            return totalPerimeter;
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
                };

                // Verificar se a categoria do elemento está na lista de categorias de construção
                int catId = element.Category.Id.IntegerValue;
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
    }
}
