@echo off

for /f "usebackq tokens=1* delims=: " %%i in (`lib\vswhere\vswhere -latest -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild`) do (
  if /i "%%i"=="installationPath" (
	set InstallDir=%%j
	if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
		echo "Using MSBuild from Visual Studio 2017"
		set msbuild="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
		goto build
	)
  )
)

FOR %%b in (
       "%VS140COMNTOOLS%\vsvars32.bat"
       "%VS120COMNTOOLS%\vsvars32.bat"
       "%VS110COMNTOOLS%\vsvars32.bat"
    ) do (
    if exist %%b ( 
		echo "Using MSBuild from %%b"
		call %%b
		set msbuild="msbuild"
		goto build
    )
)

if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" (
	echo "Using MSBuild from Visual Studio Community 2019"
	set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
	goto build
)

echo "Unable to detect suitable environment. Build may not succeed."

:build

SET target=%1
SET project=%2
SET solution=%3

IF "%target%" == "" SET target=Build
IF "%project%" == "" SET project=Orchard.proj
IF "%solution%" == "" SET solution=src\Orchard.sln

lib\nuget\nuget.exe restore %solution%

%msbuild% /t:%target% %project% /p:Solution=%solution% /m

:end

pause
