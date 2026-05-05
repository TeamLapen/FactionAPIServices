FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY [ "Directory.Packages.props", "./" ]
COPY ["src/FactionAPI.Services/FactionAPI.Services.csproj", "src/FactionAPI.Services/"]
RUN dotnet restore "src/FactionAPI.Services/FactionAPI.Services.csproj"
COPY . .
WORKDIR "/src/src/FactionAPI.Services"
RUN dotnet build "./FactionAPI.Services.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FactionAPI.Services.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FactionAPI.Services.dll"]
