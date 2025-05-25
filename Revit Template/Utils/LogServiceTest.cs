using System;
using RevitTemplate.Services;

namespace RevitTemplate.Utils
{
    /// <summary>
    /// Classe de teste para demonstrar o uso do LogService
    /// </summary>
    public static class LogServiceTest
    {
        /// <summary>
        /// Executa testes do LogService para verificar funcionalidade
        /// </summary>
        public static void RunTests()
        {
            try
            {
                LogService.LogInfo("=== Iniciando testes do LogService ===");
                
                // Teste de diferentes tipos de log
                LogService.LogInfo("Este é um log de informação");
                LogService.LogWarning("Este é um log de warning");
                LogService.LogError("Este é um log de erro (teste)");
                LogService.LogDebug("Este é um log de debug");
                
                // Teste com variáveis
                string usuario = "TestUser";
                int quantidade = 42;
                LogService.LogInfo($"Usuário {usuario} processou {quantidade} elementos");
                
                // Teste de log formatado
                LogService.LogDebug($"Timestamp atual: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                
                LogService.LogInfo("=== Testes do LogService concluídos ===");
            }
            catch (Exception ex)
            {
                LogService.LogError($"Erro durante teste: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Simula logs de uma operação típica do Revit
        /// </summary>
        public static void SimulateRevitOperation()
        {
            LogService.LogInfo("Iniciando operação do Revit");
            LogService.LogDebug("Verificando documento ativo");
            
            System.Threading.Thread.Sleep(500); // Simula processamento
            
            LogService.LogInfo("Coletando elementos selecionados");
            LogService.LogDebug("Encontrados 15 elementos");
            
            System.Threading.Thread.Sleep(300);
            
            LogService.LogWarning("Alguns elementos não possuem parâmetros CICLOPE");
            LogService.LogInfo("Adicionando parâmetros CICLOPE aos elementos");
            
            System.Threading.Thread.Sleep(700);
            
            LogService.LogInfo("Operação concluída com sucesso");
            LogService.LogDebug("Tempo total: 1.5 segundos");
        }
    }
}
