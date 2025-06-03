###############################################################################
# 1) Base stage: inherit HA base, enable Alpine edge/community, install .NET 9
###############################################################################
ARG BUILD_FROM
FROM $BUILD_FROM AS base

# Enable edge/community for dotnet9-runtime
RUN echo "http://dl-cdn.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories \
    && apk update \
    && apk add --no-cache \
    dotnet9-runtime \
    tzdata \
    libc6-compat \
    ca-certificates \
    && update-ca-certificates

# (Optional) Set timezone if your app needs it
ENV TZ=UTC

WORKDIR /app

###############################################################################
# 2) Build stage: compile the .NET console app with the official SDK
###############################################################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["GbbConnect2Console/GbbConnect2Console.csproj", "GbbConnect2Console/"]
RUN dotnet restore "GbbConnect2Console/GbbConnect2Console.csproj"

# Copy the rest of your source and build
COPY . .
WORKDIR "/src/GbbConnect2Console"
RUN dotnet build "GbbConnect2Console.csproj" -c Release -o /app/build

###############################################################################
# 3) Publish stage: produce a runtime‐only build
###############################################################################
FROM build AS publish
RUN dotnet publish "GbbConnect2Console.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

###############################################################################
# 4) Final stage: copy published output into the Alpine/.NET base
###############################################################################
FROM base AS final

# These labels tell Supervisor that this container is a Home Assistant add-on
LABEL io.hass.version="2025.6.3"
LABEL io.hass.type="addon"
LABEL io.hass.arch="armhf|armv7|aarch64|amd64|i386"

WORKDIR /app
COPY --from=publish /app/publish .

# Copy data for add-on
# Copy data for add-on
COPY run.sh /
RUN chmod a+x /run.sh

# Run run.sh script on container start
CMD [ "/run.sh" ]