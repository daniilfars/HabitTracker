# HabitTracker API

Backend для трекера привычек с гибкой настройкой частоты выполнения и журналом выполненных задач

## Технологии

- **.NET 10** / ASP.NET Core Web API
- **Entity Framework Core** (Code First, PostgreSQL)
- **Clean Architecture** (Domain, Application, Infrastructure, WebAPI)
- **JWT + Refresh Tokens** (хранение в httpOnly cookies)
- **PostgreSQL** (JSONB для хранения пользовательских дней недели)
- **Docker** и **docker-compose** (мультиконтейнерное развертывание)
- **Swagger** (OpenAPI документация)

## Функциональность

### Пользователи и аутентификация
- Регистрация, вход, выход
- Обновление токена через refresh token
- Управление профилем (получение, обновление, удаление)

### Привычки
- Создание, редактирование, удаление привычек
- Три типа частоты: Daily (ежедневно), Weekly (раз в неделю), Custom (по выбранным дням)
- Фильтрация по активности, получение списка привычек пользователя

### Журнал выполнения
- Отметка выполнения привычки за текущую дату
- Просмотр истории по конкретной привычке или по всем привычкам пользователя
- Редактирование и удаление записей

### Безопасность
- JWT токены с коротким сроком жизни
- Refresh токены хранятся в базе данных и передаются через httpOnly cookies
- Проверка прав доступа на каждом эндпоинте (пользователь не может получить доступ к чужим данным)

## Структура проекта

```
HabitTracker/
├── Domain/              # Сущности, перечисления, базовые типы
├── Application/         # DTO, интерфейсы сервисов, бизнес-логика
├── Infrastructure/      # Реализация DbContext, миграции, сервисы
├── WebAPI/              # Контроллеры, middleware, настройки аутентификации
├── docker-compose.yml   # Конфигурация мультиконтейнерного запуска
└── HabitTracker.sln
```

## Запуск проекта

### Локальный запуск (без Docker)

1. Установите [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) и [PostgreSQL](https://www.postgresql.org/download/).
2. Настройте строку подключения в `appsettings.Development.json` или через User Secrets:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=HabitTracker;Username=postgres;Password=yourpassword"
     },
     "JwtSettings": {
       "SecretKey": "your-very-long-secret-key-min-32-characters",
       "Issuer": "HabitTracker",
       "Audience": "HabitTracker",
       "ExpiryMinutes": 15
     }
   }
   ```
3. Примените миграции:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project WebAPI
   ```
4. Запустите приложение:
   ```bash
   dotnet run --project WebAPI
   ```
5. Откройте Swagger: `http://localhost:5030/swagger`

### Запуск через Docker

1. Убедитесь, что установлены [Docker](https://www.docker.com/products/docker-desktop/) и Docker Compose.
2. Из корня репозитория выполните:
   ```bash
   docker-compose up --build
   ```
3. API будет доступно по адресу: `http://localhost:8081/swagger`
4. Для остановки контейнеров:
   ```bash
   docker-compose down
   ```

## Примеры запросов

### Регистрация
```
POST /api/Auth/register
Content-Type: application/json

{
  "name": "Ivan Ivanov",
  "email": "ivan@example.com",
  "password": "123456"
}
```

### Создание привычки (требуется авторизация)
```
POST /api/habit
Content-Type: application/json
Cookie: accessToken=...

{
  "name": "Читать 20 минут",
  "description": "Ежедневное чтение",
  "frequency": "Daily",
  "customDays": null
}
```

### Отметка выполнения
```
POST /api/log
Content-Type: application/json
Cookie: accessToken=...

{
  "habitId": 1,
  "isCompleted": true,
  "notes": "Прочитал 25 страниц"
}
```

## Лицензия

MIT
```
