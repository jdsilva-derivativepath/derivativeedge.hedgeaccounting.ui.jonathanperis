#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled-extra AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["DerivativeEDGE.HedgeAccounting.UI/DerivativeEDGE.HedgeAccounting.UI.csproj", "DerivativeEDGE.HedgeAccounting.UI/"]
RUN dotnet restore "DerivativeEDGE.HedgeAccounting.UI/DerivativeEDGE.HedgeAccounting.UI.csproj"
COPY . .
WORKDIR /src/DerivativeEDGE.HedgeAccounting.UI
RUN dotnet build "DerivativeEDGE.HedgeAccounting.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DerivativeEDGE.HedgeAccounting.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DerivativeEDGE.HedgeAccounting.UI.dll"]