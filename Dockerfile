FROM node:24-alpine AS web-build
WORKDIR /web

COPY web/package.json web/package-lock.json ./
RUN npm ci

COPY web/ .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/GerenciadorDeTarefas.Domain/GerenciadorDeTarefas.Domain.csproj src/GerenciadorDeTarefas.Domain/
COPY src/GerenciadorDeTarefas.Application/GerenciadorDeTarefas.Application.csproj src/GerenciadorDeTarefas.Application/
COPY src/GerenciadorDeTarefas.Infrastructure/GerenciadorDeTarefas.Infrastructure.csproj src/GerenciadorDeTarefas.Infrastructure/
COPY src/GerenciadorDeTarefas.Api/GerenciadorDeTarefas.Api.csproj src/GerenciadorDeTarefas.Api/
RUN dotnet restore src/GerenciadorDeTarefas.Api/GerenciadorDeTarefas.Api.csproj

COPY src/ src/
RUN dotnet publish src/GerenciadorDeTarefas.Api/GerenciadorDeTarefas.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=web-build /web/dist/web/browser ./wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet", "GerenciadorDeTarefas.Api.dll"]
