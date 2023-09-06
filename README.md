# Auth

## Build
```
docker build -t auth .
docker run -d --name auth -p20000:80 auth
docker compose up -d
docker compose down -v
```