$ErrorActionPreference = "Stop"

$root="$PSScriptRoot/.."

# clean
& $PSScriptRoot/Clean.ps1

$files = New-Object System.Collections.ArrayList

# build FileDBTool
dotnet restore "$root"
if (!$?) { throw "Failed to install dependencies for FileDBTool" }
dotnet publish "$root/src/FileDBTool" --configuration Release --framework net45
if (!$?) { throw "Failed to build FileDBTool" }
$files.AddRange(("$root/src/FileDBTool/bin/Release/net45/win7-x64/publish/*.exe", `
                 "$root/src/FileDBTool/bin/Release/net45/win7-x64/publish/*.dll"))

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
