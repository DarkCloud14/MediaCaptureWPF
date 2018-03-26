@echo off
setlocal enableextensions

set VERSION=1.0.0

if exist "%ProgramFiles%\MSBuild\12.0\Bin\msbuild.exe" (
    set BUILD="%ProgramFiles%\MSBuild\12.0\Bin\msbuild.exe"
)
if exist "%ProgramFiles(x86)%\MSBuild\12.0\Bin\msbuild.exe" (
    set BUILD="%ProgramFiles(x86)%\MSBuild\12.0\Bin\msbuild.exe"
)

if exist "%ProgramFiles%\Git\cmd\git.exe" (
    set GIT="%ProgramFiles%\Git\cmd\git.exe"
)
if exist "%ProgramFiles(x86)%\Git\cmd\git.exe" (
    set GIT="%ProgramFiles(x86)%\Git\cmd\git.exe"
)

REM Clean
call .\clean.cmd

REM Build
%BUILD% .\pack.sln /maxcpucount /target:build /nologo /p:Configuration=Release /p:Platform=x86
if %ERRORLEVEL% NEQ 0 goto eof
%BUILD% .\pack.sln /maxcpucount /target:build /nologo /p:Configuration=Release /p:Platform=x64
if %ERRORLEVEL% NEQ 0 goto eof
%BUILD% .\pack.sln /maxcpucount /target:build /nologo /p:Configuration=Release /p:Platform="Any CPU"
if %ERRORLEVEL% NEQ 0 goto eof

REM Pack
nuget.exe pack MMaitre.MediaCaptureWPF.nuspec -OutputDirectory Packages -Prop NuGetVersion=%VERSION% -NoPackageAnalysis
if %ERRORLEVEL% NEQ 0 goto eof
nuget.exe pack MMaitre.MediaCaptureWPF.Symbols.nuspec -OutputDirectory Symbols -Prop NuGetVersion=%VERSION% -NoPackageAnalysis
if %ERRORLEVEL% NEQ 0 goto eof

REM Tag
%GIT% tag v%VERSION%