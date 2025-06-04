###############################################################################
# 1) Base stage: inherit HA base, enable Alpine edge/community, install .NET 9
###############################################################################
ARG BUILD_FROM
FROM $BUILD_FROM AS base

# Enable edge/community (for dotnet9-runtime)
RUN echo "http://dl-cdn.alpinelinux.org/alpine/edge/community" \
      >> /etc/apk/repositories \
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
# 2) Build stage: clone GbbConnect2 from GitHub and compile the .NET console app
#    (using Alpine‐based .NET SDK)
###############################################################################
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# 2a) Install git so we can pull the repo
RUN apk add --no-cache git

# 2b) Clone only the latest commit of GbbConnect2 (depth=1) to save time
RUN git clone --depth 1 https://github.com/gbbsoft/GbbConnect2.git .

# 2c) Restore NuGet packages for the console subproject
WORKDIR /src/GbbConnect2Console
RUN dotnet restore "GbbConnect2Console.csproj"

# 2d) Build in Release mode; output goes to /app/build in the SDK container
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

# 4c) Copy run.sh into the image root, make it executable
COPY run.sh /run.sh
RUN chmod +x /run.sh

# 4d) Use s6’s /init as entrypoint, then run.sh as CMD
ENTRYPOINT ["/init"]
CMD ["/run.sh"]