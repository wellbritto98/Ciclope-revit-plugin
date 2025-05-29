using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RevitTemplate.Models
{
    /// <summary>
    /// Represents summarized information about a Revit element or group of similar elements
    /// </summary>
    public class ElementInfo : INotifyPropertyChanged
    {
        // Evento de notificação de alteração de propriedades
        public event PropertyChangedEventHandler PropertyChanged;

        // Método para notificar alterações
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Método auxiliar para definir valores de propriedades com notificação
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Unique identifier for grouping elements
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// Name of the family element
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Category of the element
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Type name of the element (family instance)
        /// </summary>
        public string FamilyType { get; set; }        

        /// <summary>
        /// Total area of the element(s) in square meters
        /// </summary>
        public double Area { get; set; }

        /// <summary>
        /// Total volume of the element(s) in cubic meters
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Total perimeter of the element(s) in meters
        /// </summary>
        public double Perimeter { get; set; }

        /// <summary>
        /// Formatted area for display (includes units)
        /// </summary>
        public string AreaFormatted => Area > 0 ? $"{Math.Round(Area, 2)} m²" : "N/A";

        /// <summary>
        /// Formatted volume for display (includes units)
        /// </summary>
        public string VolumeFormatted => Volume > 0 ? $"{Math.Round(Volume, 2)} m³" : "N/A";        /// <summary>
        /// Formatted perimeter for display (includes units)
        /// </summary>
        public string PerimeterFormatted => Perimeter > 0 ? $"{Math.Round(Perimeter, 2)} m" : "N/A";
    
       
    }

}
