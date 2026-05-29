# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Multifactor.LdapConnector.sln ./
COPY Api/Api.csproj Api/
COPY App/App.csproj App/
COPY Infra/Infra.csproj Infra/
COPY Directory.Build.props .
COPY Directory.Packages.props .

RUN dotnet restore Api/Api.csproj

COPY . .
WORKDIR /src/Api
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    libldap-2.5-0 \
    libsasl2-2 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

RUN adduser --disabled-password --home /app --gecos '' appuser \
    && mkdir -p /data \
    && chown -R appuser /app /data
USER appuser

COPY --from=build /app/publish .

# ====================================
# Environment Variables Configuration
# ====================================
# The following environment variables can be set at runtime:
#
# REQUIRED:
# - ADMIN__DEFAULTPASSWORD           : Admin password for first login to the web panel
#                                      Example: MySecurePassword123
# - ENCRYPTION__KEY                  : 256-bit encryption key in Base64 format
#                                      Generate: openssl rand -base64 32
#                                      Example: 6B9y0WDyRuUcgW2oF1nRXRw/xLrFGESBgMeTlV1oRWA=
# - LITEDB__PATH                     : Path to the LiteDB database file inside the container
#                                      Example: /data/storage.db
#
# OPTIONAL:
# - ASPNETCORE_ENVIRONMENT           : Environment name (Development, Production)
#                                      Development: enables Swagger UI at /swagger
# - ASPNETCORE_URLS                  : Listening URL (default: http://+:8080)
#
# All other settings (LDAP connection, sync parameters, admin credentials)
# are configured through the web admin panel after first login.
#
# For docker-compose, use .env file
# For Kubernetes, use ConfigMap/Secret
# For direct docker run, use -e flags
# ====================================

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    LITEDB__PATH=/data/storage.db

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/v2/health/live || exit 1

ENTRYPOINT ["dotnet", "Api.dll"]