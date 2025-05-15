using Autodesk.Revit.UI;
using RevitTemplate.Utils;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// Event handler for operations that require a string parameter.
    /// </summary>
    public class StringParameterEventHandler : RevitEventHandler<string>
    {
        /// <summary>
        /// Executes the event handler with the specified string parameter.
        /// </summary>
        /// <param name="app">The Revit UI application.</param>
        /// <param name="parameter">The string parameter passed to the event handler.</param>
        protected override void Execute(UIApplication app, string parameter)
        {
            // Log the parameter
            Logger.LogMessage($"String Parameter Event Handler executed with parameter: {parameter}");
            
            // Show a dialog with the parameter
            TaskDialog.Show("External Event", parameter);
        }
    }
}
