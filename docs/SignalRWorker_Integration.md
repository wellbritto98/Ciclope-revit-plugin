# Integração SignalRWorker com Revit Template

## Visão Geral

Este documento explica como o SignalRWorker é integrado com o Revit Template e como os arquivos são copiados automaticamente para a pasta de addins do Revit.

## Estrutura da Solução

- **Revit Template**: Projeto .NET Framework 4.8 (addin do Revit)
- **SignalRWorker**: Projeto .NET 8.0 (Worker Service independente)

## Configuração de Build

### Revit Template (.csproj)

O projeto Revit Template foi configurado para:

1. **Compilar o SignalRWorker primeiro**: Usando um target `BuildSignalRWorker` que executa antes do build principal
2. **Copiar todos os arquivos necessários**: Durante o PostBuildEvent, copia:
   - Arquivos do Revit Template (DLLs e .addin)
   - Todos os arquivos do SignalRWorker (DLLs, EXE, JSONs de configuração)

### SignalRWorker (.csproj)

O projeto SignalRWorker foi simplificado removendo o PostBuildEvent, já que agora é gerenciado pelo Revit Template.

## Fluxo de Execução

1. **Build**: Quando o Revit Template é compilado, o SignalRWorker é compilado automaticamente
2. **Cópia**: Todos os arquivos são copiados para `%AppData%\Autodesk\Revit\Addins\2024\`
3. **Execução**: O WorkerServiceProxy localiza e executa o SignalRWorker.exe na mesma pasta

## WorkerServiceProxy

O `WorkerServiceProxy` foi modificado para:

- Localizar o SignalRWorker.exe no mesmo diretório do addin
- Verificar se o arquivo existe antes de tentar executá-lo
- Definir o diretório de trabalho correto para o processo

## Arquivos Copiados

### Revit Template
- `*.dll` - Bibliotecas do addin
- `RevitTemplate.addin` - Arquivo de configuração do addin

### SignalRWorker
- `*.dll` - Bibliotecas do .NET 8.0
- `SignalRWorker.exe` - Executável principal
- `*.json` - Arquivos de configuração
- `*.deps.json` - Dependências do .NET
- `*.runtimeconfig.json` - Configuração de runtime

## Vantagens desta Abordagem

1. **Separação de responsabilidades**: Cada projeto mantém sua estrutura independente
2. **Compatibilidade**: Não há referências diretas entre projetos com frameworks diferentes
3. **Deploy simplificado**: Todos os arquivos necessários são copiados automaticamente
4. **Manutenibilidade**: Mudanças no SignalRWorker não afetam o Revit Template

## Troubleshooting

### SignalRWorker.exe não encontrado

Se o erro "SignalRWorker.exe não encontrado" aparecer:

1. Verifique se o build do SignalRWorker foi executado com sucesso
2. Confirme se os arquivos foram copiados para a pasta de addins
3. Verifique se o caminho `%AppData%\Autodesk\Revit\Addins\2024\` existe

### Problemas de Runtime

Se o SignalRWorker não iniciar corretamente:

1. Verifique se todos os arquivos .dll e .json foram copiados
2. Confirme se o .NET 8.0 Runtime está instalado
3. Verifique os logs do processo para erros específicos
