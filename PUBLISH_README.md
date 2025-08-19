# Sistema de Publish - Revit Template Addin

Este documento explica como usar o sistema de publish para gerar pacotes de instalação do addin.

## Arquivos Criados

### 1. `publish.bat`
Script principal para gerar o pacote de publish.

**Como usar:**
```bash
publish.bat
```

**O que faz:**
- Compila o projeto em modo Release
- Gera a pasta `publish/` com todos os arquivos necessários
- Cria estrutura organizada para o instalador

### 2. `build_installer.bat`
Script para gerar o instalador usando Inno Setup.

**Pré-requisitos:**
- Inno Setup 6 instalado (https://jrsoftware.org/isdl.php)

**Como usar:**
```bash
build_installer.bat
```

**O que faz:**
- Executa o publish automaticamente
- Gera o instalador usando Inno Setup
- Cria o arquivo `installer_output/RevitTemplate_Setup.exe`

### 3. `installer_config.iss`
Configuração do Inno Setup para o instalador.

## Estrutura Gerada

Após executar `publish.bat`, você terá:

```
publish/
├── Addins/
│   ├── *.dll (arquivos do addin principal)
│   ├── RevitTemplate.addin
│   └── Worker/
│       └── SignalRWorker.exe
├── License.txt
├── README.md (se existir)
└── INSTALACAO.txt
```

## Fluxo de Trabalho

### Para Desenvolvimento:
1. Use `publish.bat` para gerar o pacote
2. Teste manualmente copiando os arquivos para a pasta de addins
3. Verifique se tudo funciona corretamente

### Para Distribuição:
1. Execute `build_installer.bat`
2. Teste o instalador gerado
3. Distribua o arquivo `installer_output/RevitTemplate_Setup.exe`

## Configurações do MSBuild

O arquivo `Revit Template.csproj` contém targets personalizados:

### `CreatePublishPackage`
- Executa automaticamente após build em Release
- Cria a pasta `publish/` com estrutura organizada
- Copia todos os arquivos necessários
- Gera arquivo de instruções

### `CopySignalRWorkerFiles`
- Copia arquivos para pasta de addins (Debug) ou bin/release (Release)
- Separa arquivos do Worker em pasta específica

## Personalização

### Modificar versão do instalador:
Edite `installer_config.iss`:
```ini
AppVersion=1.0.0
```

### Adicionar arquivos ao publish:
Edite o target `CreatePublishPackage` no `.csproj`:
```xml
<ItemGroup>
  <PublishExtraFiles Include="caminho\para\arquivo" />
</ItemGroup>
<Copy SourceFiles="@(PublishExtraFiles)" DestinationFolder="$(PublishDir)" />
```

### Modificar estrutura de pastas:
Altere as propriedades no target:
```xml
<PublishDir>$(ProjectDir)publish\</PublishDir>
<PublishAddinsDir>$(PublishDir)Addins\</PublishAddinsDir>
```

## Troubleshooting

### Erro: "Inno Setup não encontrado"
- Instale o Inno Setup 6
- Verifique se está no PATH ou nos caminhos padrão

### Erro: "SignalRWorker.exe não encontrado"
- Verifique se o SignalRWorker foi compilado corretamente
- Execute `dotnet build SignalRWorker/SignalRWorker.csproj -c Release`

### Arquivos faltando no publish
- Verifique se todos os arquivos foram copiados corretamente
- Execute `publish.bat` novamente

## Notas Importantes

1. **Sempre compile em Release** para o publish
2. **Teste o instalador** em ambiente limpo
3. **Verifique permissões** de escrita na pasta de addins
4. **Mantenha a estrutura** de pastas intacta
5. **O SignalRWorker é auto-contido** - não precisa de .NET 8.0 instalado
