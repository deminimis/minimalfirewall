# Runs Visual Studio MSBuild with the given arguments.
# Full-framework MSBuild is required: the NetFwTypeLib COM reference cannot be built with 'dotnet build'.
$ErrorActionPreference = 'Stop'
$msbuild = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
if ($msbuild) {
    $msbuildPath = $msbuild.Source
} else {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (!(Test-Path $vswhere)) {
        throw 'Install Visual Studio 2026 or Build Tools with the .NET desktop development workload.'
    }
    $msbuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (!$msbuildPath) { throw 'Visual Studio MSBuild.exe was not found.' }
}
& $msbuildPath @args
exit $LASTEXITCODE
