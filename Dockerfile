# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files first for better Docker layer caching
COPY src/DataGovIL.Api/DataGovIL.Api.csproj src/DataGovIL.Api/
COPY src/DataGovIL.Client/DataGovIL.Client.csproj src/DataGovIL.Client/

# Restore dependencies
RUN dotnet restore src/DataGovIL.Api/DataGovIL.Api.csproj

# Copy source code
COPY src/DataGovIL.Api/ src/DataGovIL.Api/
COPY src/DataGovIL.Client/ src/DataGovIL.Client/

# Publish
RUN dotnet publish src/DataGovIL.Api/DataGovIL.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Render provides the PORT environment variable.
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000}

EXPOSE 10000

ENTRYPOINT ["dotnet", "DataGovIL.Api.dll"]