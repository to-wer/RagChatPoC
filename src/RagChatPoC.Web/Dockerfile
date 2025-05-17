FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RagChatPoC.Web/RagChatPoC.Web/RagChatPoC.Web.csproj", "RagChatPoC.Web/RagChatPoC.Web/"]
COPY ["RagChatPoC.Web/RagChatPoC.Web.Client/RagChatPoC.Web.Client.csproj", "RagChatPoC.Web/RagChatPoC.Web.Client/"]
RUN dotnet restore "RagChatPoC.Web/RagChatPoC.Web/RagChatPoC.Web.csproj"
COPY . .
WORKDIR "/src/RagChatPoC.Web/RagChatPoC.Web"
RUN dotnet build "./RagChatPoC.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RagChatPoC.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RagChatPoC.Web.dll"]
