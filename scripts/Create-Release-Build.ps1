$ErrorActionPreference = "Stop"

$root="$PSScriptRoot/.."

# clean
& $PSScriptRoot/Clean.ps1
# copy zlib
& $PSScriptRoot/Copy-zlib.ps1

$files = New-Object System.Collections.ArrayList

# build FileDBGenerator
msbuild "$root/RDAExplorer.sln" /target:"FileDBGenerator" /property:Configuration=Release
if (!$?) { throw "Failed to build FileDBGenerator" }
$files.AddRange(("$root/src/FileDBGenerator/bin/Release/FileDBGenerator.exe", `
                 "$root/src/FileDBGenerator/bin/Release/FileDBGenerator.exe.config", `
                 "$root/src/FileDBGenerator/bin/Release/*.dll"))

# build RDAExplorerGUI
msbuild "$root/RDAExplorer.sln" /target:"RDAExplorerGUI" /property:Configuration=Release
if (!$?) { throw "Failed to build RDAExplorerGUI" }
$files.AddRange(("$root/src/RDAExplorerGUI/bin/Release/RDAExplorerGUI.exe", `
                 "$root/src/RDAExplorerGUI/bin/Release/RDAExplorerGUI.exe.config", `
                 "$root/src/RDAExplorerGUI/bin/Release/*.dll"))

# create zip
$version = (dir "$root/src/RDAExplorerGUI/bin/Release/RDAExplorerGUI.exe").VersionInfo.ProductVersion
$archiveName = "RDAExplorer-" + $version + ".zip"
Compress-Archive $files `
    -DestinationPath $archiveName `
    -Force
if (!$?) { throw "Failed to create archive" }
