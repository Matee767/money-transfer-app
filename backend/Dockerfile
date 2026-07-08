# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so dependency layers are cached between builds
COPY src/MoneyTransfer.Api/MoneyTransfer.Api.csproj src/MoneyTransfer.Api/
RUN dotnet restore src/MoneyTransfer.Api/MoneyTransfer.Api.csproj

COPY src/ src/
RUN dotnet publish src/MoneyTransfer.Api/MoneyTransfer.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "MoneyTransfer.Api.dll"]
