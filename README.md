# Denji

Denji это OAuth 2.0 и OpenID Connect сервер.

В качестве отправной точки
взят [IdentityServer4](https://identityserver4.readthedocs.io/en/latest/). Так
как IdentityServer4 перестал развиваться, то было принято решение скопировать
исходники в проект и развивать проект на их основе.

Стек: C#, .NET7, PostgreSQL.

## Dependencies

1. `dotnet sdk >= 7.0.400`
2. `postgreSQL`
3. `docker & docker compose`

## Build & Run

Для запуска проекта есть набор сконфигурированных заранее сервисов, нужно
просто запустить.

Можно запустить одной командой, с помощью докера:
`docker compose up -d --build denji`. Образы будут собраны из исходников и запущены.

Либо можно запустить из исходников локально:

1. Запустить базу данных: `docker compose up -d denji-db`.
1. Запустить мигратор: `Api/Properties/launchSettings.json` конфигурацию `Migrator`.
1. Запустить апи: `Api/Properties/launchSettings.json` конфигурацию `Api`.

Остановить и удалить любые сервисы запущенные в докере:
`docker compose down -v`.

## Migrations

В проекте используется Entity Framework, все миграции генерируются и применяются с его помощью.

Для работы с миграциями локально нужно выполнить команду `dotnet tool restore`.

В проекте есть два контекста для работы с базой данных:

1. `ConfigurationDbContext` — контекст для работы с ресурсами, скоупами, клиентами и тд.
1. `PersistedGrantDbContext` — контекст для работы с операционными данными, токенами и прочим.
1. `OperationalDbContext` — контекст для работы с операционными данными.

Чтобы создать миграцию в `ConfigurationDbContext`:

```
dotnet ef migrations add Test \
-o Migrations/Configuration \
--context ConfigurationDbContext \
--project src/Migrations \
--startup-project src/Api
```

Чтобы создать миграцию в `PersistedGrantDbContext`:

```
dotnet ef migrations add Initial \
-o Migrations/PersistedGrant \
--context PersistedGrantDbContext \
--project src/Migrations \
--startup-project src/Api
```

Чтобы создать миграцию в `OperationalDbContext`:

```
dotnet ef migrations add Initial \
-o Migrations/Operational \
--context OperationalDbContext \
--project src/Migrations \
--startup-project src/Api
```

## Contributing

Изменения в проекте приветствуются в соответствии с [правилами](https://github.com/yaiam/.github/blob/main/CONTRIBUTING.md).