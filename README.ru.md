[![License](https://img.shields.io/badge/license-view-orange)](https://github.com/MultifactorLab/management-ldap-connector/blob/main/LICENSE.md)

# MGM.LDAP.Connector

_Also available in other languages: [English](README.md)_

**MGM.LDAP.Connector** — это локально развёртываемый компонент на стороне заказчика, предназначенный для интеграции **каталога LDAP/Active Directory** с **платформой Multifactor (MGM)**.

Компонент является частью решения сервиса <a href="https://multifactor.ru/" target="_blank">МультиФактор</a>.

* <a href="https://github.com/MultifactorLab/management-ldap-connector" target="_blank">Исходный код</a>
* <a href="https://github.com/MultifactorLab/management-ldap-connector/releases" target="_blank">Сборки</a>

## Содержание

* [Описание](#описание)
* [Требования](#требования)
* [Быстрый старт](#быстрый-старт)
  * [Сборка Docker образа](#сборка-docker-образа)
  * [SSL-сертификат для LDAP](#ssl-сертификат-для-ldap)
  * [Настройка окружения](#настройка-окружения)
  * [Развертывание](#развертывание)
* [Настройка](#настройка)
  * [Настройки LDAP](#настройки-ldap)
  * [Настройки синхронизации](#настройки-синхронизации)
  * [Статус синхронизации](#статус-синхронизации)
* [API](#api)
* [Ссылки](#ссылки)
* [Лицензия](#лицензия)

## Описание

MGM.LDAP.Connector выступает в роли моста между каталогом LDAP/Active Directory и платформой Multifactor (MGM).

**Архитектура:**
* **На стороне заказчика (локальное развертывание):**
  * LDAP/Active Directory — корпоративный каталог пользователей
  * MGM.LDAP.Connector — данный компонент

* **На стороне MultiFactor:**
  * MGM — платформа управления доступом и 2FA

**Основные функции:**
* Аутентификация пользователей через LDAP/Active Directory (API для внешних систем)
* Периодическая синхронизация учётных записей из LDAP-каталога в MGM
* Веб-панель администратора для настройки подключения и мониторинга синхронизации

## Требования

**Системные:**
* Docker Engine 20.10+
* Docker Compose 3.8+ (опционально)
* ОС: Linux, Windows Server 2019+, macOS
* Порт: настраиваемый

**Сетевая связность:**
* Исходящий HTTPS (443) к MGM API
* Входящий HTTP к LDAP Connector (настраиваемый порт) от внешних систем
* Исходящий LDAP (389) или LDAPS (636) к серверу каталога

## Быстрый старт

### Сборка Docker образа

```bash
# Клонирование репозитория
git clone https://github.com/MultifactorLab/management-ldap-connector.git
cd management-ldap-connector

# Сборка образа
docker build -t mgm-ldap-connector:latest .
```

### SSL-сертификат для LDAP

Если LDAP-сервер использует самоподписанный сертификат или внутренний CA, его необходимо добавить в образ. Поместите сертификат CA в формате PEM в `cicd/certs/ca.pem` — он будет автоматически зарегистрирован при сборке:

```dockerfile
COPY cicd/certs/ca.pem /usr/local/share/ca-certificates/samba-ca.crt
RUN update-ca-certificates
```

Если CA меняется — пересоберите образ.

### Настройка окружения

Создайте файл `.env`:

```bash
# Пароль администратора для первого входа в панель управления
ADMIN__DEFAULTPASSWORD=your_secure_password

# 256-битный ключ шифрования в формате Base64
# Генерация: openssl rand -base64 32
ENCRYPTION__KEY=your_base64_256bit_key

# Путь к файлу базы данных внутри контейнера
LITEDB__PATH=/data/storage.db
```

### Развертывание

**С помощью Docker Compose (рекомендуется):**

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
# Запуск сервиса
docker-compose up -d

# Проверка статуса
docker-compose ps

# Просмотр логов
docker-compose logs -f ldap-connector

# Проверка доступности
curl http://localhost:8080/api/ping
```

**С помощью Docker Run:**

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

После запуска откройте `http://<host>:8080` в браузере. Войдите с паролем из `ADMIN__DEFAULTPASSWORD`. При первом входе система потребует сменить пароль.

## Настройка

### Настройки LDAP

Раздел `/settings/ldap`. Параметры подключения к каталогу Active Directory / LDAP.

| Поле | Описание |
|------|----------|
| Адрес сервера | FQDN или IP-адрес LDAP-сервера |
| Порт | 389 (LDAP) или 636 (LDAPS) |
| Использовать SSL | Подключение через LDAPS |
| Base DN | Корневой DN для поиска пользователей |
| Bind DN | DN сервисной учётной записи |
| Пароль Bind DN | Пароль сервисной учётной записи (хранится зашифрованным) |
| Атрибут имени пользователя | Атрибут логина (`sAMAccountName` для AD, `uid` для OpenLDAP) |
| Атрибут email | Атрибут электронной почты (обычно `mail`) |
| Атрибут отображаемого имени | Атрибут полного имени (обычно `displayName`) |
| DN группы для синхронизации | DN группы для фильтрации. Если не задан — синхронизируются все пользователи из Base DN |

Нажмите **«Проверить подключение»** для проверки параметров, затем **«Сохранить»**.

### Настройки синхронизации

Раздел `/settings/sync`. Параметры подключения к MGM API.

| Поле | Описание |
|------|----------|
| Интервал синхронизации (минуты) | Периодичность синхронизации (минимум 1 минута, по умолчанию 60) |
| URL MGM API | Адрес API платформы Multifactor |
| API Key | Ключ авторизации для MGM API |
| API Secret | Секрет авторизации (хранится зашифрованным) |

API Key и API Secret генерируются в личном кабинете MGM в разделе «LDAP» после указания URL коннектора. Они же используются для Basic-аутентификации при вызове `/api/auth`.

После сохранения настроек синхронизация выполнится автоматически.

### Статус синхронизации

Раздел `/sync/status`. Информация о последнем запуске: время, количество синхронизированных пользователей и статус (успех/ошибка). Кнопка **«Запустить синхронизацию»** позволяет выполнить синхронизацию вручную.

## API

| Метод | Путь | Авторизация | Описание |
|-------|------|-------------|----------|
| `GET` | `/api/ping` | Нет | Проверка доступности сервиса |
| `POST` | `/api/auth` | Basic (API Key : API Secret) | Аутентификация LDAP-пользователя |

**POST /api/auth**

Запрос:
```json
{
  "username": "ivanov",
  "password": "user_password"
}
```

Успешный ответ (200):
```json
{
  "success": true,
  "displayName": "Иванов Иван"
}
```

Коды ошибок: `401` — неверные учётные данные, `404` — пользователь не найден, `503` — LDAP-сервер недоступен.

## Ссылки

* **Поддержка:** support@multifactor.ru
* **GitHub:** <https://github.com/MultifactorLab/management-ldap-connector>

## Лицензия

Обратите внимание на <a href="https://github.com/MultifactorLab/management-ldap-connector/blob/main/LICENSE.ru.md" target="_blank">лицензию</a>. Она не дает вам право вносить изменения в исходный код Компонента и создавать производные продукты на его основе. Исходный код предоставляется в ознакомительных целях.
