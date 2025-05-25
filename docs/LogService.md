# LogService - Sistema de Logs

## Descrição
O `LogService` é um serviço estático que permite registrar logs em tempo real na aplicação. Os logs são exibidos na `LogPage` com diferentes níveis de prioridade e cores.

## Como Usar

### Importar o namespace
```csharp
using RevitTemplate.Services;
```

### Métodos Disponíveis

#### LogInfo(string message)
Para informações gerais:
```csharp
LogService.LogInfo("Operação concluída com sucesso");
LogService.LogInfo($"Usuário {username} logado");
```

#### LogError(string message)
Para erros:
```csharp
LogService.LogError("Falha na conexão com o servidor");
LogService.LogError($"Erro ao processar arquivo: {ex.Message}");
```

#### LogWarning(string message)
Para avisos:
```csharp
LogService.LogWarning("Configuração não encontrada, usando padrão");
LogService.LogWarning("Versão do Revit não é a recomendada");
```

#### LogDebug(string message)
Para informações de debug:
```csharp
LogService.LogDebug("Entrando no método ProcessData()");
LogService.LogDebug($"Parâmetros recebidos: {param1}, {param2}");
```

#### ClearLogs()
Para limpar todos os logs:
```csharp
LogService.ClearLogs();
```

## Características

- **Thread-Safe**: Pode ser usado de qualquer thread
- **Tempo Real**: Logs aparecem instantaneamente na LogPage
- **Auto-Scroll**: A tela de logs faz scroll automático para as novas mensagens
- **Cores Diferenciadas**: Cada tipo de log tem uma cor específica
- **Timestamp**: Cada log inclui horário de criação

## Níveis de Log e Cores

| Nível   | Cor     | Uso                                    |
|---------|---------|----------------------------------------|
| INFO    | Verde   | Informações gerais e sucessos         |
| ERROR   | Vermelho| Erros que precisam de atenção         |
| WARNING | Laranja | Avisos e situações não ideais         |
| DEBUG   | Azul    | Informações técnicas para debug       |

## Exemplo Completo

```csharp
public class ExampleService
{
    public void ProcessRevitElements()
    {
        LogService.LogInfo("Iniciando processamento dos elementos");
        
        try
        {
            LogService.LogDebug("Obtendo elementos selecionados");
            var elements = GetSelectedElements();
            
            if (elements.Count == 0)
            {
                LogService.LogWarning("Nenhum elemento selecionado");
                return;
            }
            
            LogService.LogInfo($"Processando {elements.Count} elementos");
            
            foreach (var element in elements)
            {
                ProcessElement(element);
                LogService.LogDebug($"Elemento {element.Id} processado");
            }
            
            LogService.LogInfo("Processamento concluído com sucesso");
        }
        catch (Exception ex)
        {
            LogService.LogError($"Erro durante processamento: {ex.Message}");
            throw;
        }
    }
}
```

## Navegação para LogPage

Para navegar para a LogPage programaticamente:

```csharp
// De dentro de uma Page
var mainWindow = Window.GetWindow(this) as CiclopeWindow;
mainWindow?.NavigateToPage(new LogPage());

// De qualquer lugar com acesso à janela principal
ciclopeWindow.NavigateToPage(new LogPage());
```
