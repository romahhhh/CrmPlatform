FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CRMService/CRMService.csproj", "CRMService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "CRMService/CRMService.csproj"

COPY . .
WORKDIR /src/CRMService
RUN dotnet publish "CRMService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CRMService.dll"]