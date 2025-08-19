@echo off
echo ========================================
echo    PUBLISH REVIT TEMPLATE ADDIN
echo ========================================
echo.

echo Compilando em modo Release...
dotnet build "Revit Template\Revit Template.csproj" -c Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERRO: Falha na compilação!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    PUBLISH CONCLUIDO COM SUCESSO!
echo ========================================
echo.
echo Localização do pacote: publish\
echo.
echo Estrutura criada:
echo   publish\
echo   ├── Addins\
echo   │   ├── *.dll
echo   │   ├── RevitTemplate.addin
echo   │   └── Worker\
echo   │       └── SignalRWorker.exe
echo   ├── License.txt
echo   ├── README.md (se existir)
echo   └── INSTALACAO.txt
echo.
echo Próximos passos:
echo 1. Verifique a pasta 'publish'
echo 2. Use os arquivos para criar o instalador
echo 3. Teste a instalação em um ambiente limpo
echo.
pause
