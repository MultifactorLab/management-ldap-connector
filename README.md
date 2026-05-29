[![License](https://img.shields.io/badge/license-view-orange)](https://github.com/MultifactorLab/management-ldap-connector/blob/main/LICENSE.md)

# MGM.LDAP.Connector

_Также доступно на других языках: [Русский](README.ru.md)_

**MGM.LDAP.Connector** is a locally deployed component on the customer's side, designed to integrate an **LDAP/Active Directory** catalog with the **Multifactor (MGM) platform**.

The component is part of the <a href="https://multifactor.ru/" target="_blank">MultiFactor</a> service.

* <a href="https://github.com/MultifactorLab/management-ldap-connector" target="_blank">Source code</a>
* <a href="https://github.com/MultifactorLab/management-ldap-connector/releases" target="_blank">Releases</a>

## Table of Contents

* [Overview](#overview)
* [Requirements](#requirements)
* [Quick Start](#quick-start)
  * [Building the Docker Image](#building-the-docker-image)
  * [SSL Certificate for LDAP](#ssl-certificate-for-ldap)
  * [Environment Setup](#environment-setup)
  * [Deployment](#deployment)
* [Configuration](#configuration)
  * [LDAP Settings](#ldap-settings)
  * [Sync Settings](#sync-settings)
  * [Sync Status](#sync-status)
* [API](#api)
* [Links](#links)
* [License](#license)

## Overview

MGM.LDAP.Connector acts as a bridge between an LDAP/Active Directory catalog and the Multifactor (MGM) platform.

**Architecture:**
* **Customer side (local deployment):**
  * LDAP/Active Directory — corporate user directory
  * MGM.LDAP.Connector — this component

* **MultiFactor side:**
  * MGM — access management and 2FA platform

**Key features:**
* User authentication via LDAP/Active Directory (API for external systems)
* Periodic synchronization of user accounts from the LDAP catalog to MGM
* Web-based admin panel for connection configuration and sync monitoring

## Requirements

**System:**
* Docker Engine 20.10+
* Docker Compose 3.8+ (optional)
* OS: Linux, Windows Server 2019+, macOS
* Port: configurable

**Network connectivity:**
* Outbound HTTPS (443) to MGM API
* Inbound HTTP to LDAP Connector (configurable port) from external systems
* Outbound LDAP (389) or LDAPS (636) to the directory server

## Quick Start

### Building the Docker Image

```bash
# Clone the repository
git clone https://github.com/MultifactorLab/management-ldap-connector.git
cd management-ldap-connector

# Build the image
docker build -t mgm-ldap-connector:latest .
```

### SSL Certificate for LDAP

If the LDAP server uses a self-signed certificate or an internal CA, it must be added to the image. Place the CA certificate in PEM format at `cicd/certs/ca.pem` — it will be automatically registered during the build:

```dockerfile
COPY cicd/certs/ca.pem /usr/local/share/ca-certificates/samba-ca.crt
RUN update-ca-certificates
```

If the CA changes — rebuild the image.

### Environment Setup

Create a `.env` file:

```bash
# Admin password for the first login to the web panel
ADMIN__DEFAULTPASSWORD=your_secure_password

# 256-bit encryption key in Base64 format
# Generate: openssl rand -base64 32
ENCRYPTION__KEY=your_base64_256bit_key

# Path to the database file inside the container
LITEDB__PATH=/data/storage.db
```

### Deployment

**Using Docker Compose (recommended):**

```yaml
# docker-compose.yml
version: '3.8'

services:
  ldap-connector:
    image: mgm-ldap-connector:latest
    container_name: mgm-ldap-connector
    env_file:
      - .env
    ports:
      - "8080:8080"
    volumes:
      - ldap-connector-data:/data
    restart: unless-stopped

volumes:
  ldap-connector-data:
```

```bash
# Start the service
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f ldap-connector

# Check availability
curl http://localhost:8080/api/ping
```

**Using Docker Run:**

```bash
docker run -d \
  --name mgm-ldap-connector \
  -p 8080:8080 \
  -e ADMIN__DEFAULTPASSWORD="your_secure_password" \
  -e ENCRYPTION__KEY="your_base64_256bit_key" \
  -e LITEDB__PATH="/data/storage.db" \
  -v ldap-connector-data:/data \
  --restart unless-stopped \
  mgm-ldap-connector:latest
```

**PowerShell:**

```powershell
docker run -d `
  --name mgm-ldap-connector `
  -p 8080:8080 `
  -e ADMIN__DEFAULTPASSWORD="your_secure_password" `
  -e ENCRYPTION__KEY="your_base64_256bit_key" `
  -e LITEDB__PATH="/data/storage.db" `
  -v ldap-connector-data:/data `
  --restart unless-stopped `
  mgm-ldap-connector:latest
```

After starting, open `http://<host>:8080` in your browser. Log in with the password from `ADMIN__DEFAULTPASSWORD`. On first login, the system will require you to change the password.

## Configuration

### LDAP Settings

Section `/settings/ldap`. Connection parameters for Active Directory / LDAP.

| Field | Description |
|-------|-------------|
| Server Address | FQDN or IP address of the LDAP server |
| Port | 389 (LDAP) or 636 (LDAPS) |
| Use SSL | Connect via LDAPS |
| Base DN | Root DN for user search |
| Bind DN | Service account DN |
| Bind Password | Service account password (stored encrypted) |
| Username Attribute | Login attribute (`sAMAccountName` for AD, `uid` for OpenLDAP) |
| Email Attribute | Email address attribute (usually `mail`) |
| Display Name Attribute | Full name attribute (usually `displayName`) |
| Sync Group DN | Group DN for filtering. If not set — all users from Base DN are synchronized |

Click **"Test Connection"** to verify the parameters, then **"Save"**.

### Sync Settings

Section `/settings/sync`. MGM API connection parameters.

| Field | Description |
|-------|-------------|
| Sync Interval (minutes) | Synchronization frequency (minimum 1 minute, default 60) |
| MGM API URL | Multifactor platform API address |
| API Key | Authorization key for MGM API |
| API Secret | Authorization secret (stored encrypted) |

API Key and API Secret are generated in the MGM dashboard under the "LDAP" section after specifying the connector URL. They are also used for Basic authentication when calling `/api/auth`.

After saving the settings, synchronization will run automatically.

### Sync Status

Section `/sync/status`. Information about the last run: time, number of synchronized users, and status (success/error). The **"Run Sync"** button allows you to trigger synchronization manually.

## API

| Method | Path | Authorization | Description |
|--------|------|---------------|-------------|
| `GET` | `/api/ping` | None | Service availability check |
| `POST` | `/api/auth` | Basic (API Key : API Secret) | LDAP user authentication |

**POST /api/auth**

Request:
```json
{
  "username": "jdoe",
  "password": "user_password"
}
```

Successful response (200):
```json
{
  "success": true,
  "displayName": "John Doe"
}
```

Error codes: `401` — invalid credentials, `404` — user not found, `503` — LDAP server unavailable.

## Links

* **Support:** support@multifactor.ru
* **GitHub:** <https://github.com/MultifactorLab/management-ldap-connector>

## License

Please note the <a href="https://github.com/MultifactorLab/management-ldap-connector/blob/main/LICENSE.md" target="_blank">license</a>. It does not grant you the right to modify the source code of the Component or create derivative products based on it. The source code is provided for informational purposes only.
