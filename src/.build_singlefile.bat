:: you need to have msbuild in environment variables for this to work
:: my msbuild.exe was in C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin

msbuild MinimalFirewall.csproj /t:Publish /p:Configuration=Release /p:RuntimeIdentifier=win-x64 /p:SelfContained=false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
pause