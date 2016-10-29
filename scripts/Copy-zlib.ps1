$root="$PSScriptRoot/.."

$src="$root/lib/zlib/bin/Release/zlib.dll"

$destinations=@("$root/src/RDAExplorer/bin/Release", `
                "$root/src/RDAExplorer/bin/Debug", `
                "$root/src/RDAExplorerGUI/bin/Release", `
                "$root/src/RDAExplorerGUI/bin/Debug")

$destinations.Foreach({
    mkdir -p $_ -Force
    Copy-Item $src "$_/zlib.dll" -Force
})
