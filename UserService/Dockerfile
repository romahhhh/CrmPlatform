# Этап 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости (кэшируется)
COPY ["UserService/UserService.csproj", "UserService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "UserService/UserService.csproj"

# Копируем остальные файлы и собираем
COPY . .
WORKDIR /src/UserService
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserService.dll"]