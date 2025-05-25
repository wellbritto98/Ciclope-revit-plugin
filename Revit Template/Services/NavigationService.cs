using System;
using System.Windows;
using System.Windows.Controls;
using RevitTemplate.UI.Views;

namespace RevitTemplate.Services
{
    /// <summary>
    /// Serviço de navegação para plugins do Revit
    /// Gerencia a navegação entre páginas sem depender de Application.Current
    /// </summary>
    public static class NavService
    {
        private static CiclopeWindow _mainWindow;

        /// <summary>
        /// Registra a janela principal para navegação
        /// </summary>
        /// <param name="window">Janela principal da aplicação</param>
        public static void RegisterMainWindow(CiclopeWindow window)
        {
            _mainWindow = window;
            LogService.LogDebug("NavigationService: Janela principal registrada");
        }

        /// <summary>
        /// Navega para uma página específica
        /// </summary>
        /// <param name="page">Página de destino</param>
        /// <returns>True se a navegação foi bem-sucedida, False caso contrário</returns>
        public static bool NavigateToPage(object page)
        {
            try
            {
                if (_mainWindow != null)
                {
                    _mainWindow.NavigateToPage(page);
                    LogService.LogDebug($"NavigationService: Navegação para {page.GetType().Name} realizada com sucesso");
                    return true;
                }
                else
                {
                    LogService.LogError("NavigationService: Janela principal não está registrada");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"NavigationService: Erro durante navegação - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tenta navegar para uma página usando o controle pai atual como fallback
        /// </summary>
        /// <param name="currentControl">Controle atual (Page, UserControl, etc.)</param>
        /// <param name="page">Página de destino</param>
        /// <returns>True se a navegação foi bem-sucedida, False caso contrário</returns>
        public static bool NavigateFromControl(FrameworkElement currentControl, object page)
        {
            try
            {
                // Primeiro tenta usar a janela registrada
                if (NavigateToPage(page))
                {
                    return true;
                }

                // Fallback: busca pela janela pai do controle atual
                var parentWindow = Window.GetWindow(currentControl);
                if (parentWindow is CiclopeWindow ciclopeWindow)
                {
                    ciclopeWindow.NavigateToPage(page);
                    LogService.LogDebug($"NavigationService: Navegação via fallback para {page.GetType().Name} realizada com sucesso");
                    return true;
                }

                LogService.LogError("NavigationService: Não foi possível encontrar uma janela adequada para navegação");
                return false;
            }
            catch (Exception ex)
            {
                LogService.LogError($"NavigationService: Erro durante navegação via controle - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Limpa o registro da janela principal
        /// </summary>
        public static void Unregister()
        {
            _mainWindow = null;
            LogService.LogDebug("NavigationService: Janela principal desregistrada");
        }

        /// <summary>
        /// Verifica se o serviço está devidamente inicializado
        /// </summary>
        /// <returns>True se a janela principal está registrada</returns>
        public static bool IsInitialized => _mainWindow != null;
    }
}
