FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NotificationService/NotificationService.csproj", "NotificationService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "NotificationService/NotificationService.csproj"

COPY . .
WORKDIR /src/NotificationService
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "NotificationService.dll"]