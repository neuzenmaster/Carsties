FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /app

COPY Carsties.sln ./
COPY src/NotificationService/NotificationService.csproj ./src/NotificationService/

RUN dotnet restore ./src/NotificationService/NotificationService.csproj

COPY . .
RUN dotnet publish ./src/NotificationService/NotificationService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "NotificationService.dll" ]