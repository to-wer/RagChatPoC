FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
EXPOSE 8080

COPY . .

ENTRYPOINT ["dotnet", "RagChatPoC.Api.dll"]