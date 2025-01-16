rmdir "Build" /S /Q
mkdir "Build"
dotnet publish "AccountHelperWpf\AccountHelperWpf.csproj" -c Release --framework net9.0-windows --self-contained true
cd Build

:: take first folder in the current directory
for /d %%f in (*) do (
    set "folder=%%f"
    goto :found
)

:found

cd "%folder%"
:: remove all files in the folder
for /d %%d in (*) do (
    rmdir /s /q "%%d"
)

cd ..
powershell Compress-Archive -Path "%folder%\*" -DestinationPath "%folder%.zip"

rmdir "%folder%" /S /Q
rename "%folder%.zip" "AccountHelper v%folder%.zip"