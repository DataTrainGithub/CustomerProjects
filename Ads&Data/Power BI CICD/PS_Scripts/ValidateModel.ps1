[CmdletBinding()]
Param(
    [string]$filepath
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

$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -D `"Data Source=powerbi://api.powerbi.com/v1.0/myorg/Analytics Sandbox;User ID=app:$application_id@$tenant_id;Password=$application_secret;`" `"GithubActions_$model`" -O -G -W -E -C -P -Y"
exit $p.ExitCode