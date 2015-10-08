@del Shaspect*.nupkg

msbuild ..\ShaspectBuilder.sln /t:clean;build /p:Configuration=Release
@if errorlevel 1 goto BuildFailed

msbuild ..\Shaspect.sln /t:clean;build /p:Configuration=Release
@if errorlevel 1 goto BuildFailed

..\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe ..\Shaspect.AssemblyTests\bin\Release\Shaspect.AssemblyTests.dll
@if errorlevel 1 goto BuildFailed

..\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe ..\Shaspect.Tests\bin\Release\Shaspect.Tests.dll
@if errorlevel 1 goto BuildFailed

..\.nuget\nuget pack Shaspect.nuspec
@if errorlevel 1 goto BuildFailed

:BuildFailed
