#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Bunnings.Api/Bunnings.Api.csproj", "Bunnings.Api/"]
RUN dotnet restore "Bunnings.Api/Bunnings.Api.csproj"
COPY . .
WORKDIR "/src/Bunnings.Api"
RUN dotnet build "Bunnings.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bunnings.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bunnings.Api.dll"]