using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.Core.Services;
using RevitTemplate.Utils;
using RevitTemplate.Models;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// EventHandler para selecionar elementos no modelo 3D
    /// </summary>
    public class SelectElementsEventHandler : RevitEventHandler<List<int>>
    {
        /// <summary>
        /// Executa a seleção de elementos no modelo Revit
        /// </summary>
        protected override void Execute(UIApplication app, List<int> elementIds)
        {
            try
            {
                Logger.LogThreadInfo("Selecionando elementos no modelo 3D");
                
                if (elementIds == null || elementIds.Count == 0)
                {
                    Logger.LogMessage("Nenhum elemento para selecionar");
                    return;
                }
                  Document doc = app.ActiveUIDocument.Document;
                UIDocument uidoc = app.ActiveUIDocument;

                // Limpa a seleção atual
                IList<ElementId> emptyList = new List<ElementId>();
                uidoc.Selection.SetElementIds(emptyList);

                // Obtém uma vista 3D para zoom
                View3D view3D = FindOr3DView(doc);
                if (view3D != null)
                {
                    // Ativa a vista 3D
                    uidoc.ActiveView = view3D;
                    Logger.LogMessage($"Ativada vista 3D: {view3D.Name}");
                }
                else
                {
                    // Se não foi possível encontrar ou criar uma vista 3D, notifica o usuário
                    TaskDialog.Show("Aviso", "Não foi possível encontrar ou criar uma vista 3D para mostrar os elementos.");
                    return;
                }
                
                // Cria um conjunto de IDs de elementos para seleção
                ICollection<ElementId> elementsToSelect = new List<ElementId>();
                foreach (int id in elementIds)
                {
                    ElementId elementId = new ElementId(id);
                    elementsToSelect.Add(elementId);
                }
                
                // Seleciona os elementos
                if (elementsToSelect.Count > 0)
                {
                    uidoc.Selection.SetElementIds(elementsToSelect);
                    
                    // Zoom nos elementos selecionados
                    if (view3D != null)
                    {
                        uidoc.ShowElements(elementsToSelect);
                    }
                    
                    Logger.LogMessage($"Selecionados {elementsToSelect.Count} elemento(s) no modelo");
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Erro ao selecionar elementos", $"{ex.Message}\n\nDetalhes: {ex}");
            }
        }
          /// <summary>
        /// Encontra uma vista 3D no documento ou usa a vista ativa se for 3D
        /// Se não encontrar nenhuma vista 3D, abre a vista 3D padrão
        /// </summary>
        private View3D FindOr3DView(Document doc)
        {
            // Verifica se a vista ativa já é uma vista 3D
            if (doc.ActiveView is View3D activeView3D)
            {
                return activeView3D;
            }
            
            // Busca por vistas 3D no documento
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D));
                
            View3D view3D = null;
            
            // Primeiro, tenta encontrar a vista 3D padrão "{3D}"
            view3D = collector
                .Cast<View3D>()
                .FirstOrDefault(v => v.Name == "{3D}" && !v.IsTemplate);
                
            // Se não encontrou a vista padrão, prefere vistas 3D que não sejam de perspectiva
            if (view3D == null)
            {
                view3D = collector
                    .Cast<View3D>()
                    .FirstOrDefault(v => !v.IsPerspective && !v.IsTemplate);
            }
            
            // Se ainda não encontrou, pega qualquer vista 3D não de template
            if (view3D == null)
            {
                view3D = collector
                    .Cast<View3D>()
                    .FirstOrDefault(v => !v.IsTemplate);
            }
            
            // Se não encontrou nenhuma vista 3D disponível, cria uma nova vista 3D
            if (view3D == null)
            {
                try
                {
                    // Cria uma transação para criar uma nova vista 3D
                    using (Transaction tx = new Transaction(doc, "Criar Vista 3D"))
                    {
                        tx.Start();
                        
                        // Cria uma nova vista 3D padrão
                        view3D = View3D.CreateIsometric(doc, GetDefaultView3DTypeId(doc));
                        if (view3D != null)
                        {
                            view3D.Name = "Vista 3D Criada pelo App";
                            Logger.LogMessage("Nova vista 3D criada com sucesso");
                        }
                        
                        tx.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Logger.HandleError(ex);
                    TaskDialog.Show("Erro", $"Não foi possível criar uma nova vista 3D: {ex.Message}");
                }
            }
            
            return view3D;
        }
        
        /// <summary>
        /// Obtém o ElementId do tipo de vista 3D padrão
        /// </summary>
        private ElementId GetDefaultView3DTypeId(Document doc)
        {
            // Encontra o tipo de vista 3D padrão
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType));
                
            ViewFamilyType viewFamilyType = collector
                .Cast<ViewFamilyType>()
                .FirstOrDefault(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                
            return viewFamilyType?.Id ?? ElementId.InvalidElementId;
        }
    }
}