# PowerShell script to update using statements for modular structure

# Function to update using statements
function Update-UsingStatements {
    param (
        [string]$FilePath
    )
    
    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw
        
        # Replace old using statements with new module-based ones
        $content = $content -replace "using YopoAPI\.DTOs;", "using YopoAPI.Modules.Authentication.DTOs;`r`nusing YopoAPI.Modules.UserManagement.DTOs;`r`nusing YopoAPI.Modules.RoleManagement.DTOs;`r`nusing YopoAPI.Modules.PolicyManagement.DTOs;"
        $content = $content -replace "using YopoAPI\.Services;", "using YopoAPI.Modules.Authentication.Services;`r`nusing YopoAPI.Modules.UserManagement.Services;`r`nusing YopoAPI.Modules.RoleManagement.Services;`r`nusing YopoAPI.Modules.PolicyManagement.Services;"
        $content = $content -replace "using YopoAPI\.Models;", "using YopoAPI.Modules.Authentication.Models;`r`nusing YopoAPI.Modules.UserManagement.Models;`r`nusing YopoAPI.Modules.RoleManagement.Models;`r`nusing YopoAPI.Modules.PolicyManagement.Models;"
        
        Set-Content $FilePath $content
        Write-Host "Updated using statements in: $FilePath"
    }
}

# Update all files in modules
$AllCsFiles = Get-ChildItem "Modules\**\*.cs" -Recurse
foreach ($file in $AllCsFiles) {
    Update-UsingStatements $file.FullName
}

# Update remaining files in root directories
$RootFiles = @(
    "Data\ApplicationDbContext.cs",
    "Program.cs",
    "Controllers\StatusController.cs"
)

foreach ($file in $RootFiles) {
    if (Test-Path $file) {
        Update-UsingStatements (Resolve-Path $file).Path
    }
}

Write-Host "Using statements update completed!"
