FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /source
COPY *.sln                                                                   .
COPY src/Api/*.csproj                                                        ./src/Api/
COPY src/IdentityServer4/*.csproj                                            ./src/IdentityServer4/
COPY src/IdentityServer4.EntityFramework/*.csproj                            ./src/IdentityServer4.EntityFramework/
COPY src/IdentityServer4.EntityFramework.Storage/*.csproj                    ./src/IdentityServer4.EntityFramework.Storage/
COPY src/IdentityServer4.Storage/*.csproj                                    ./src/IdentityServer4.Storage/
COPY tests/IdentityServer4.EntityFramework.IntegrationTests/*.csproj         ./tests/IdentityServer4.EntityFramework.IntegrationTests/
COPY tests/IdentityServer4.EntityFramework.Storage.IntegrationTests/*.csproj ./tests/IdentityServer4.EntityFramework.Storage.IntegrationTests/
COPY tests/IdentityServer4.EntityFramework.Storage.UnitTests/*.csproj        ./tests/IdentityServer4.EntityFramework.Storage.UnitTests/
COPY tests/IdentityServer4.IntegrationTests/*.csproj                         ./tests/IdentityServer4.IntegrationTests/
COPY tests/IdentityServer4.UnitTests/*.csproj                                ./tests/IdentityServer4.UnitTests/
RUN dotnet restore

COPY src/.   ./src/
COPY tests/. ./tests/
WORKDIR /source/src/Api
RUN dotnet publish -c release -o /dist --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /dist ./
ENTRYPOINT ["dotnet", "Api.dll"]