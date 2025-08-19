# Instalação do Revit Template

Este documento explica como criar um instalador para o Revit Template usando Inno Setup.

## Pré-requisitos

1. **Inno Setup**: Baixe e instale o Inno Setup de https://jrsoftware.org/isinfo.php
2. **Compilação**: Certifique-se de que ambos os projetos estão compilados em modo Release

## Estrutura de Arquivos Necessária

Antes de executar o script Inno Setup, certifique-se de que você tem a seguinte estrutura de arquivos:

```
revit-wpf-template/
├── Revit Template/
│   ├── bin/Release/
│   │   ├── RevitTemplate.dll
│   │   ├── *.dll (todas as dependências)
│   │   └── Resources/ (todos os recursos)
│   ├── RevitTemplate.addin
│   └── Resources/
├── SignalRWorker/
│   └── bin/Release/net8.0/
│       ├── SignalRWorker.exe
│       └── *.dll (todas as dependências)
├── config.json
├── License.txt
├── RevitTemplate_Setup.iss
└── README_Setup.md
```

## Passos para Compilar o Instalador

1. **Compile os projetos em Release**:
   ```bash
   # Para o Revit Template
   cd "Revit Template"
   dotnet build -c Release
   
   # Para o SignalR Worker
   cd "../SignalRWorker"
   dotnet build -c Release
   ```

2. **Execute o Inno Setup Compiler**:
   - Abra o Inno Setup Compiler
   - Abra o arquivo `RevitTemplate_Setup.iss`
   - Clique em "Build" → "Compile" (ou pressione Ctrl+F9)

3. **Localize o instalador**:
   - O arquivo executável será criado em `Output/RevitTemplate_Setup.exe`

## O que o Instalador Faz

### Instalação de Arquivos
- **SignalR Worker**: Instalado em `%PROGRAMFILES%\Revit Template\SignalRWorker\`
- **Revit Template**: Instalado em `%APPDATA%\Autodesk\Revit\Addins\2024\`
- **Recursos**: Copiados para o diretório do add-in
- **Configuração**: Criado em `%APPDATA%\RevitTemplate\config.json`

### Registro do Windows
- Registra o add-in no Revit 2024
- Armazena o caminho do SignalR Worker no registro

### Dependências
- Verifica se o .NET Framework 4.8 está instalado
- Verifica se o .NET 8.0 Runtime está instalado
- Instala automaticamente o .NET 8.0 Runtime se necessário

### Permissões
- Define permissões adequadas para o diretório do add-in
- Permite que usuários acessem os arquivos necessários

## Personalização

### Alterar Versão
Edite a linha no arquivo `RevitTemplate_Setup.iss`:
```pascal
#define MyAppVersion "1.0.0"
```

### Alterar Nome da Aplicação
Edite a linha no arquivo `RevitTemplate_Setup.iss`:
```pascal
#define MyAppName "Revit Template"
```

### Adicionar Mais Idiomas
Adicione na seção `[Languages]`:
```pascal
Name: "espanol"; MessagesFile: "compiler:Languages\Spanish.isl"
```

### Alterar Ícone
Substitua o arquivo `Resources\revitIcon.ico` ou altere a linha:
```pascal
SetupIconFile=Resources\revitIcon.ico
```

## Solução de Problemas

### Erro: "SignalR Worker não encontrado"
- Verifique se o SignalR Worker foi compilado em Release
- Verifique se o arquivo `config.json` foi criado corretamente
- Verifique as permissões do diretório de instalação

### Erro: "Add-in não aparece no Revit"
- Verifique se o arquivo `.addin` foi copiado corretamente
- Verifique se o registro foi criado corretamente
- Reinicie o Revit após a instalação

### Erro: ".NET 8.0 Runtime não encontrado"
- O instalador tentará baixar automaticamente
- Se falhar, baixe manualmente de https://dotnet.microsoft.com/download/dotnet/8.0

## Desinstalação

O instalador cria uma entrada no "Adicionar/Remover Programas" que:
- Remove todos os arquivos instalados
- Remove as entradas do registro
- Limpa as configurações

## Suporte

Para problemas com o instalador, verifique:
1. Logs do Inno Setup (F9 durante a compilação)
2. Logs do Windows Event Viewer
3. Permissões de administrador
