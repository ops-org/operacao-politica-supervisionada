# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["OPS.API/OPS.API.csproj", "OPS.API/"]
COPY ["OPS.Core/OPS.Core.csproj", "OPS.Core/"]
COPY ["OPS.Infraestrutura/OPS.Infraestrutura.csproj", "OPS.Infraestrutura/"]
RUN dotnet restore "OPS.API/OPS.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/OPS.API"
RUN dotnet build "OPS.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OPS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5200
ENV ASPNETCORE_ENVIRONMENT=Docker
EXPOSE 5200
ENTRYPOINT ["dotnet", "OPS.API.dll"]
