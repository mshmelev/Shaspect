msbuild ..\ShaspectBuilder.sln /t:clean;build /p:Configuration=Release
..\.nuget\nuget pack Shaspect.nuspec