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

$derrivativeModels = $currModel.derrivativeModels

if (!$derrivativeModels) {
    Write-Host "No derrivative models needed..."
    exit
}

foreach ($derrivativeModel in $derrivativeModels) {
    # Set Environment Variable used in Tabular Editor Script
    $env:CONFIG = $derrivativeModel | ConvertTo-Json
    
    $p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -S `".\CSHARP_Scripts\PQ_DerrivativeModel.csx`" -B `".\ToUploadModel.bim`" -G"
    Write-Host "Finished creating derrivative model: $($p.ExitCode)"

    if ($p.ExitCode -eq 1) {
        exit $p.ExitCode
    }

    #Push changed file to server
    $p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "`"ToUploadModel.bim`" -D `"Data Source=powerbi://api.powerbi.com/v1.0/myorg/$workspace;User ID=app:$application_id@$tenant_id;Password=$application_secret;`" `"$($derrivativeModel.Name)`" -O -G -W -E -C -P"
    Write-Host "Pushing BIM file to the service: $($p.ExitCode)"
    
    if ($p.ExitCode -eq 1) {
        exit $p.ExitCode
    }
}