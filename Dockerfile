# Base image with runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Install ffmpeg into the base runtime image
RUN apt-get update \
    && apt-get install -y ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["mafia-API.csproj", "."]
RUN dotnet restore "./mafia-API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "mafia-API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "mafia-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mafia-API.dll"]
