using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.Core.Services;
using RevitTemplate.Utils;

namespace RevitTemplate.Infrastructure
{
    public class UpdateFamilyTypesGridEventHandler : RevitEventHandler<object>
    {
        

        public UpdateFamilyTypesGridEventHandler()
        {
        }        protected override void Execute(UIApplication app, object parameter)
        {
            try
            {
                // Set the UIApplication in the singleton instance
                UIApplicationProvider.Instance.SetUIApplication(app);
                
                IRevitDocumentService service = new RevitDocumentService(UIApplicationProvider.Instance);
                var families = service.GetAllFamilyInstances();
                
                Logger.LogMessage("Tabela de tipos de família atualizada.");
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
                TaskDialog.Show("Erro ao atualizar tabela", $"{ex.Message}\n\nDetalhes: {ex}");
            }
        }
    }
}
