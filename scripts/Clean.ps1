$root="$PSScriptRoot/.."
Get-ChildItem $root -Include obj,bin,build -Recurse -Force |
    ?{ $_.fullname -notmatch "\\zlib\\?" } |
    Remove-Item -Recurse -Force
