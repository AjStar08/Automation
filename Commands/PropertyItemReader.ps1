=== PowerShell
Get-ItemProperty -Path IMG-20210101-WA0008.jpg | Format-list -Property * -Force
Set-ItemProperty -Path IMG-20210101-WA0008.jpg -Name LastWriteTime -Value "01/01/2021 22:20:39"

Get-ChildItem -force .\ * | ForEach-Object{$_.CreationTime = ("8 August 2020 00:00:00")}
Get-ChildItem -force .\ * | ForEach-Object{$_.CreationTime = ($_.LastWriteTime)}

