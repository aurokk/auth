# Denji

## Build

```
docker build -t denji .
docker run -d --name denji -p20000:80 denji
docker compose up -d
docker compose down -v
```

## Migrations

```
dotnet new tool-manifest
dotnet tool install dotnet-ef
dotnet tool restore
```

```
dotnet ef migrations add Test \
-o Migrations/Configuration \
--context ConfigurationDbContext \
--project src/Migrations \
--startup-project src/Api
```

```
dotnet ef migrations add Initial \
-o Migrations/PersistedGrant \
--context PersistedGrantDbContext \
--project src/Migrations \
--startup-project src/Api
```