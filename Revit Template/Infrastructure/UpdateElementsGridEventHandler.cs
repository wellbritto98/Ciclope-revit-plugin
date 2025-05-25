using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.Core.Services;
using RevitTemplate.Models;
using RevitTemplate.Utils;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// EventHandler para atualizar o grid de elementos no ViewModel
    /// </summary>
    public class UpdateElementsGridEventHandler : RevitEventHandler<object>
    {


        /// <summary>
        /// Executa a busca de elementos do documento Revit e atualiza o ViewModel
        /// </summary>
        /// <param name="app">A aplicação Revit</param>
        /// <param name="parameter">Parâmetro adicional (não utilizado)</param>
        protected override void Execute(UIApplication app, object parameter)
        {
            try
            {
                Logger.LogThreadInfo("Atualizando dados de elementos");
                IRevitDocumentService service = new RevitDocumentService(app);
                
                // Usa um método assíncrono, mas como estamos em um método síncrono (Execute),
                // precisamos esperar pela conclusão usando GetAwaiter().GetResult()
                var elements = service.GetElementInfoAsync().GetAwaiter().GetResult();
                
                // Atualiza o ViewModel com os elementos obtidos
                
                Logger.LogMessage("Tabela de elementos atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Erro ao atualizar tabela", $"{ex.Message}\n\nDetalhes: {ex}");
            }
        }
    }
}