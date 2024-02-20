[CmdletBinding()]
Param(
    [string]$filepath,
    [string]$environment
)

$application_id = $env:APPLICATION_ID
$tenant_id = $env:TENANT_ID
$application_secret = (ConvertTo-SecureString -String $env:APPLICATION_SECRET -AsPlainText -Force)

if ($null -eq $application_id -or $null -eq $tenant_id -or $null -eq $application_secret) {
    Write-Host "Environment variables not set!"
    exit 1
}


$model = (Split-Path $filepath -Leaf).Split('.')[0]
Write-Host "Model: $model"

$mapping = Get-Content .\Mapping\ModelMapping.json | ConvertFrom-Json
$currModel = $mapping.models | Where-Object { $_.Name -eq $Model }

$environments = $currModel.environments
$env = $currModel.environments.$environment

if (!$env) {
    Exit
}

Write-Host "Environment: $env"

# Connecting to Power BI Tenant
$credential = New-Object System.Management.Automation.PSCredential ($application_id, $application_secret)
Connect-PowerBIServiceAccount -ServicePrincipal -Tenant $tenant_id -Credential $credential

# Get workspace
$workspace = Get-PowerBIWorkspace -Filter "tolower(name) eq '$($env.ToLower())'"

# Get dataset
$dataset = Get-PowerBIDataset -Workspace $workspace | Where-Object { $_.Name -eq $model }

if (!$dataset) {
    return $false
}
else {
    return $true
}