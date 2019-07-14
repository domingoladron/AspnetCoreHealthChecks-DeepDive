FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 54411

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["HealthChecks.SomeModelService/HealthChecks.SomeModelService.csproj", "HealthChecks.SomeModelService/"]
RUN dotnet restore "HealthChecks.SomeModelService/HealthChecks.SomeModelService.csproj"
COPY . .
WORKDIR "/src/HealthChecks.SomeModelService"
RUN dotnet build "HealthChecks.SomeModelService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HealthChecks.SomeModelService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HealthChecks.SomeModelService.dll"]
