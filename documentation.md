# YopoAPI Project Documentation

This document provides an overview of the YopoAPI project structure and its components.

## 📁 **Root Level Files**
- **YopoAPI.csproj** - Project file
- **YopoAPI.sln** - Solution file
- **Program.cs** - Application entry point
- **appsettings.json** / **appsettings.Development.json** - Configuration files
- **README.md** - Documentation
- **.gitignore** - Git ignore rules
- **YopoAPI.http** - HTTP request file for testing
- **UpdateNamespaces.ps1** / **UpdateUsingStatements.ps1** - PowerShell scripts

## 📂 **Main Directories**

### **Controllers/**
- `StatusController.cs` - Basic status endpoint

### **Data/**
- `ApplicationDbContext.cs` - Entity Framework database context

### **DTOs/**
- `ApiResponseDto.cs` - Common API response structure

### **Middleware/**
- `ExceptionMiddleware.cs` - Global exception handling

### **Migrations/**
- Database migration files (InitialCreate, FixSeedData, AddExtendedUserManagementFeatures)

### **Modules/** (Modular Architecture)
This is the core of your API, organized into feature modules:

#### **🔐 Authentication/**
- Controllers: `AuthController.cs`
- DTOs: `AuthDto.cs`, `EnhancedAuthDto.cs`
- Models: `PasswordResetToken.cs`
- Services: `JwtTokenService.cs`, `PasswordResetService.cs`

#### **📋 PolicyManagement/**
- Controllers: `PolicyController.cs`
- DTOs: `PolicyDto.cs`
- Models: `Policy.cs`
- Services: `PolicyService.cs`

#### **🔑 RoleManagement/**
- Controllers: `PrivilegesController.cs`, `RolesController.cs`
- DTOs: `PrivilegeDto.cs`, `RoleDto.cs`
- Models: `Privilege.cs`, `Role.cs`, `RolePrivilege.cs`
- Services: `PrivilegeService.cs`, `RoleService.cs`

#### **👥 UserManagement/**
- Controllers: `InvitationsController.cs`, `UsersController.cs`
- DTOs: `InvitationDto.cs`, `UserDto.cs`
- Models: `Invitation.cs`, `User.cs`
- Services: `InvitationService.cs`, `UserService.cs`

### **Other Directories**
- **Properties/** - Launch settings
- **wwwroot/** - Static web assets with `index.html`
- **bin/** / **obj/** - Build artifacts and compiled files

## 🏗️ **Architecture Pattern**
Your project follows a **modular monolith** architecture where each business domain (Authentication, User Management, Role Management, Policy Management) is organized into separate modules with their own Controllers, DTOs, Models, and Services. This provides good separation of concerns and maintainability.

The project uses **.NET 9.0**, **Entity Framework Core**, **JWT Authentication**, and includes **Swagger** for API documentation.
