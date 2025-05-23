using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using RevitTemplate.Utils;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// Event handler para preencher os parâmetros CICLOPE nos elementos Revit
    /// </summary>
    public class FillCiclopeParametersEventHandler : RevitEventHandler<Tuple<string, string, List<int>>>
    {
        protected override void Execute(UIApplication app, Tuple<string, string, List<int>> parameters)
        {
            try
            {
                // Descompactar os parâmetros
                string paramName = parameters.Item1;   // Nome do parâmetro (Base, Estado, Codigo)
                string value = parameters.Item2;       // Valor a ser atribuído
                List<int> elementIds = parameters.Item3;  // Lista de IDs dos elementos

                // Log para diagnóstico
                Logger.LogMessage($"[FillCiclopeParametersEventHandler] Preenchendo parâmetro '{paramName}' com valor '{value}' em {elementIds?.Count ?? 0} elementos");

                if (elementIds == null || elementIds.Count == 0)
                {
                    Logger.LogMessage("[FillCiclopeParametersEventHandler] Nenhum elemento para atualizar");
                    return;
                }

                var doc = app.ActiveUIDocument.Document;
                
                // Iniciar transação para modificar os elementos
                using (Transaction t = new Transaction(doc, $"Atualizar parâmetro {paramName}"))
                {
                    t.Start();

                    int elementsUpdated = 0;

                    // Atualizar cada elemento
                    foreach (int id in elementIds)
                    {
                        ElementId elementId = new ElementId(id);
                        Element element = doc.GetElement(elementId);
                        
                        if (element != null)
                        {
                            // Obter o parâmetro específico pelo nome
                            Parameter parameter = element.LookupParameter(paramName);
                            
                            if (parameter != null && parameter.StorageType == StorageType.String)
                            {
                                // Definir o valor do parâmetro
                                parameter.Set(value);
                                elementsUpdated++;
                            }
                        }
                    }

                    t.Commit();
                    Logger.LogMessage($"[FillCiclopeParametersEventHandler] {elementsUpdated} elementos atualizados com sucesso");
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Erro ao preencher parâmetros", $"{ex.Message}\n\nDetalhes: {ex}");
            }
        }
    }
}
