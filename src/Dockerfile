FROM mcr.microsoft.com/dotnet/aspnet:7.0-focal AS base
RUN apt-get update -y
RUN apt-get install -y tzdata
WORKDIR /app
FROM mcr.microsoft.com/dotnet/sdk:7.0-focal AS build
WORKDIR /src
# Copy the main source project files
COPY . .
RUN dotnet restore "Lisbeth.Bot.API/Lisbeth.Bot.API.csproj" --no-cache
COPY . .
WORKDIR "/src/."
RUN dotnet build "Lisbeth.Bot.API/Lisbeth.Bot.API.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Lisbeth.Bot.API/Lisbeth.Bot.API.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV TZ Europe/Berlin
ENTRYPOINT ["dotnet", "Lisbeth.Bot.API.dll"]
