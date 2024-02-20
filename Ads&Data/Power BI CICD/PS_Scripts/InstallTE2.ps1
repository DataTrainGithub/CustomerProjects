# Installing Portable Tabular Editor
$tabularDownload = 'https://github.com/TabularEditor/TabularEditor/releases/download/2.17.1/TabularEditor.Portable.zip'

# Download destination (root of PowerShell script execution path):
$downloadDestination = join-path (get-location) "TabularEditor.zip"

# Download from GitHub:
Invoke-WebRequest -Uri $tabularDownload -OutFile $downloadDestination

# Unzip Tabular Editor portable, and then delete the zip file:
Expand-Archive -Path $downloadDestination -DestinationPath (get-location).Path
Remove-Item $downloadDestination