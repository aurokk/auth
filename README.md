# Auth

## Статическая генерация swagger.json
```
dotnet publish src/Api/Api.csproj -o publish/ -c Release
dotnet swagger tofile --output swagger.json publish/Api.dll private
```