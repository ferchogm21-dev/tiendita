# Imagen base de runtime .NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "TienditaApp.csproj"
RUN dotnet publish "TienditaApp.csproj" -c Release -o /app/publish

# Runtime final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render usa el puerto 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TienditaApp.dll"]