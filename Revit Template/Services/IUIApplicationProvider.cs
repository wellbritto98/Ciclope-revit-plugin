using Autodesk.Revit.UI;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Interface for providing UIApplication context.
    /// </summary>
    public interface IUIApplicationProvider
    {
        /// <summary>
        /// Gets the current UIApplication instance.
        /// </summary>
        UIApplication UIApplication { get; }

        /// <summary>
        /// Sets the current UIApplication instance.
        /// </summary>
        /// <param name="uiApplication">The UIApplication to set.</param>
        void SetUIApplication(UIApplication uiApplication);
    }
}
