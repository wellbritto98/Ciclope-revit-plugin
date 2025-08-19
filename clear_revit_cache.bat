@echo off
echo Limpando cache do Revit...
echo.

REM Fechar o Revit se estiver aberto
taskkill /f /im Revit.exe 2>nul
timeout /t 2 /nobreak >nul

REM Limpar cache do Revit
echo Removendo cache do Revit...
if exist "%APPDATA%\Autodesk\Revit\Autodesk Revit 2024\Journals" (
    rmdir /s /q "%APPDATA%\Autodesk\Revit\Autodesk Revit 2024\Journals"
    echo Cache removido.
) else (
    echo Cache não encontrado.
)

REM Limpar cache de addins
echo Removendo cache de addins...
if exist "%APPDATA%\Autodesk\Revit\Addins\2024\RevitTemplate.dll" (
    del /q "%APPDATA%\Autodesk\Revit\Addins\2024\RevitTemplate.dll"
    echo Addin anterior removido.
) else (
    echo Addin anterior não encontrado.
)

echo.
echo Cache limpo! Agora você pode:
echo 1. Compilar o projeto
echo 2. Copiar os arquivos para a pasta de addins
echo 3. Abrir o Revit
echo.
pause
