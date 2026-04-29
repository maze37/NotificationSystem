# NotificationSystem

`NotificationSystem` — сервис оркестрации уведомлений (`email/push/webhook`) на `.NET 10` в стиле `Clean Architecture` с практичным `DDD`-подходом.

Проект решает задачу надежной асинхронной доставки:
- постановка уведомлений в очередь;
- обработка воркером;
- повторные попытки с backoff;
- перевод в `DLQ` при исчерпании попыток;
- наблюдаемость через структурированные логи.

## Структура решения

- `src/NotificationSystem.Domain`
  - доменные сущности, value objects, инварианты, правила переходов статусов.
- `src/NotificationSystem.Application`
  - use-cases, `ICommand/IQuery` + handlers, валидаторы, сценарии обработки, `Result<..., Error>`.
- `src/NotificationSystem.Infrastructure`
  - EF Core, репозитории, миграции, RabbitMQ, gRPC-адаптеры, DI.
- `src/NotificationSystem.Contracts`
  - все внешние контракты: API request/response модели, `Envelope`, `Error`, события, protobuf, константы, базовые классы `Entity/AggregateRoot`.
- `src/NotificationSystem.Api`
  - REST API, middleware, health checks, swagger, logging.
- `src/NotificationSystem.Worker`
  - фоновый consumer, обработка retry и DLQ.

## Ключевые сценарии

- `POST /api/notifications` — создание задания на отправку уведомления.
- `GET /api/notifications/{id}` — получение уведомления по идентификатору.
- `GET /api/notifications?status=&channel=&from=&to=` — фильтрация уведомлений.

## Формат API-ответа

- Успех: `{ result, errors: null, timeGenerated }`
- Ошибка: `{ result: null, errors, timeGenerated }`
- Маппинг ошибок:
  - `validation -> 400`
  - `not_found -> 404`
  - `conflict -> 409`
  - `failure -> 500`

## Граница слоев

- `Application` содержит только orchestration/use-case логику, порты, валидаторы и доменные сценарии.
- `Contracts` содержит все модели обмена через границы приложения:
  - HTTP request/response контракты;
  - результаты use-case'ов, которые возвращаются во внешний слой;
  - события и protobuf-контракты.
- Внутренние доменные сущности и EF-модели не используются как внешние контракты напрямую.

Поддерживаемые статусы:
- `Created`
- `Queued`
- `Processing`
- `Delivered`
- `Failed`
- `DeadLettered`

## Надежность и устойчивость

- Идемпотентность через уникальный `CorrelationId`.
- Повторы через очереди с TTL (`10s`, `30s`, `60s`, `300s`).
- Dead-letter при постоянной ошибке или исчерпании лимита попыток.
- Индексы PostgreSQL:
  - `(Status, CreatedWhen)`
  - уникальный `CorrelationId`.

## Технологии

- `.NET 10`, `ASP.NET Core Web API`
- `PostgreSQL`, `EF Core`
- `RabbitMQ`
- `gRPC`
- `FluentValidation`
- `Serilog` + `Seq`
- `Docker Compose`

## Локальный запуск

```bash
docker compose down -v
docker compose up --build
```

## Проверка сборки

```bash
dotnet build NotificationSystem.slnx
```

## Конфигурация (ключевые параметры)

- `ConnectionStrings:Postgres`
- `RabbitMq:Host`, `RabbitMq:Port`, `RabbitMq:UserName`, `RabbitMq:Password`, `RabbitMq:VirtualHost`
- `RabbitMq:RetryDelaysSeconds`
- `GrpcEndpoints:TemplateServiceUrl`, `GrpcEndpoints:DeliveryServiceUrl`
- `Api:Port` (для API-хоста)
- `Seq` (опционально)

Локальный PostgreSQL из compose доступен на `localhost:5434`.

Адреса для проверки работоспособности:
- Swagger: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- RabbitMQ UI: [http://localhost:15672](http://localhost:15672)
- Seq: [http://localhost:5341](http://localhost:5341)

Быстрый smoke-check:
1. `POST /api/notifications` с `recipient=delivered@example.com`.
2. `GET /api/notifications/{id}` до статуса `Delivered`.
3. Проверить логи по `CorrelationId` в Seq.
