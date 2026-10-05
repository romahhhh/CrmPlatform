# CrmPlatform — микросервисная CRM для специалистов

Учебный проект: микросервисная CRM-система, в которой специалисты управляют
клиентами и назначают сессии. Сервисы общаются через RabbitMQ, кэшируют
сессии в Redis, уведомления обрабатываются асинхронно.

## Архитектура

```
┌─────────────┐      ┌─────────────┐      ┌────────────────────┐
│ UserService │      │ CRMService  │      │ NotificationService│
│  :5001      │      │  :5002      │      │  (background)      │
└──────┬──────┘      └──────┬──────┘      └──────────┬─────────┘
       │                    │                        │
       │ JWT                │ JWT + Redis            │
       ▼                    ▼                        ▼
   ┌───────────────────────────────────────────────────────┐
   │          RabbitMQ (exchange crm.events)                │
   │  routing keys: user.registered, client.created,        │
   │                session.planned                         │
   └───────────────────────────────────────────────────────┘
                            │
       ┌────────────────────┼────────────────────┐
       ▼                    ▼                    ▼
   ┌──────────┐        ┌─────────┐         ┌──────────┐
   │ Postgres │        │  Redis  │         │RabbitMQ  │
   │ :5432    │        │ :6379   │         │ :15672 UI│
   └──────────┘        └─────────┘         └──────────┘
```

### Сервисы

| Сервис | Назначение | Порт (снаружи) |
|---|---|---|
| **UserService** | Регистрация, логин, выдача JWT | 5001 |
| **CRMService** | CRUD клиентов и сессий, Redis-кэш | 5002 |
| **NotificationService** | Обработка событий из RabbitMQ (эмуляция уведомлений) | — |
| **PostgreSQL** | Хранилище данных (`crm_db`, `crm_crm_db`) | 5432 |
| **Redis** | Кэш списков сессий по `ClientId` | 6379 |
| **RabbitMQ** | Обмен событиями | 5672, 15672 (UI) |

## Технологии

- **.NET 8 WebAPI**
- **PostgreSQL 16** (EF Core, Npgsql)
- **RabbitMQ 3** (MassTransit)
- **Redis 7** (StackExchange.Redis)
- **JWT** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Serilog** (логирование в консоль и файл)
- **Swagger / OpenAPI**
- **Docker + docker-compose**

## Быстрый старт

### Требования

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (с WSL 2 на Windows)
- Свободные порты: `5001`, `5002`, `5432`, `6379`, `5672`, `15672`

### Запуск

```bash
# 1. Клонировать репозиторий
git clone <URL_РЕПОЗИТОРИЯ>
cd CrmPlatform

# 2. Запустить всё (сборка + старт контейнеров)
docker compose up -d --build

# 3. Проверить, что все контейнеры Up
docker compose ps
```

Первый запуск займёт 2–5 минут (сборка образов). Последующие — секунды.

### Проверка

- **Swagger UserService:** http://localhost:5001/swagger
- **Swagger CRMService:** http://localhost:5002/swagger
- **RabbitMQ UI:** http://localhost:15672 (guest / guest)

### Остановка

```bash
# Остановить, сохранить данные БД
docker compose down

# Остановить и удалить данные БД (включая volume)
docker compose down -v
```

## Сквозной сценарий

```bash
# 1. Регистрация пользователя
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"anna@example.com","password":"password123","name":"Анна"}'

# 2. Логин → получить token
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"anna@example.com","password":"password123"}'
# Скопируйте "token" из ответа

# 3. Создать клиента (нужен токен)
curl -X POST http://localhost:5002/api/clients \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"name":"Пётр","email":"petr@example.com","phone":"+375291234567"}'
# Скопируйте "id" клиента

# 4. Создать сессию
curl -X POST http://localhost:5002/api/sessions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{
    "clientId":"<CLIENT_ID>",
    "scheduledAt":"2026-10-15T14:00:00Z",
    "durationInMinutes":60,
    "status":0,
    "notes":"Первая встреча"
  }'

# 5. Список сессий по клиенту (сначала из БД, потом из Redis)
curl http://localhost:5002/api/sessions/by-client/<CLIENT_ID> \
  -H "Authorization: Bearer <TOKEN>"
```

### Ожидаемые логи NotificationService

```bash
docker logs crm_notificationservice --tail=20
```

```
[Notification] New user registered: Анна (anna@example.com)
[Notification] Welcome email sent to petr@example.com
[Notification] Reminder for session 15.10.2026 14:00 (60 min)
```

## API

### UserService (`:5001`)

| Метод | Путь | Описание |
|---|---|---|
| POST | `/api/auth/register` | Регистрация нового пользователя |
| POST | `/api/auth/login` | Логин, выдача JWT |
| GET | `/api/auth/me` | Информация о текущем пользователе (нужен JWT) |
| PUT | `/api/auth/me` | Обновить профиль (имя) |
| DELETE | `/api/auth/me` | Удалить свой аккаунт |

### CRMService (`:5002`)

| Метод | Путь | Описание |
|---|---|---|
| GET | `/api/clients` | Список клиентов текущего пользователя |
| GET | `/api/clients/{id}` | Один клиент |
| POST | `/api/clients` | Создать клиента |
| PUT | `/api/clients/{id}` | Обновить клиента |
| DELETE | `/api/clients/{id}` | Удалить клиента |
| GET | `/api/sessions/by-client/{clientId}` | Сессии клиента (Redis-кэш) |
| GET | `/api/sessions/{id}` | Одна сессия |
| POST | `/api/sessions` | Создать сессию |
| PUT | `/api/sessions/{id}` | Обновить сессию |
| DELETE | `/api/sessions/{id}` | Удалить сессию |

⚠️ Все методы CRMService требуют JWT в заголовке `Authorization: Bearer <token>`.
Пользователь работает **только со своими** клиентами и сессиями.

## События RabbitMQ

Exchange: **`crm.events`**

| Routing key | Событие | Кто публикует | Кто слушает |
|---|---|---|---|
| `user.registered` | `UserRegisteredEvent` | UserService | NotificationService |
| `client.created` | `ClientCreatedEvent` | CRMService | NotificationService |
| `session.planned` | `SessionPlannedEvent` | CRMService | NotificationService |

## Структура решения

```
CrmPlatform/
├── CrmPlatform.sln
├── docker-compose.yml
├── init-db.sql
├── .dockerignore
├── README.md
├── Shared/                          # общие типы (события, настройки)
│   ├── Events/
│   │   ├── UserRegisteredEvent.cs
│   │   ├── ClientCreatedEvent.cs
│   │   └── SessionPlannedEvent.cs
│   ├── Messaging/
│   │   └── RabbitMqConstants.cs
│   └── Settings/
│       ├── JwtOptions.cs
│       ├── RabbitMqOptions.cs
│       └── RedisOptions.cs
├── UserService/
│   ├── Controllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Services/
│   ├── Data/
│   ├── Messaging/
│   ├── Migrations/
│   ├── Dockerfile
│   └── Program.cs
├── CRMService/
│   ├── Controllers/
│   ├── Models/
│   ├── DTOs/
│   ├── Services/
│   ├── Data/
│   ├── Cache/                       # Redis-кэш
│   ├── Messaging/
│   ├── Migrations/
│   ├── Dockerfile
│   └── Program.cs
└── NotificationService/
    ├── Consumers/                   # обработчики событий
    ├── Services/                    # ConsoleNotificationSender
    ├── Dockerfile
    └── Program.cs
```

## Разработка без Docker

Если нужно запустить сервис локально (`dotnet run`) без контейнера:

1. Поднять инфраструктуру:
   ```bash
   docker compose up -d postgres redis rabbitmq
   ```
2. Применить миграции:
   ```bash
   dotnet ef database update --project UserService
   dotnet ef database update --project CRMService
   ```
3. Запустить сервисы:
   ```bash
   dotnet run --project UserService
   dotnet run --project CRMService
   dotnet run --project NotificationService
   ```
4. В `appsettings.json` хосты — `localhost` (по умолчанию).

## Что реализовано

- ✅ Микросервисная архитектура (3 сервиса + Shared)
- ✅ JWT-аутентификация, единый Secret между UserService и CRMService
- ✅ Регистрация и логин с BCrypt-хэшированием
- ✅ CRUD клиентов и сессий с проверкой владельца (`UserId` из JWT)
- ✅ Redis-кэш списка сессий по `ClientId` с инвалидацией
- ✅ Публикация событий в RabbitMQ (3 типа)
- ✅ Обработка событий в NotificationService (эмуляция уведомлений)
- ✅ Serilog (структурированное логирование)
- ✅ Swagger на каждом сервисе
- ✅ Docker + docker-compose

## Лицензия

Учебный проект.