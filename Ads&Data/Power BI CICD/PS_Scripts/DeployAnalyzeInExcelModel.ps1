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

$mapping = Get-Content .\Mapping\ModelMapping.json | ConvertFrom-Json
$currModel = $mapping.models | Where-Object { $_.Name -eq $Model }

$analyzeInExcel = $currModel.analyzeInExcel

Write-Host "AnalyzeInExcel: $analyzeInExcel"

if ((!$analyzeInExcel) -or ($analyzeInExcel -eq $False)) {
    Exit
}


$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -S `".\CSHARP_Scripts\Other\AnalyzeInExcel.csx`" -B $filepath -G"

Write-Host $p.ExitCode

if ($p.ExitCode -eq 1) {
    Exit $p.ExitCode
}

$cliparams = "-O -G -W -E -C -P -Y"
$workspace = "DaIP_Datasets_AnalyzeInExcel"

$p = Start-Process -filepath TabularEditor.exe -Wait -NoNewWindow -PassThru ` -ArgumentList "$filepath -D `"Data Source=powerbi://api.powerbi.com/v1.0/myorg/$workspace;User ID=app:$application_id@$tenant_id;Password=$application_secret;`" `"$model`" $cliparams"
exit $p.ExitCode