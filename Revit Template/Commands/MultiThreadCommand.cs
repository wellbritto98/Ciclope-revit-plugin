using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTemplate.Commands
{
    /// <summary>
    /// External command for launching the application in a separate thread.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class MultiThreadCommand : IExternalCommand
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
                // Call the application to show the main window in a separate thread
                RevitApp.Instance.ShowMainWindowSeparateThread(commandData.Application);
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
