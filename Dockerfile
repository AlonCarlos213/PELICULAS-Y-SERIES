# Use the official .NET 9.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and restore as distinct layers
COPY MyStreamProject.sln ./
COPY MyStream.Api/MyStream.Api.csproj ./MyStream.Api/
COPY MyStream.Core/MyStream.Core.csproj ./MyStream.Core/
COPY MyStream.Infrastructure/MyStream.Infrastructure.csproj ./MyStream.Infrastructure/
RUN dotnet restore MyStreamProject.sln

# Copy everything else and build
COPY . ./
WORKDIR /src
RUN dotnet restore MyStreamProject.sln
RUN dotnet publish MyStream.Api/MyStream.Api.csproj -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Set the port for Railway
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "MyStream.Api.dll"]
