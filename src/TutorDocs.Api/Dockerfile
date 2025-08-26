FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TutorDocs.Api/TutorDocs.Api.csproj", "TutorDocs.Api/"]
COPY ["src/TutorDocs.Shared/TutorDocs.Shared.csproj", "TutorDocs.Shared/"]
COPY ["src/TutorDocs.Lambda.ChunkEmbed/TutorDocs.Lambda.ChunkEmbed.csproj", "TutorDocs.Lambda.ChunkEmbed/"]
COPY ["src/TutorDocs.Lambda.ConvertToPdf/TutorDocs.Lambda.ConvertToPdf.csproj", "TutorDocs.Lambda.ConvertToPdf/"]

RUN dotnet restore "TutorDocs.Api/TutorDocs.Api.csproj"
COPY . .
WORKDIR "/src/TutorDocs.Api"
RUN dotnet build "TutorDocs.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TutorDocs.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "TutorDocs.Api.dll"]
