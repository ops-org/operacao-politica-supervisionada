#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy Projects
COPY ./OPS.API/OPS.API.csproj OPS.API/
COPY ./OPS.Core/OPS.Core.csproj OPS.Core/

# .NET Restore
RUN dotnet restore ./OPS.API/OPS.API.csproj

# Copy All Files
COPY . .
#RUN dotnet build "OPS.API.csproj" -c Release -o /app/build

# .NET Publish
WORKDIR "/src/OPS.API"
RUN dotnet publish "OPS.API.csproj" -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 8081
#EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OPS.API.dll"]