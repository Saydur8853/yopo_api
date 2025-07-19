# PowerShell script to update namespaces for modular structure

# Function to update file content
function Update-FileNamespace {
    param (
        [string]$FilePath,
        [string]$OldNamespace,
        [string]$NewNamespace
    )
    
    if (Test-Path $FilePath) {
        (Get-Content $FilePath -Raw) -replace $OldNamespace, $NewNamespace | Set-Content $FilePath
        Write-Host "Updated: $FilePath"
    }
}

# Authentication Module DTOs
$AuthDTOFiles = Get-ChildItem "Modules\Authentication\DTOs\*.cs" -Recurse
foreach ($file in $AuthDTOFiles) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.DTOs" "namespace YopoAPI.Modules.Authentication.DTOs"
}

# Authentication Module Services
$AuthServiceFiles = Get-ChildItem "Modules\Authentication\Services\*.cs" -Recurse
foreach ($file in $AuthServiceFiles) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Services" "namespace YopoAPI.Modules.Authentication.Services"
}

# Authentication Module Models
$AuthModelFiles = Get-ChildItem "Modules\Authentication\Models\*.cs" -Recurse
foreach ($file in $AuthModelFiles) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Models" "namespace YopoAPI.Modules.Authentication.Models"
}

# UserManagement Module
$UserMgmtControllers = Get-ChildItem "Modules\UserManagement\Controllers\*.cs" -Recurse
foreach ($file in $UserMgmtControllers) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Controllers" "namespace YopoAPI.Modules.UserManagement.Controllers"
}

$UserMgmtDTOs = Get-ChildItem "Modules\UserManagement\DTOs\*.cs" -Recurse
foreach ($file in $UserMgmtDTOs) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.DTOs" "namespace YopoAPI.Modules.UserManagement.DTOs"
}

$UserMgmtServices = Get-ChildItem "Modules\UserManagement\Services\*.cs" -Recurse
foreach ($file in $UserMgmtServices) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Services" "namespace YopoAPI.Modules.UserManagement.Services"
}

$UserMgmtModels = Get-ChildItem "Modules\UserManagement\Models\*.cs" -Recurse
foreach ($file in $UserMgmtModels) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Models" "namespace YopoAPI.Modules.UserManagement.Models"
}

# RoleManagement Module
$RoleMgmtControllers = Get-ChildItem "Modules\RoleManagement\Controllers\*.cs" -Recurse
foreach ($file in $RoleMgmtControllers) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Controllers" "namespace YopoAPI.Modules.RoleManagement.Controllers"
}

$RoleMgmtDTOs = Get-ChildItem "Modules\RoleManagement\DTOs\*.cs" -Recurse
foreach ($file in $RoleMgmtDTOs) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.DTOs" "namespace YopoAPI.Modules.RoleManagement.DTOs"
}

$RoleMgmtServices = Get-ChildItem "Modules\RoleManagement\Services\*.cs" -Recurse
foreach ($file in $RoleMgmtServices) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Services" "namespace YopoAPI.Modules.RoleManagement.Services"
}

$RoleMgmtModels = Get-ChildItem "Modules\RoleManagement\Models\*.cs" -Recurse
foreach ($file in $RoleMgmtModels) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Models" "namespace YopoAPI.Modules.RoleManagement.Models"
}

# PolicyManagement Module
$PolicyMgmtControllers = Get-ChildItem "Modules\PolicyManagement\Controllers\*.cs" -Recurse
foreach ($file in $PolicyMgmtControllers) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Controllers" "namespace YopoAPI.Modules.PolicyManagement.Controllers"
}

$PolicyMgmtDTOs = Get-ChildItem "Modules\PolicyManagement\DTOs\*.cs" -Recurse
foreach ($file in $PolicyMgmtDTOs) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.DTOs" "namespace YopoAPI.Modules.PolicyManagement.DTOs"
}

$PolicyMgmtServices = Get-ChildItem "Modules\PolicyManagement\Services\*.cs" -Recurse
foreach ($file in $PolicyMgmtServices) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Services" "namespace YopoAPI.Modules.PolicyManagement.Services"
}

$PolicyMgmtModels = Get-ChildItem "Modules\PolicyManagement\Models\*.cs" -Recurse
foreach ($file in $PolicyMgmtModels) {
    Update-FileNamespace $file.FullName "namespace YopoAPI.Models" "namespace YopoAPI.Modules.PolicyManagement.Models"
}

Write-Host "Namespace update completed!"
