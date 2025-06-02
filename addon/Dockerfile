# -------------------------------------------------------------------
# 1) Base image for running the compiled app
# -------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# -------------------------------------------------------------------
# 2) Build stage
# -------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GbbConnect2Console/GbbConnect2Console.csproj", "GbbConnect2Console/"]
RUN dotnet restore "GbbConnect2Console/GbbConnect2Console.csproj"
COPY . .
WORKDIR "/src/GbbConnect2Console"
RUN dotnet build "GbbConnect2Console.csproj" -c Release -o /app/build

# -------------------------------------------------------------------
# 3) Publish stage
# -------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish "GbbConnect2Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------------------------------------------------
# 4) Final stage: copy published files into the runtime image
# -------------------------------------------------------------------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GbbConnect2Console.dll"]