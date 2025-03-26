FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FootballAPI/FootballAPI.csproj", "FootballAPI/"]
RUN dotnet restore "FootballAPI/FootballAPI.csproj"
COPY . .
WORKDIR "/src/FootballAPI"
RUN dotnet build "FootballAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FootballAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Indicate we're running in a container
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "FootballAPI.dll"]