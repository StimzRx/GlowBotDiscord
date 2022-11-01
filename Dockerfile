FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GlowBotDiscord/GlowBotDiscord.csproj", "GlowBotDiscord/"]
RUN dotnet restore "GlowBotDiscord/GlowBotDiscord.csproj"
COPY . .
WORKDIR "/src/GlowBotDiscord"
RUN dotnet build "GlowBotDiscord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GlowBotDiscord.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GlowBotDiscord.dll"]
