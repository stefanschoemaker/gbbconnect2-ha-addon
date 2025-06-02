#!/usr/bin/env bashio
set -e

# 1) Make sure the GbbConnect folder exists
mkdir -p /root/GbbConnect

# 2) Write out whatever the user put under "parameters_xml" in config.json
bashio::log.info "Writing /root/GbbConnect/Parameters.xml from add-on config…"
bashio::config.get "parameters_xml" > /root/GbbConnect/Parameters.xml

# 3) Start the .NET app
bashio::log.info "Launching dotnet GbbConnect2Console.dll…"
exec dotnet /app/GbbConnect2Console.dll
