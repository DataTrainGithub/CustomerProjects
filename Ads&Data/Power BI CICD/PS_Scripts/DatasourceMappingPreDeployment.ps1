[CmdletBinding()]
Param(
    [string]$filepath,
    [string]$environment
)

$username = $env:USERNAME
$password = $env:PASSWORD

$model = (Split-Path $filepath -Leaf).Split('.')[0]
Write-Host "Model: $model"

# $mapping = Get-Content .\Mapping\ModelMapping.json | ConvertFrom-Json
# $currModel = $mapping.models | Where-Object { $_.Name -eq $Model }

# $environments = $currModel.environments
# $workspace = Invoke-Expression '$environments.$($environment)'
# Write-Host "Workspace: $workspace"

# if (!$workspace){
#     exit
# }

# Set Environment Variable used in Tabular Editor Script
$env:POWERBI_ENV = $environment

$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -S `".\CSHARP_Scripts\PQ_DatasourceMapping.csx`" -B $filepath -G"
    
Write-Host $p.ExitCode

if ($p.ExitCode -eq 1)
{
    Exit $p.ExitCode
}