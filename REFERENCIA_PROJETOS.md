# Configuração Multi-Project - Revit Template e SignalR Worker

## Visão Geral

Este documento explica como foi configurada a solução multi-project entre o **Revit Template** (.NET Framework 4.8) e o **SignalR Worker** (.NET 8.0) sem referência direta entre projetos.

## Estrutura dos Projetos

### Revit Template
- **Framework**: .NET Framework 4.8
- **Tipo**: Library (DLL)
- **GUID**: `{80671941-95DE-4707-9C40-758E89CBD063}`
- **Propósito**: Add-in do Revit que fornece interface WPF

### SignalR Worker
- **Framework**: .NET 8.0
- **Tipo**: Worker Service (Executável)
- **GUID**: `{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}`
- **Propósito**: Serviço de comunicação SignalR

## Configuração Multi-Project

### 1. Arquivo de Solução (.sln)
Criado o arquivo `RevitTemplate.sln` que gerencia ambos os projetos independentemente:

```xml
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "RevitTemplate", "Revit Template\Revit Template.csproj", "{80671941-95DE-4707-9C40-758E89CBD063}"
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SignalRWorker", "SignalRWorker\SignalRWorker.csproj", "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
```

### 2. Projetos Independentes
- **Sem referência direta**: Devido à incompatibilidade de frameworks (.NET 4.8 vs .NET 8.0)
- **Compilação separada**: Cada projeto é compilado independentemente
- **Gerenciamento na inicialização**: O SignalR Worker é compilado quando necessário

### 3. Post-Build Event Simplificado
Configurado apenas para copiar os arquivos do Revit Template:

```xml
<PostBuildEvent>
    Copy "$(TargetDir)*.dll" "$(AppData)\Autodesk\Revit\Addins\2024"
    Copy "$(TargetDir)RevitTemplate.addin" "$(AppData)\Autodesk\Revit\Addins\2024"
</PostBuildEvent>
```

## Como Funciona

### Compilação
1. O Visual Studio/MSBuild compila os projetos independentemente
2. O Revit Template é compilado primeiro (projeto principal)
3. O SignalR Worker é compilado quando necessário (na inicialização)

### Execução
1. O Revit Template carrega como add-in do Revit
2. Na inicialização, o SignalR Worker é compilado se necessário
3. O WorkerServiceProxy localiza e executa o SignalR Worker
4. A comunicação é feita via pipes nomeados

### Localização do SignalR Worker
O `WorkerServiceProxy` procura o SignalR Worker em:
1. **Registro**: `HKCU\Software\Revit Template\SignalRWorkerPath`
2. **Arquivo de Configuração**: `%APPDATA%\RevitTemplate\config.json`
3. **Caminho Padrão**: `%PROGRAMFILES%\Revit Template\SignalRWorker\SignalRWorker.exe`

## Vantagens desta Configuração

### ✅ Desenvolvimento
- **Compilação Independente**: Cada projeto pode ser compilado separadamente
- **Flexibilidade**: O SignalR Worker é compilado apenas quando necessário
- **Compatibilidade**: Resolve a incompatibilidade entre .NET 4.8 e .NET 8.0

### ✅ Distribuição
- **Instalador Único**: O Inno Setup instala ambos os componentes
- **Dependências Gerenciadas**: As dependências são resolvidas automaticamente
- **Configuração Automática**: Os caminhos são configurados automaticamente

### ✅ Manutenção
- **Projetos Independentes**: Mudanças em um projeto não afetam o outro
- **Versionamento**: Ambos os projetos podem ser versionados independentemente
- **Build Flexível**: Compilação sob demanda do SignalR Worker

## Comandos de Build

### Visual Studio
```bash
# Abrir a solução
RevitTemplate.sln

# Build da solução completa
Build > Build Solution (Ctrl+Shift+B)
```

### Linha de Comando
```bash
# Build da solução completa
dotnet build RevitTemplate.sln -c Release

# Build individual dos projetos
dotnet build "Revit Template/Revit Template.csproj" -c Release
dotnet build SignalRWorker/SignalRWorker.csproj -c Release
```

### Script Automatizado
```bash
# Executar o script batch
build_installer.bat
```

## Solução de Problemas

### Erro: "Project reference not found"
- Verifique se o GUID do projeto está correto
- Certifique-se de que ambos os projetos estão na mesma solução

### Erro: "SignalR Worker not found"
- Verifique se o Post-Build Event está copiando os arquivos
- Confirme se o caminho no registro/arquivo de configuração está correto

### Erro: "Build dependency failed"
- Verifique se o SignalR Worker compila independentemente
- Confirme se todas as dependências do .NET 8.0 estão instaladas

## Próximos Passos

1. **Testar a Compilação**: Execute `build_installer.bat` para testar
2. **Verificar Referências**: Abra a solução no Visual Studio
3. **Testar Execução**: Execute o add-in no Revit
4. **Ajustar Configurações**: Modifique caminhos se necessário
