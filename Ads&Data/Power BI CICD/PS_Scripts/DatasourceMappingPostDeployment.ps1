[CmdletBinding()]
Param(
    [string]$filepath,
    [string]$environment
)

$username = $env:USERNAME
$password = $env:PASSWORD

if ($username -eq $null -or $password -eq $null)
{
    Write-Host "Username or Password missing..."
    exit 1
}

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

# Download BIM file from server
$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "`"Provider=MSOLAP;Data Source=powerbi://api.powerbi.com/v1.0/myorg/$workspace;User ID=$username;Password=$password;`" `"$model`" -B .\DownloadedModel.bim -G -W -E"
Write-Host "Finished Downloading BIM file: $($p.ExitCode)"

# Apply Datasource Mapping to file from server
$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "`"DownloadedModel.bim`" -S `".\CSHARP_Scripts\PQ_DatasourceMapping.csx`" -B `".\ToUploadModel.bim`" -G"
Write-Host "Finished Applying Datasource Mapping against BIM file: $($p.ExitCode)"

# Push changed file to server
$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "`"ToUploadModel.bim`" -D `"Provider=MSOLAP;Data Source=powerbi://api.powerbi.com/v1.0/myorg/$workspace;User ID=$username;Password=$password;`" `"$model`" -O -G -W -E -C -P"
Write-Host "Pushing BIM file to the service: $($p.ExitCode)"
exit $p.ExitCode
