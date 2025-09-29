<div align="center">
  <img src="readme-banner/logo.svg" alt="Mafia" width="120" height="120">
</div>

# Mafia API

A real-time online Mafia game backend built with ASP.NET Core. Play the classic Mafia game with friends and strangers - no host computer required, fun for all ages, available in 3 languages, and works seamlessly even if you accidentally close the page.

## Features

- **No Host Computer Required**: Cloud-based system eliminates the need for a dedicated host computer
- **Real-time Gameplay**: Seamless real-time synchronization with all players seeing updates instantly
- **Multi-language Support**: Play in English, Spanish, or French
- **Accident-Proof**: Rejoin your game seamlessly and continue where you left off
- **Simple Room Join**: Join games with a simple room code, no complex registration process
- **Fun for All Ages**: Carefully designed gameplay that's enjoyable for players of all ages

## Game Features

- **Quick Setup**: Start playing in seconds, no downloads or installations required
- **Global Access**: Play with friends from anywhere in the world, anytime
- **Mobile Friendly**: Optimized for all devices - desktop, tablet, and mobile
- **Classic Gameplay**: Authentic Mafia experience with all the classic roles and mechanics

## How It Works

1. **Create or Join**: Create a new game room or join an existing one using a simple room code
2. **Choose Your Role**: Get assigned your role (Mafia, Sheriff, Doctor, or Civilian) and start strategizing
3. **Play & Win**: Use strategy, deception, and teamwork to outsmart your opponents

## Technology Stack

- **Backend**: ASP.NET Core
- **Real-time Communication**: SignalR
- **Database**: Entity Framework Core
- **Authentication**: JWT-based authentication
- **API Documentation**: OpenAPI/Swagger

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server or SQLite
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
2. Restore NuGet packages
3. Update connection strings in `appsettings.json`
4. Run database migrations
5. Start the application


## API Endpoints

The API provides endpoints for:

- User management and authentication
- Room creation and joining
- Game state management
- Real-time game events
- Server status monitoring

## Real-time Features

Built with SignalR for real-time communication:

- Live game state updates
- Player actions and votes
- Chat functionality
- Room management
- Connection status monitoring

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is open source and available under the MIT License.

## Support

For support, please open an issue on GitHub or contact the development team.