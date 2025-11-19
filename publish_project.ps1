# Test_Site_Gitlab-Runner_Build_Folder => "t3_NsAiAy"
# Live_Site_Gitlab-Runner_Build_Folder => "t3_p7s5mV"

# Set variables (replace with actual paths if not using environment variables)
$PUBLISH_DIR = "C:\Gitlab-CI-CD\artifacts"

# Navigate to the source directory
Set-Location -Path "C:\Gitlab-runner\builds\t3_p7s5mV\0\nop-station\comalytics-nopstation-b2b-nop-470\src"

# Restore dependencies
dotnet restore

# Navigate to the Nop.Web project directory
Set-Location -Path "Presentation\Nop.Web"

# Publish the project in Release configuration, outputting to the specified directory
dotnet publish "Nop.Web.csproj" -c Release -o $PUBLISH_DIR /p:CopyRefAssembliesToPublishDirectory=false

# Array of paths to check and remove if they exist
$pathsToCheck = @(
    "C:\Gitlab-CI-CD\artifacts\Plugins\NopStation.Plugin.B2B.ManageB2CandB2BCustomer\Views\B2BB2CCustomer\_B2BB2CRegister.cshtml",
    "C:\Gitlab-CI-CD\artifacts\Plugins\NopStation.Plugin.B2B.ManageB2CandB2BCustomer\Areas\Admin\Views\SyncTasks",
    "C:\Gitlab-CI-CD\artifacts\Plugins\NopStation.Plugin.B2B.ManageB2CandB2BCustomer\Areas\Admin\Views\ErpDataScheduler"
)

# Check each path and delete if it exists
foreach ($path in $pathsToCheck) {
    if (Test-Path $path) {
        Write-Host "Found directory to remove: $path"
        Write-Host "Deleting..."
        Remove-Item -Path $path -Recurse -Force
        Write-Host "Directory deleted successfully."
    } else {
        Write-Host "Directory not found: $path. No action needed."
    }
}

