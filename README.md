# auth

```
dotnet publish src/Api/Api.csproj -o publish/ -c Release
dotnet swagger tofile --output swagger.json publish/Api.dll private
```