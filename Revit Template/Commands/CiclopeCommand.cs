using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitTemplate.UI.Views;
using System.Windows;
using RevitTemplate.UI.Views;
using Autodesk.Revit.Attributes;

namespace RevitTemplate.Commands
{
    /// <summary>
    /// External command for launching the application in a single thread.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CiclopeCommand : IExternalCommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="commandData">The command data.</param>
        /// <param name="message">The message.</param>
        /// <param name="elements">The elements.</param>
        /// <returns>The result of the command.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Call the application to show the CICLOPE window
                RevitApp.Instance.ShowCiclopeWindow(commandData.Application);
                return Result.Succeeded;
            }
            catch (System.Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
