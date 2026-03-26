# YaPracticumApi
## REST API для управления событиями

Проект на .net 9

### Сборка/запуск

build: ```dotnet build```

run: ```dotnet run --project Web```

listening: ```https://localhost:7013```

swagger: ```https://localhost:7013/swagger```

### API

- GET /api/events - возвращает коллекцию событий. (При запуске уже есть добавленные)
- GET /api/events/{id} - возвращает событие
- POST /api/events - создает и возвращает событие.
- PUT /api/events/{id} - изменяет и возвращает событие.
- DELETE /api/events/{id} - удаляет событие.