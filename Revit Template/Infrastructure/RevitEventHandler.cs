using Autodesk.Revit.UI;
using RevitTemplate.Utils;
using System;

namespace RevitTemplate.Infrastructure
{
    /// <summary>
    /// Generic class for handling Revit external events.
    /// </summary>
    /// <typeparam name="T">The type of argument to pass to the event handler.</typeparam>
    public abstract class RevitEventHandler<T> : IExternalEventHandler
    {
        private readonly object _lock = new object();
        private T _parameter;
        private readonly ExternalEvent _externalEvent;

        /// <summary>
        /// Initializes a new instance of the RevitEventHandler class.
        /// </summary>
        protected RevitEventHandler()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        /// <summary>
        /// Gets the name of the event handler.
        /// </summary>
        /// <returns>The name of the event handler.</returns>
        public string GetName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Executes the event handler.
        /// </summary>
        /// <param name="app">The Revit UI application.</param>
        public void Execute(UIApplication app)
        {
            try
            {
                T parameter;
                lock (_lock)
                {
                    parameter = _parameter;
                    _parameter = default;
                }

                Execute(app, parameter);
            }
            catch (Exception ex)
            {
                Logger.HandleError(ex);
            }
        }

        /// <summary>
        /// Raises the external event with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the event handler.</param>
        public void Raise(T parameter)
        {
            lock (_lock)
            {
                _parameter = parameter;
            }

            _externalEvent.Raise();
        }

        /// <summary>
        /// Executes the event handler with the specified parameter.
        /// </summary>
        /// <param name="app">The Revit UI application.</param>
        /// <param name="parameter">The parameter passed to the event handler.</param>
        protected abstract void Execute(UIApplication app, T parameter);
    }
}
