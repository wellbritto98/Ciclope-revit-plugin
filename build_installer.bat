@echo off
echo ========================================
echo    GERADOR DE INSTALADOR
echo ========================================
echo.

echo 1. Executando publish...
call publish.bat

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERRO: Falha no publish!
    pause
    exit /b 1
)

echo.
echo 2. Verificando se o Inno Setup está instalado...

set INNO_COMPILER="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist %INNO_COMPILER% (
    set INNO_COMPILER="C:\Program Files\Inno Setup 6\ISCC.exe"
)

if not exist %INNO_COMPILER% (
    echo.
    echo ERRO: Inno Setup não encontrado!
    echo Por favor, instale o Inno Setup 6:
    echo https://jrsoftware.org/isdl.php
    echo.
    pause
    exit /b 1
)

echo.
echo 3. Gerando instalador...
%INNO_COMPILER% installer_config.iss

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERRO: Falha na geração do instalador!
    pause
    exit /b 1
)

echo.
echo ========================================
echo    INSTALADOR GERADO COM SUCESSO!
echo ========================================
echo.
echo Localização: installer_output\RevitTemplate_Setup.exe
echo.
echo Próximos passos:
echo 1. Teste o instalador em um ambiente limpo
echo 2. Verifique se todos os arquivos foram instalados
echo 3. Teste o addin no Revit
echo.
pause
