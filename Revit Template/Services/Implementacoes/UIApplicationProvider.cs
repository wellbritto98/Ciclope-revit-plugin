using Autodesk.Revit.UI;

namespace RevitTemplate.Core.Services
{
    /// <summary>
    /// Implementation of IUIApplicationProvider for providing UIApplication context.
    /// This is a singleton to ensure the same instance is used throughout the application.
    /// </summary>
    public class UIApplicationProvider : IUIApplicationProvider
    {
        private static UIApplicationProvider _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance of UIApplicationProvider.
        /// </summary>
        public static UIApplicationProvider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new UIApplicationProvider();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the current UIApplication instance.
        /// </summary>
        public UIApplication UIApplication { get; private set; }

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// </summary>
        private UIApplicationProvider()
        {
        }

        /// <summary>
        /// Sets the current UIApplication instance.
        /// </summary>
        /// <param name="uiApplication">The UIApplication to set.</param>
        public void SetUIApplication(UIApplication uiApplication)
        {
            UIApplication = uiApplication;
        }
    }
}
