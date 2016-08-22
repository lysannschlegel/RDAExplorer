$root="$PSScriptRoot/.."
Get-ChildItem $root -Include obj,bin,build -Recurse | Remove-Item -Recurse -Verbose
