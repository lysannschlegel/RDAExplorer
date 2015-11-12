$version = (dir ./bin/Release/RDAExplorerGUI.exe).VersionInfo.ProductVersion
$archiveName = "RDAExplorer-" + $version + ".zip"
Compress-Archive "bin/Release/RDAExplorerGUI.exe", "bin/Release/*.dll", "../../LICENSE" -DestinationPath $archiveName
