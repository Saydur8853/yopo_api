# YOPO API - User Manual

## 📋 Table of Contents
- [Overview](#overview)
- [System Requirements](#system-requirements)
- [Quick Start](#quick-start)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [Core Features](#core-features)
- [API Endpoints](#api-endpoints)
- [User Roles & Permissions](#user-roles--permissions)
- [Data Models](#data-models)
- [Error Handling](#error-handling)
- [Development Guide](#development-guide)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)
- [API Examples](#api-examples)

---

## 🚀 Overview

**YOPO API** is a comprehensive backend API system built with .NET 9.0 and ASP.NET Core, designed to provide robust user management, authentication, and role-based access control functionality. The system features JWT-based authentication, hierarchical role management, invitation systems, and comprehensive security features.

### Key Features
- 🔐 **JWT Authentication** - Secure token-based authentication
- 👥 **User Management** - Complete CRUD operations for users
- 🎭 **Role-Based Access Control** - Multi-level role hierarchy
- 📧 **Invitation System** - Secure user invitation workflow
- 🔒 **Password Reset** - Token-based password recovery
- 📋 **Policy Management** - Terms & conditions, privacy policies
- 🛡️ **Security Features** - Comprehensive privilege system
- 📊 **Health Monitoring** - API status and health checks

---

## 💻 System Requirements

### Prerequisites
- **.NET 9.0 SDK** or later
- **SQL Server LocalDB** (included with Visual Studio)
- **PowerShell 5.1+** (Windows)
- **Visual Studio 2022** or **Visual Studio Code** (recommended)

### Dependencies
- Microsoft.EntityFrameworkCore.SqlServer (9.0.7)
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.7)
- BCrypt.Net-Next (4.0.3)
- Swashbuckle.AspNetCore (9.0.3)

---

## 🏃‍♂️ Quick Start

### 1. Setup Database
```powershell
# Install Entity Framework tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply database migrations
dotnet ef database update
```

### 2. Run the Application
```powershell
# Development with HTTP only
dotnet run --launch-profile http

# Development with HTTPS
dotnet run --launch-profile https
```

### 3. Access the API
- **Base URL**: `http://localhost:5260` or `https://localhost:7097`
- **Swagger UI**: `http://localhost:5260/swagger`
- **Status Page**: `http://localhost:5260/`

---

## 📚 API Documentation

### Interactive Documentation
The API includes comprehensive Swagger documentation available at:
- **Swagger UI**: `/swagger/index.html`
- **OpenAPI Spec**: `/swagger/v1/swagger.json`

### Authentication in Swagger
1. Click **"Authorize"** button in Swagger UI
2. Enter: `Bearer {your-jwt-token}`
3. Click **"Authorize"** to apply to all requests

---

## 🔐 Authentication

### JWT Token Authentication
The API uses JSON Web Tokens (JWT) for authentication with the following configuration:
- **Token Expiry**: 24 hours
- **Issuer**: "YOPO API"
- **Audience**: "YOPO API"

### Getting Started
1. **First User**: The first user to sign up automatically becomes a Super Admin
2. **Subsequent Users**: Regular users can sign up directly or via invitation
3. **Login**: Use email and password to obtain JWT token

### Token Usage
Include the JWT token in all authenticated requests:
```http
Authorization: Bearer {your-jwt-token}
```

---

## 🎯 Core Features

### 1. User Management
- Create, read, update, and delete users
- User activation/deactivation
- Role assignment and management
- Profile management

### 2. Authentication System
- User registration and login
- Password change functionality
- Password reset with email verification
- JWT token refresh
- External login support (placeholder)

### 3. Role-Based Access Control
- Hierarchical role system
- Custom privilege assignment
- Role inheritance
- Multi-level permissions

### 4. Invitation System
- Secure user invitations
- Email-based invitation workflow
- Invitation expiration management
- Role-specific invitations

### 5. Policy Management
- Terms and conditions
- Privacy policies
- Version control
- Public access endpoints

---

## 🛠️ API Endpoints

### Authentication Endpoints (`/api/auth`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/login` | User login | No |
| POST | `/signup` | User registration | No |
| POST | `/change-password` | Change user password | Yes |
| POST | `/forgot-password` | Request password reset | No |
| POST | `/verify-code` | Verify reset code | No |
| POST | `/reset-password` | Reset password | No |
| POST | `/refresh-token` | Refresh JWT token | Yes |
| POST | `/logout` | User logout | Yes |
| GET | `/me` | Get current user | Yes |
| GET | `/role-check` | Check user role | Yes |

### User Management (`/api/users`)
| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| GET | `/` | Get all users | Admin, Manager |
| GET | `/{id}` | Get user by ID | Admin, Manager |
| POST | `/` | Create new user | Admin |
| PUT | `/{id}` | Update user | Admin |
| DELETE | `/{id}` | Delete user | Admin |
| GET | `/by-email/{email}` | Get user by email | Admin, Manager |
| POST | `/{id}/toggle-status` | Toggle user status | Admin |
| POST | `/{id}/assign-role` | Assign role to user | Super Admin |
| POST | `/{id}/update-status` | Update user status | Super Admin |

### Role Management (`/api/roles`)
| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| GET | `/` | Get all roles | Super Admin |
| GET | `/{id}` | Get role by ID | Super Admin |
| POST | `/` | Create new role | Super Admin |
| PUT | `/{id}` | Update role | Super Admin |
| DELETE | `/{id}` | Delete role | Super Admin |
| POST | `/{id}/assign-privileges` | Assign privileges to role | Super Admin |
| GET | `/{id}/privileges` | Get role privileges | Super Admin |
| POST | `/hierarchy` | Set role hierarchy | Super Admin |

### Invitations (`/api/invitations`)
| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| POST | `/` | Create invitation | Super Admin |
| GET | `/` | Get all invitations | Super Admin, Property Admin, Security Admin |
| GET | `/{id}` | Get invitation by ID | Super Admin, Property Admin, Security Admin |
| GET | `/check` | Check invitation status | Public |
| DELETE | `/{id}` | Delete invitation | Super Admin |

### Policy Management (`/api/policy`)
| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| GET | `/terms` | Get terms and conditions | Public |
| GET | `/privacy` | Get privacy policy | Public |
| GET | `/` | Get all policies | Super Admin |
| POST | `/` | Create policy | Super Admin |
| PUT | `/{id}` | Update policy | Super Admin |
| DELETE | `/{id}` | Delete policy | Super Admin |

### System Status (`/api/status`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/` | Get API status | No |
| GET | `/health` | Get health check | No |

---

## 👥 User Roles & Permissions

### Default Roles
1. **Super Admin** (Level 0)
   - Full system access
   - User and role management
   - System configuration

2. **Property Admin** (Level 1)
   - Property management privileges
   - User management (limited)

3. **Security Admin** (Level 2)
   - Security settings management
   - User monitoring

4. **Normal User** (Level 3)
   - Basic user privileges
   - Profile management

### Role Hierarchy
- Higher hierarchy levels can manage lower levels
- Super Admin can perform all operations
- Role assignments cascade privileges

### Privilege Categories
- **User Management**: Create, read, update, delete users
- **Role Management**: Manage roles and permissions
- **Invitation Management**: Create and manage invitations
- **System Configuration**: Configure system settings
- **Property Management**: Manage properties
- **Security Management**: Security settings

---

## 📊 Data Models

### User Model
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "isActive": true,
  "isSuperAdmin": false,
  "role": {
    "id": 2,
    "name": "Normal User",
    "description": "Regular user with limited access"
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### Role Model
```json
{
  "id": 1,
  "name": "Super Admin",
  "description": "Super Administrator with full system access",
  "hierarchyLevel": 0,
  "parentRoleId": null,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Invitation Model
```json
{
  "id": 1,
  "email": "newuser@example.com",
  "phoneNumber": "+1234567890",
  "invitedByUserId": 1,
  "roleId": 2,
  "isUsed": false,
  "expiresAt": "2024-01-02T00:00:00Z",
  "createdAt": "2024-01-01T00:00:00Z",
  "usedAt": null
}
```

---

## ⚠️ Error Handling

### Standard Error Response
```json
{
  "message": "Error description",
  "details": "Additional error details (if available)"
}
```

### Common HTTP Status Codes
- **200 OK**: Success
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

### Authentication Errors
- **401**: Invalid credentials or expired token
- **403**: Insufficient role permissions

---

## 🔧 Development Guide

### Project Structure
```
YopoAPI/
├── Controllers/          # API controllers
├── Services/            # Business logic
├── Data/               # Database context
├── Models/             # Data models
├── DTOs/               # Data transfer objects
├── Middleware/         # Custom middleware
├── Migrations/         # Database migrations
├── Properties/         # Launch settings
└── wwwroot/           # Static files
```

### Configuration
Update `appsettings.json` for your environment:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UserManagementDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyForJWTTokenGenerationThisKeyMustBeLongEnough",
    "Issuer": "YOPO API",
    "Audience": "YOPO API",
    "ExpiryHours": "24"
  }
}
```

### Adding New Features
1. Create model in `Models/`
2. Add DTOs in `DTOs/`
3. Implement service in `Services/`
4. Create controller in `Controllers/`
5. Update database context if needed
6. Create and apply migrations

### Database Migrations
```powershell
# Create new migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

---

## 🚀 Deployment

### Environment Setup
1. **Development**: Uses LocalDB, detailed error messages
2. **Production**: Configure production database, minimal error details

### Configuration for Production
- Update connection strings
- Set secure JWT keys
- Configure HTTPS settings
- Set up logging
- Configure CORS policies

### Docker Deployment (Optional)
```dockerfile
# Example Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY ./publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "YopoAPI.dll"]
```

---

## 🔍 Troubleshooting

### Common Issues

#### Database Connection Issues
```powershell
# Check SQL Server LocalDB
sqllocaldb info

# Start LocalDB instance
sqllocaldb start mssqllocaldb

# Update database
dotnet ef database update
```

#### Authentication Issues
- Verify JWT token format
- Check token expiration
- Ensure proper role assignments

#### Permission Errors
- Verify user roles
- Check endpoint authorization requirements
- Confirm user is active

### Debug Commands
```powershell
# Check application status
dotnet run --launch-profile http

# View logs
# Check console output during development

# Test API endpoints
curl -X GET "http://localhost:5260/api/status"
```

---

## 📝 API Examples

### 1. User Registration
```bash
curl -X POST "http://localhost:5260/api/auth/signup" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "phoneNumber": "+1234567890"
  }'
```

### 2. User Login
```bash
curl -X POST "http://localhost:5260/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePassword123!"
  }'
```

### 3. Get Current User
```bash
curl -X GET "http://localhost:5260/api/auth/me" \
  -H "Authorization: Bearer {your-jwt-token}"
```

### 4. Get All Users (Admin only)
```bash
curl -X GET "http://localhost:5260/api/users" \
  -H "Authorization: Bearer {admin-jwt-token}"
```

### 5. Create Invitation (Super Admin only)
```bash
curl -X POST "http://localhost:5260/api/invitations" \
  -H "Authorization: Bearer {super-admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "phoneNumber": "+1234567890",
    "roleId": 2
  }'
```

### 6. Password Reset Flow
```bash
# Request reset code
curl -X POST "http://localhost:5260/api/auth/forgot-password" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com"
  }'

# Verify code
curl -X POST "http://localhost:5260/api/auth/verify-code" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "code": "123456"
  }'

# Reset password
curl -X POST "http://localhost:5260/api/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "code": "123456",
    "newPassword": "NewSecurePassword123!"
  }'
```

---

## 📞 Support

For questions, issues, or contributions:
- Check the Swagger documentation at `/swagger`
- Review the API status at `/api/status`
- Test endpoints using the provided examples
- Ensure proper authentication and authorization

---

## 🔄 Version Information

- **API Version**: 1.0.0
- **Framework**: .NET 9.0
- **Database**: SQL Server LocalDB
- **Authentication**: JWT Bearer Token
- **Documentation**: OpenAPI 3.0

---

*This user manual provides comprehensive guidance for using the YOPO API. For the most up-to-date information, always refer to the Swagger documentation when the API is running.*
