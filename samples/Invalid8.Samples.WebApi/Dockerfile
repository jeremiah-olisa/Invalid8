FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5102
EXPOSE 7095

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy entire solution
COPY . .

# Restore and build the specific project
RUN dotnet restore "samples/Invalid8.Samples.WebApi/Invalid8.Samples.WebApi.csproj"
RUN dotnet build "samples/Invalid8.Samples.WebApi/Invalid8.Samples.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "samples/Invalid8.Samples.WebApi/Invalid8.Samples.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Invalid8.Samples.WebApi.dll"]