# BotApiTemplate

## Local build requirements

Project targets **.NET 8** (`net8.0`), so install .NET SDK 8.x before running build locally.

### Ubuntu 24.04

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### Verify installation

```bash
dotnet --info
dotnet restore
dotnet build -c Release
```

## Docker build

Repository already uses a multi-stage Dockerfile with `mcr.microsoft.com/dotnet/sdk:8.0` for build and `mcr.microsoft.com/dotnet/aspnet:8.0` for runtime, so host machine does not need SDK when building image in Docker.

```bash
docker build -t bot-api-template .
```
