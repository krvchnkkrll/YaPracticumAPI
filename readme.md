# YaPracticumApi

REST API для управления событиями

## Микросевисы

| Сервис       | БД            | Порты в докере |     Порты локальные      |
|--------------|---------------|:--------------:|:------------------------:|
| **Users**    | `users_db`    |      8081      | `https://localhost:8081` |
| **Events**   | `events_db`   |      8082      | `https://localhost:8082` |
| **Bookings** | `bookings_db` |      8083      | `https://localhost:8083` |

Каждый сервис построен по чистой архитектуре. Общий контракт события и имена топиков вынесены в отдельный проект.

## Брокер сообщений

В качестве брокера сообщений используется Kafka. Топики создаёт тот сервис, который на них подписан (иначе при первом запуске на пустом брокере подписчик может не найти топик).

### `booking-confirmed`

- **Publisher:** Bookings. Фоновый `BookingProcessingBackgroundService` каждые 5 секунд забирает все брони со статусом `Pending`, переводит бронь в `Confirmed`, сохраняет в свою БД — и только после успешного сохранения публикует `BookingConfirmedEvent` (`BookingId`, `EventId`, `UserId`, `SeatsCount`, `ConfirmedAt`). Ключ сообщения — `EventId`.
- **Consumer:** Events, `KafkaBookingConfirmedConsumerWorker`. На каждое сообщение находит событие по `EventId`, уменьшает `AvailableSeats` (`TryReserveSeats`).

### `booking-cancelled`

- **Publisher:** Bookings, при отмене подтверждённой брони — после того как статус `Cancelled` сохранён в БД.
- **Consumer:** Events, `KafkaBookingCancelledConsumerWorker`. Возвращает место обратно (`ReleaseSeats`).

## Авторизация

Jwt Токен выдаётся в UsersServiceAPI (`POST /auth/login`). EventsServiceAPI и BookingsServiceAPI проверяют — секрет, издателя `BookingApi` и аудиторию `BookingApiClient`.

- `[Authorize(Roles = "Admin")]` — `POST`/`PUT`/`DELETE /api/events`.
- Обычная аутентификация — эндпоинты броней; `UserId` читается из claims (`ICurrentUserService`).
- Доступ к чужой брони (`GET`/`DELETE /api/bookings/{id}`) закрыт для всех, кроме владельца и `Admin` (`IAccessService`).

## Запуск через Docker

Поднимает одной командой Zookeeper, Kafka, Kafka UI, три отдельные PostgreSQL и три сервиса:

```bash
docker compose up -d
```

- Users: `http://localhost:8081/swagger`
- Events: `http://localhost:8082/swagger`
- Bookings: `http://localhost:8083/swagger`
- Kafka UI: `http://localhost:8080`

Строки подключения к БД и адрес Kafka переопределяются переменными окружения прямо в `compose.yaml` (`ConnectionStrings__Postgres`, `Kafka__BootstrapServers`, `Kafka__ConsumerGroup`). Миграции накатываются автоматически при старте каждого сервиса.

## Запуск локально

Требования: .NET 9 SDK, PostgreSQL (три базы — `users_db`, `events_db`, `bookings_db`), Kafka + Zookeeper.

Для каждого сервиса в `*.Presentation/appsettings.json` уже прописаны локальные значения по умолчанию (`Host=localhost;Port=5432;...`, `Kafka:BootstrapServers = localhost:9092`).

```bash
dotnet run --project Users.Presentation
dotnet run --project Events.Presentation
dotnet run --project Bookings.Presentation
```
## Тесты

В каждом микросервисе свой набор тестов: Unit и интеграционные.

```bash
dotnet test
```
- `Tests` — тесты сервисов Application-слоя через реальный DI-контейнер с `EFCore.InMemory` вместо PostgreSQL; для Bookings внешние зависимости (`ICurrentUserService`, `IBookingEventPublisher`) замоканы через Moq. Docker не требуется.
- `IntegrationTests` — тесты репозиториев через `Testcontainers.PostgreSql` с PostgreSQL в Docker.

## Аутентификация и роли

Две роли: `User` и `Admin`.

- **Admin** — управляет событиями, может отменить любую бронь.
- **User** — может бронировать события и отменять только свои брони.

Алгоритм получения токена:
1. `POST /auth/register` (Users) — логин, пароль и опционально роль (по умолчанию `User`).
2. `POST /auth/login` (Users) — тот же логин/пароль, в ответе JWT-токен.
3. Указать токен в `Authorize` — в Swagger любого из трёх сервисов.

**Разграничение доступа:**

| Эндпоинт                                                              | Сервис   | Доступ                                 |
|-----------------------------------------------------------------------|----------|----------------------------------------|
| `POST /auth/register`, `POST /auth/login`                             | Users    | Без токена                             |
| `GET /api/events`, `GET /api/events/{id}`                             | Events   | Без токена                             |
| `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` | Events   | Только `Admin`                         |
| `POST /api/bookings/{id}/book`                                        | Bookings | Любой аутентифицированный пользователь |
| `GET /api/bookings/{id}`                                              | Bookings | Владелец брони или `Admin`             |
| `DELETE /api/bookings/{id}`                                           | Bookings | Владелец брони или `Admin`             |

## Модели

### Event (Events)
- `Id`, `Title`, `Description?`, `StartAt`, `EndAt`, `TotalSeats`, `AvailableSeats`

### User (Users)
- `Id`, `Login` (уникален), `PasswordHash` (SHA-256), `Role`

### Booking (Bookings)
- `Id`, `EventId`, `UserId`, `Status`, `CreatedAt`, `ProcessedAt?`

### BookingStatus
1. `Pending` — бронь создана
2. `Confirmed` — подтверждена фоновой обработкой в Bookings, место в Events зарезервировано
3. `Rejected` — используется только при внутренней ошибке обработки в самом Bookings.
4. `Cancelled` — отменена владельцем/админом; если бронь была `Confirmed`, место в Events освобождается через `booking-cancelled`

## Доменные правила

- У одного пользователя не может быть больше 10 активных (`Pending`/`Confirmed`) броней одновременно (Bookings, `BookingLimitExceededException` → 409).
- Нельзя зарезервировать место на уже начавшемся событии или при нехватке мест — эта проверка выполняется в Events асинхронно, в момент обработки `booking-confirmed`.
- Отменить бронь может только её владелец или `Admin` (403 иначе).
- Повторная отмена уже отменённой брони не приводит к ошибке.

## Формат ошибок

Единый для всех трёх сервисов:
```json
{
  "title": "",
  "status": 404,
  "detail": ""
}
```

## Пример сценария целиком

1. `POST /auth/register` (Users) с ролью `Admin` → `POST /auth/login` → токен админа.
2. `POST /auth/register` (Users) обычным пользователем → токен пользователя.
3. `POST /api/events` (Events, токен админа) — создать событие, запомнить `Id` и `AvailableSeats`.
4. `POST /api/bookings/{eventId}/book` (Bookings, токен пользователя) — бронь создаётся со статусом `Pending`.
5. Через 5–10 секунд `GET /api/bookings/{id}` — статус `Confirmed`.
6. `GET /api/events/{id}` (Events) — `AvailableSeats` уменьшилось на 1. Это доказывает, что событие прошло через Kafka, а не через прямой вызов.
7. `DELETE /api/bookings/{id}` (Bookings) — бронь `Cancelled`, `AvailableSeats` в Events возвращается обратно.

P.S
1) В задании нет, но не обработан кейс: Если бронь создается на событие, которого уже нет или событие уже началось.
2) В задании не было, но я на автомате сделал для `booking-cancelled` отмену.

- 5 запросов успешно создают бронь.
- 15 запросов получают ошибку 409 Conflict.
- AvailableSeats становится равным 0.

Таким образом система гарантирует, что количество созданных броней никогда не превышает вместимость события.

P.S.
1) Нейминг таблиц и полей не делал через Fluent API, т.к. считаю лучше это сделать в DI глобально для всех таблиц.
2) По старом спринту в BackgroundService требовалась проверка на то, что у брони может отсутствовать мероприятие, 
но это условие никогда не будет выполнено, т.к. бронь не может существовать без мероприятия, но оставил.