[CmdletBinding()]
Param(
    [string]$filepath,
    [string]$environment
)

$application_id = $env:APPLICATION_ID
$tenant_id = $env:TENANT_ID
$application_secret = $env:APPLICATION_SECRET

if ($null -eq $application_id -or $null -eq $tenant_id -or $null -eq $application_secret) {
    Write-Host "Environment variables not set!"
    exit 1
}

$model = (Split-Path $filepath -Leaf).Split('.')[0]
Write-Host "Model: $model"

$mapping = Get-Content .\Mapping\ModelMapping.json | ConvertFrom-Json
$currModel = $mapping.models | Where-Object { $_.Name -eq $Model }

$environments = $currModel.environments
$workspace = Invoke-Expression '$environments.$($environment)'
Write-Host "Workspace: $workspace"

if (!$workspace) {
    exit
}

# $cliparams = "-O -G -W -E -C -P"

if ($workspace -eq "DEV") {
    $cliparams = "-O -G -W -E -C -P"
}
else {
    $cliparams = "-O -G -W -E -C -P -Y"
}

$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -D `"Data Source=powerbi://api.powerbi.com/v1.0/myorg/$workspace;User ID=app:$application_id@$tenant_id;Password=$application_secret;`" `"$model`" $cliparams"
exit $p.ExitCode