[CmdletBinding()]
Param(
    [string]$filepath
)

$model = (Split-Path $filepath -Leaf).Split('.')[0]
Write-Host "Model: $model"

$mapping = Get-Content .\Mapping\ModelMapping.json | ConvertFrom-Json
$currModel = $mapping.models | Where-Object { $_.Name -eq $Model }

$scripts = $currModel.scripts

Write-Host "Scripts: $scripts"

if (!$scripts){
    Exit
}

foreach ($script in $scripts){
    $p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -S `".\CSHARP_Scripts\$script.csx`" -B $filepath -G"
    
    Write-Host $p.ExitCode

    if ($p.ExitCode -eq 1)
    {
        Exit $p.ExitCode
    }
}