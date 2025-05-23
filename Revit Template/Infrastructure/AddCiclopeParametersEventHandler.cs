using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using RevitTemplate.Core.Services;
using RevitTemplate.Utils;
using System;
using System.IO;
using System.Collections.Generic;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// Event handler to add CICLOPE shared parameters to specific elements in the project.
    /// </summary>
    public class AddCiclopeParametersEventHandler : RevitEventHandler<Tuple<string, List<int>>>
    {
        protected override void Execute(UIApplication app, Tuple<string, List<int>> parameters)
        {
            try
            {
                string sharedParameterFilePath = parameters.Item1;
                List<int> elementIds = parameters.Item2;

                // Log the path for diagnostics
                Logger.LogMessage($"[AddCiclopeParametersEventHandler] Caminho recebido: '{sharedParameterFilePath}'");
                Logger.LogMessage($"[AddCiclopeParametersEventHandler] Elementos recebidos: {elementIds?.Count ?? 0}");

                if (string.IsNullOrWhiteSpace(sharedParameterFilePath) || !File.Exists(sharedParameterFilePath))
                {
                    Logger.LogMessage($"[AddCiclopeParametersEventHandler] Arquivo não encontrado: '{sharedParameterFilePath}'");
                    throw new FileNotFoundException(
                        $"O arquivo de parâmetros compartilhados não foi encontrado: '{sharedParameterFilePath}'. " +
                        "Verifique o caminho e tente novamente.");
                }

                var doc = app.ActiveUIDocument.Document;
                var service = new FamilyParameterService(doc, app.Application, sharedParameterFilePath);
                
                if (elementIds != null && elementIds.Count > 0)
                {
                    // Adiciona parâmetros apenas aos elementos especificados
                    service.AddCiclopeParametersToSpecificElements(elementIds);
                    Logger.LogMessage($"CICLOPE parameters added to {elementIds.Count} selected elements.");
                }
                else
                {
                    // Adiciona parâmetros a todas as famílias se nenhum elemento for especificado
                    service.AddCiclopeParametersToAllFamilies();
                    Logger.LogMessage("CICLOPE parameters added to all families.");
                }
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Erro ao adicionar parâmetros", $"{ex.Message}\n\nDetalhes: {ex}");
            }
        }
    }
}
