FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["UserService/UserService.csproj", "UserService/"]

RUN dotnet restore "UserService/UserService.csproj"

COPY . .

WORKDIR /src/UserService

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

RUN dotnet ef database update

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5179

ENV ASPNETCORE_URLS=http://+:5179
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "UserService.dll"]
