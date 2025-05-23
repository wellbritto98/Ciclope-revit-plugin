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
        /// Name of the element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Category of the element
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Type name of the element
        /// </summary>
        public string Type { get; set; }        

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
        /// Quantity of instances of this element
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Lista de IDs de elementos do Revit neste grupo
        /// </summary>
        public List<int> RevitElementIds { get; set; } = new List<int>();        

        /// <summary>
        /// Formatted area for display (includes units)
        /// </summary>
        public string AreaFormatted => Area > 0 ? $"{Math.Round(Area, 2)} m²" : "N/A";

        /// <summary>
        /// Formatted volume for display (includes units)
        /// </summary>
        public string VolumeFormatted => Volume > 0 ? $"{Math.Round(Volume, 2)} m³" : "N/A";

        /// <summary>
        /// Formatted perimeter for display (includes units)
        /// </summary>
        public string PerimeterFormatted => Perimeter > 0 ? $"{Math.Round(Perimeter, 2)} m" : "N/A";

        #region Parametros CICLOPE

        private string _valorBase;
        /// <summary>
        /// Parâmetro Base CICLOPE
        /// </summary>
        public string ValorBase
        {
            get => _valorBase;
            set
            {
                if (SetProperty(ref _valorBase, value))
                {
                    // Quando o valor for alterado, disparar evento para atualização do valor em todos os elementos
                    ParametroCiclopeAlterado?.Invoke(this, new ParametroCiclopeEventArgs("Base", value, RevitElementIds));
                }
            }
        }

        private string _valorEstado;
        /// <summary>
        /// Parâmetro Estado CICLOPE
        /// </summary>
        public string ValorEstado
        {
            get => _valorEstado;
            set
            {
                if (SetProperty(ref _valorEstado, value))
                {
                    // Quando o valor for alterado, disparar evento para atualização do valor em todos os elementos
                    ParametroCiclopeAlterado?.Invoke(this, new ParametroCiclopeEventArgs("Estado", value, RevitElementIds));
                }
            }
        }

        private string _valorCodigo;
        /// <summary>
        /// Parâmetro Código CICLOPE
        /// </summary>
        public string ValorCodigo
        {
            get => _valorCodigo;
            set
            {
                if (SetProperty(ref _valorCodigo, value))
                {
                    // Quando o valor for alterado, disparar evento para atualização do valor em todos os elementos
                    ParametroCiclopeAlterado?.Invoke(this, new ParametroCiclopeEventArgs("Codigo", value, RevitElementIds));
                }
            }
        }

        /// <summary>
        /// Evento disparado quando um parâmetro CICLOPE é alterado para atualizar todos os elementos do grupo
        /// </summary>
        public static event EventHandler<ParametroCiclopeEventArgs> ParametroCiclopeAlterado;

        #endregion
    }

    /// <summary>
    /// Argumentos do evento quando um parâmetro CICLOPE é alterado
    /// </summary>
    public class ParametroCiclopeEventArgs : EventArgs
    {
        /// <summary>
        /// Nome do parâmetro (Base, Estado, Codigo)
        /// </summary>
        public string NomeParametro { get; }

        /// <summary>
        /// Novo valor do parâmetro
        /// </summary>
        public string Valor { get; }

        /// <summary>
        /// IDs dos elementos do Revit que devem ser atualizados
        /// </summary>
        public List<int> ElementoIds { get; }

        public ParametroCiclopeEventArgs(string nomeParametro, string valor, List<int> elementoIds)
        {
            NomeParametro = nomeParametro;
            Valor = valor;
            ElementoIds = new List<int>(elementoIds); // Cria uma cópia da lista para evitar alterações externas
        }
    }
}
