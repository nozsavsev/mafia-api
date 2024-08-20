#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["mafia-API.csproj", "."]
RUN dotnet restore "./mafia-API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "mafia-API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mafia-API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mafia-API.dll"]