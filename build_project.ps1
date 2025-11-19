# Test_Site_Gitlab-Runner_Build_Folder => "t3_NsAiAy"
# Live_Site_Gitlab-Runner_Build_Folder => "t3_p7s5mV"

# Navigate to the source directory
Set-Location -Path "C:\Gitlab-runner\builds\t3_p7s5mV\0\nop-station\comalytics-nopstation-b2b-nop-470\src"

# Clean the project in Release configuration
dotnet clean --configuration Release

# Restore all dependencies
dotnet restore

# Build the specific NopStation plugin to avoid missing references
dotnet build "Plugins\NopStation.Plugin.Misc.Actions\NopStation.Plugin.Misc.Actions.csproj" --configuration Release

# Output the current directory path for verification
Write-Output (Get-Location)

# Perform the main build without restoring again
dotnet build --no-restore -c Release
