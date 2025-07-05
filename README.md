# UserFeedbackWebAPI

A modern .NET 8 Web API for collecting and managing user feedback with comprehensive authentication features.

## Features

- **Complete Authentication System**:
  - User registration with email confirmation
  - JWT-based authentication with access & refresh tokens
  - Role-based authorization
  - Password hashing with Microsoft Identity
  - Token refresh mechanism
  - Account logout

- **User Feedback Collection**:
  - Submit ratings and comments
  - Email notifications
  - Data validation

## Technologies

- **.NET 8.0**
- **Entity Framework Core**
- **JWT Authentication**
- **Email Integration**
- **SQL Server/SQLite** (configurable)

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or SQLite
- SMTP server access for email features

### Installation

1. **Clone the repository**:


2. **Update the connection string** in `appsettings.json`:

3. **Configure email settings**:

4. **Set JWT configuration**:

5. **Apply migrations**:

6. **Run the application**:


## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Authenticate and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Invalidate refresh token
- `GET /api/auth/confirm` - Confirm email address
- `POST /api/auth/resend-confirmation` - Resend confirmation email

### Feedback

- `POST /api/feedback` - Submit new feedback
- `GET /api/feedback` - Get all feedback (admin only)
- `GET /api/feedback/{id}` - Get specific feedback

## Database Models

- **AppUser**: Authentication and user management
- **Feedback**: User submitted ratings and comments

## Deployment Options

- **Local Development**: IIS Express or Kestrel
- **Production**: Azure App Service, Docker, AWS, or any platform supporting .NET 8
- **Free Options**: 
  - Replit (no credit card required)
  - Azure for Students (if applicable)
  - Azure Free Tier (with limitations)

## Authentication Flow

1. User registers with email/password
2. User confirms email via link
3. User logs in and receives access/refresh tokens
4. Access token is used for API requests
5. When expired, refresh token is used to get new tokens

## License

MIT License

## Contributing

Contributions welcome! Please feel free to submit a Pull Request.
