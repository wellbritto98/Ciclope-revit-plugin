using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.Core.Services;
using RevitTemplate.Utils;
using RevitTemplate.ViewModels;

namespace RevitTemplate.Infrastructure
{
    public class UpdateFamilyTypesGridEventHandler : RevitEventHandler<object>
    {
        private ParametrosPageViewModel _viewModel;

        public UpdateFamilyTypesGridEventHandler(ParametrosPageViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        protected override void Execute(UIApplication app, object parameter)
        {
            try
            {
                IRevitDocumentService service = new RevitDocumentService(app);
                var families = service.GetAllFamilyInstances();
                _viewModel.UpdateFamilyTypes(families);
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
