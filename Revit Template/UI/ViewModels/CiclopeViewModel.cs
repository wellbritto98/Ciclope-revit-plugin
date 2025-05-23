using RevitTemplate.UI.ViewModels;
using RevitTemplate.Utils;
using System;
using System.Windows;
using System.Windows.Input;

namespace RevitTemplate.UI.ViewModels
{
    public class CiclopeViewModel : ViewModelBase
    {
        public ICommand AdicionarParametrosCommand { get; }
        public ICommand EnviarParaCiclopeCommand { get; }

        public ICommand AtualizarTiposCommand { get; }

        public CiclopeViewModel()
        {
            AdicionarParametrosCommand = new RelayCommand(x => AdicionarParametros(x));
            EnviarParaCiclopeCommand = new RelayCommand(x => EnviarParaCiclope(x));
            AtualizarTiposCommand = new RelayCommand(x => AtualizarTipos(x));
        }

        private void AdicionarParametros(object parameter)
        {
            throw new NotImplementedException("Funcionalidade 'Adicionar parâmetros' em desenvolvimento");
        }
        private void EnviarParaCiclope(object parameter)
        {
            throw new NotImplementedException("Funcionalidade 'Enviar para Ciclope' em desenvolvimento");
        }
        private void AtualizarTipos(object parameter)
        {
            throw new NotImplementedException("Funcionalidade 'Atualizar tipos' em desenvolvimento");
        }
    }
}
