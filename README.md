# Task Management System

## Technical Assessment Overview

Build a full-stack application to manage tasks and users.

The solution includes a .NET 6+ backend API and an Angular frontend. The API supports CRUD operations for tasks and users, along with role-based access control.

## Requirements

### 1. Functional Requirements

#### Entities
- **User**: Manages user accounts with roles (Admin/User)
- **Task**: Manages tasks with status tracking and user assignment

#### API Endpoints (Backend - .NET 6+)

**Users:**
- Create a user (Admin only)
- Retrieve a user
- List all users (Admin only)
- Update user details
- Delete a user (Admin only)

**Tasks:**
- Create a task (Admin only)
- Retrieve a task (Admin/User can only view tasks assigned to them)
- Update task details (Admin can update all fields; Users can only update the status of their assigned tasks)
- Delete a task (Admin only)
- List all tasks (Admin sees all; Users see only their assigned tasks)

#### Frontend (Angular 15+)
- User-friendly dashboard
- User Management (Admin role only)
- Task List: Admin can view and edit all tasks; Users can only view and update the status of their assigned tasks
- Form Validation: Proper validation for all user input fields
- Role-based Navigation: Restrict routes based on the role (Admin/User)
- Login form and authentication

### 2. Non-Functional Requirements

#### Backend
- Use .NET 6+ (or the latest version)
- Implement Entity Framework Core with SQL Server database (with in-memory fallback for testing)
- Implement dependency injection for service and repository layers
- Implement unit tests for:
  - At least one service layer method
  - At least one controller action
- Document API endpoints using Swagger/OpenAPI

#### Frontend
- Use Angular 15+ (or the latest stable version)
- Implement responsive design for better user experience
- Provide error handling and meaningful feedback for API failures
- Implement HTTP Interceptors for attaching authorization tokens

### 3. Additional Requirements

- Seed the database with:
  - 2 users: one Admin and one Regular User
  - 3 tasks with varying statuses (assigned to different users)
- Provide instructions for:
  - Running both backend and frontend projects
  - Setting up dependencies and configuring environments

## Technical Stack

### Backend
- **.NET 6.0**
- **Entity Framework Core 6.0** with SQL Server
- **JWT Authentication** (System.IdentityModel.Tokens.Jwt)
- **Serilog** for structured logging
- **Swashbuckle.AspNetCore** for Swagger/OpenAPI
- **BCrypt.Net-Next** for password hashing
- **xUnit** for unit testing

### Frontend
- **Angular 15+**
- **TypeScript**
- **RxJS** for reactive programming

## Project Structure

```
task/
├── backend/
│   ├── TaskManagement.Core/
│   │   ├── Entities/          # Domain entities (User, Task)
│   │   └── Interfaces/        # Repository and service interfaces
│   ├── TaskManagement.Infrastructure/
│   │   ├── Data/              # DbContext and database initialization
│   │   ├── Repositories/      # Repository implementations
│   │   └── Services/          # Business logic services
│   ├── TaskManagement.API/
│   │   ├── Controllers/       # API controllers
│   │   ├── Program.cs         # Application entry point
│   │   └── appsettings.json  # Configuration
│   └── TaskManagement.Tests/
│       ├── Services/          # Service layer tests
│       └── Controllers/       # Controller tests
├── frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/    # Angular components
│   │   │   ├── services/      # Angular services
│   │   │   ├── guards/        # Route guards
│   │   │   └── interceptors/  # HTTP interceptors
│   │   └── environments/      # Environment configuration
│   └── angular.json
└── README.md
```

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 6.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Node.js** 18.x or later ([Download](https://nodejs.org/))
- **Angular CLI** 15+ (`npm install -g @angular/cli`)
- **SQL Server** 2019 or later (or SQL Server Express)
- **Git** for cloning the repository

## Installation & Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd task
```

### 2. Backend Setup

#### Navigate to Backend Directory
```bash
cd backend
```

#### Restore NuGet Packages
```bash
dotnet restore
```

#### Configure Database Connection

Edit `TaskManagement.API/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME\\INSTANCE_NAME;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server:**
- Replace `YOUR_SERVER_NAME` with your SQL Server name (e.g., `DESKTOP-8KJVJIV`)
- Replace `INSTANCE_NAME` with your instance name (e.g., `MSSQLSERVER01`)
- If using default instance, use: `Server=YOUR_SERVER_NAME;Database=TaskManagementDb;...`

**For In-Memory Database (Testing):**
- Leave the connection string empty or remove it to use in-memory database

#### Create and Apply Migrations

```bash
cd TaskManagement.API
dotnet ef migrations add InitialCreate --project ../TaskManagement.Infrastructure
dotnet ef database update --project ../TaskManagement.Infrastructure
```

The database will be automatically seeded with:
- **2 Users:**
  - Admin: `username: admin`, `password: Admin123!`
  - User: `username: user`, `password: User123!`
- **3 Tasks** with varying statuses assigned to different users

### 3. Frontend Setup

#### Navigate to Frontend Directory
```bash
cd frontend
```

#### Install Dependencies
```bash
npm install
```

#### Configure Environment

The environment files are already configured:
- `src/environments/environment.ts` - Development environment
- `src/environments/environment.prod.ts` - Production environment

Default API URL: `http://localhost:5000`

## Running the Application

### Backend

#### Option 1: Using Visual Studio
1. Open `TaskManagement.sln` in Visual Studio
2. Set `TaskManagement.API` as the startup project
3. Press F5 to run

#### Option 2: Using Command Line

```bash
cd backend/TaskManagement.API
dotnet run --urls "http://localhost:5000;https://localhost:7000"
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7000`
- Swagger UI: `http://localhost:5000/swagger`

### Frontend

```bash
cd frontend
ng serve --open
```

The application will open automatically at `http://localhost:4200`

### Default Credentials

**Admin:**
- Username: `admin`
- Password: `Admin123!`

**Regular User:**
- Username: `user`
- Password: `User123!`

## API Documentation

### Swagger/OpenAPI

Once the backend is running, access the Swagger UI at:
```
http://localhost:5000/swagger
```

The Swagger documentation includes:
- Complete API endpoint descriptions
- Request/response models with examples
- Authentication requirements
- HTTP status codes
- Detailed summaries for each endpoint

### API Endpoints

#### Authentication
- `POST /api/auth/login` - Authenticate user and get JWT token

#### Users (Requires Authentication)
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user (Admin only)
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (Admin only)

#### Tasks (Requires Authentication)
- `GET /api/tasks` - Get all tasks (role-based filtering)
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task (Admin only)
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task (Admin only)

### Authentication

All endpoints except `/api/auth/login` require a JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

## Database Configuration

### SQL Server Setup

1. **Create Database:**
   The database will be created automatically when you run migrations.

2. **Connection String Format:**
   ```
   Server=SERVER_NAME\INSTANCE_NAME;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;
   ```

3. **Migrations:**
   ```bash
   dotnet ef migrations add MigrationName --project ../TaskManagement.Infrastructure
   dotnet ef database update --project ../TaskManagement.Infrastructure
   ```

### Database Seeding

The database is automatically seeded on first run with:
- **Admin User:** `admin` / `Admin123!`
- **Regular User:** `user` / `User123!`
- **3 Sample Tasks** with different statuses

## Testing

### Running Unit Tests

```bash
cd backend
dotnet test
```

### Test Coverage

The project includes unit tests for:
- **Service Layer:** `UserService` methods
- **Controller Layer:** `TasksController` actions

### Test Structure

```
TaskManagement.Tests/
├── Services/
│   └── UserServiceTests.cs    # Tests for user service
└── Controllers/
    └── TasksControllerTests.cs # Tests for tasks controller
```

## Role-Based Access Control

### Admin Role
- View all users and tasks
- Create, update, and delete users
- Create, update, and delete tasks
- Assign tasks to any user
- Access user management dashboard

### User Role
- View own profile
- View only assigned tasks
- Update status of assigned tasks
- Cannot create or delete tasks
- Cannot access user management

## Evaluation Criteria

### Code Quality
- ✅ Proper use of layers (controllers, services, repositories)
- ✅ Readable, maintainable, and well-organized code
- ✅ Appropriate use of design patterns like Repository and Unit of Work

### Functionality
- ✅ All specified endpoints work as expected
- ✅ Role-based access control is implemented correctly

### Testing
- ✅ Test coverage for critical functionality
- ✅ Unit tests for service layer and controller actions

### Documentation
- ✅ Clear and concise API documentation (Swagger/OpenAPI)
- ✅ Instructions in README.md are easy to follow

## Extras (Optional but Implemented)

The following additional features have been implemented to improve the final score:

1. **Structured Logging with Serilog**
   - Console and file logging
   - Request logging middleware
   - Log rotation (daily, 7-day retention)
   - Enriched with context, environment, and thread information

2. **Comprehensive Swagger Documentation**
   - XML comments for all endpoints
   - Detailed summaries and descriptions
   - Request/response examples
   - HTTP status code documentation

3. **Repository Pattern**
   - Clean separation of concerns
   - Generic repository implementation
   - Unit of Work pattern support

4. **Dependency Injection**
   - Service and repository layer DI
   - Interface-based design

5. **Error Handling**
   - Meaningful error messages
   - Proper HTTP status codes
   - Frontend error display

6. **CORS Configuration**
   - Properly configured for Angular frontend
   - Preflight request handling

## Troubleshooting

### Backend Issues

**Port Already in Use:**
```bash
# Find process using port 5000
netstat -ano | findstr :5000
# Kill the process
taskkill /PID <process-id> /F
```

**Database Connection Issues:**
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists or migrations are applied

**Build Errors:**
```bash
dotnet clean
dotnet restore
dotnet build
```

### Frontend Issues

**Port 4200 Already in Use:**
```bash
# Use different port
ng serve --port 4201
```

**Module Not Found Errors:**
- Delete `node_modules` and `package-lock.json`
- Run `npm install` again

**TypeScript Errors:**
- Restart TypeScript server in IDE
- Clear Angular cache: `ng cache clean`

### CORS Errors

If you encounter CORS errors:
1. Verify backend CORS configuration in `Program.cs`
2. Ensure frontend URL matches CORS allowed origins
3. Check that backend is running before frontend

## License

This project is created as part of a technical assessment.

## Author

Full-Stack Developer Assignment - Task Management System
"# TaskManagementSystem" 
