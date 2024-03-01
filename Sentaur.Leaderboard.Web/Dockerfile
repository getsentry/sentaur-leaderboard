FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sentaur.Leaderboard.Web/Sentaur.Leaderboard.Web.csproj", "Sentaur.Leaderboard.Web/"]
RUN dotnet restore "Sentaur.Leaderboard.Web/Sentaur.Leaderboard.Web.csproj"
COPY . .
WORKDIR "/src/Sentaur.Leaderboard.Web"
RUN dotnet build "Sentaur.Leaderboard.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Sentaur.Leaderboard.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sentaur.Leaderboard.Web.dll"]
