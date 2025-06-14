# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore
COPY . . 
RUN dotnet restore

# Publish the application to the /out folder
RUN dotnet publish -c Release -o /out

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published files from build stage
COPY --from=build /out ./

# Start the app
ENTRYPOINT ["dotnet", "ChatApp.Server.dll"]
