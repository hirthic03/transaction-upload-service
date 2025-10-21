FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY src/Transactions.Api/*.csproj ./src/Transactions.Api/
COPY src/Transactions.Domain/*.csproj ./src/Transactions.Domain/
COPY src/Transactions.Infrastructure/*.csproj ./src/Transactions.Infrastructure/
COPY tests/Transactions.Tests/*.csproj ./tests/Transactions.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build
RUN dotnet build -c Release --no-restore

# Run tests
RUN dotnet test -c Release --no-build --verbosity normal

# Publish
RUN dotnet publish src/Transactions.Api/Transactions.Api.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Transactions.Api.dll"]