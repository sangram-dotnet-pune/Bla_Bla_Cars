FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ChatService/ChatService.csproj ChatService/
RUN dotnet restore ChatService/ChatService.csproj

COPY . .
RUN dotnet publish ChatService/ChatService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5006
EXPOSE 5006
ENTRYPOINT ["dotnet", "ChatService.dll"]
