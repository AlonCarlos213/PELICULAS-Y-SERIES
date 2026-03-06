# Use the official .NET 9.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
COPY MyStream.Api/MyStream.Api.csproj ./MyStream.Api/
COPY MyStream.Core/MyStream.Core.csproj ./MyStream.Core/
COPY MyStream.Infrastructure/MyStream.Infrastructure.csproj ./MyStream.Infrastructure/
RUN dotnet restore

# Copy everything else and build
COPY . ./
WORKDIR /app/MyStream.Api
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/MyStream.Api/out ./

# Set the port for Railway
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "MyStream.Api.dll"]
