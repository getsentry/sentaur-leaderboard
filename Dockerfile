﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG SENTRY_AUTH_TOKEN
ENV SENTRY_AUTH_TOKEN=$SENTRY_AUTH_TOKEN
WORKDIR /src
COPY ["Sentaur.Leaderboard.Api/Sentaur.Leaderboard.Api.csproj", "Sentaur.Leaderboard.Api/"]
RUN dotnet restore "Sentaur.Leaderboard.Api/Sentaur.Leaderboard.Api.csproj"
COPY . .
WORKDIR "/src/Sentaur.Leaderboard.Api"
RUN dotnet build "Sentaur.Leaderboard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG SENTRY_AUTH_TOKEN
ENV SENTRY_AUTH_TOKEN=$SENTRY_AUTH_TOKEN
RUN dotnet publish "Sentaur.Leaderboard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentaur.Leaderboard.Api.dll"]
