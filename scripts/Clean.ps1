$root="$PSScriptRoot/.."
Get-ChildItem $root -Include obj,bin,build -Recurse -Force | Remove-Item -Recurse -Force
