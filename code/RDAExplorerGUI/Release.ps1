$version = (dir ./bin/Release/RDAExplorerGUI.exe).VersionInfo.ProductVersion
$archiveName = "RDAExplorer-" + $version + ".zip"
Compress-Archive "bin/Release/RDAExplorerGUI.exe", "bin/Release/RDAExplorerGUI.exe.config", "bin/Release/*.dll", "../../LICENSE.txt" -DestinationPath $archiveName
