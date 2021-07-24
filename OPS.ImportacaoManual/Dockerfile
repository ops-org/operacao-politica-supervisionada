#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["OPS.ImportacaoManual/OPS.ImportacaoManual.csproj", "OPS.ImportacaoManual/"]
COPY ["OPS.ImportacaoDados/OPS.ImportacaoDados.csproj", "OPS.ImportacaoDados/"]
COPY ["OPS.Core/OPS.Core.csproj", "OPS.Core/"]
RUN dotnet restore "OPS.ImportacaoManual/OPS.ImportacaoManual.csproj"
COPY . .
WORKDIR "/src/OPS.ImportacaoManual"
RUN dotnet build "OPS.ImportacaoManual.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OPS.ImportacaoManual.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OPS.ImportacaoManual.dll"]