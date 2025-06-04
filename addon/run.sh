#!/usr/bin/with-contenv bashio
set -e

bashio::log.info "Generating /app/GbbConnect2/Parameters.xml from add-on options…"

mkdir -p /app/GbbConnect2

{
  echo '<?xml version="1.0" encoding="utf-8"?>'
  printf '<Parameters Version="1"'

  if bashio::config.true 'server_autostart'; then
    printf ' Server_AutoStart="1"'
  else
    printf ' Server_AutoStart="0"'
  fi

  if bashio::config.true 'is_verbose_log'; then
    printf ' IsVerboseLog="1"'
  else
    printf ' IsVerboseLog="0"'
  fi

  if bashio::config.true 'is_driver_log'; then
    printf ' IsDriverLog="1"'
  else
    printf ' IsDriverLog="0"'
  fi

  if bashio::config.true 'is_driver_log2'; then
    printf ' IsDriverLog2="1"'
  else
    printf ' IsDriverLog2="0"'
  fi

  echo '>'

  printf '  <Plant Version="1"'
  printf ' Number="1"'
  printf ' Name="%s"'              "$(bashio::config 'plant_name')"
  printf ' DriverNo="%s"'          "$(bashio::config 'plant_driver_no')"
  if bashio::config.true 'plant_is_disabled'; then
    printf ' IsDisabled="1"'
  else
    printf ' IsDisabled="0"'
  fi
  printf ' AddressIP="%s"'         "$(bashio::config 'plant_address_ip')"
  printf ' PortNo="%s"'            "$(bashio::config 'plant_port_no')"
  printf ' SerialNumber="%s"'      "$(bashio::config 'plant_serial_number')"
  printf ' GbbOptimizer_PlantId="%s"'    "$(bashio::config 'gbboptimizer_plant_id')"
  printf ' GbbOptimizer_PlantToken="%s"' "$(bashio::config 'gbboptimizer_plant_token')"
  printf ' GbbOptimizer_Mqtt_Address="%s"' "$(bashio::config 'gbboptimizer_mqtt_address')"
  printf ' GbbOptimizer_Mqtt_Port="%s"'    "$(bashio::config 'gbboptimizer_mqtt_port')"

  echo ' />'
  echo '</Parameters>'
} > /app/GbbConnect2/Parameters.xml

bashio::log.info "Wrote /app/GbbConnect2/Parameters.xml successfully."

bashio::log.info "—— Parameters.xml contents ——"
while IFS= read -r line; do
  # Replace the GbbOptimizer_PlantToken value with asterisks for security
  # This is to avoid logging sensitive information in the logs
  masked=$(echo "$line" | sed 's/GbbOptimizer_PlantToken="[^"]*"/GbbOptimizer_PlantToken="*******"/')
  bashio::log.info "$masked"
done < /app/GbbConnect2/Parameters.xml
bashio::log.info "—— End Parameters.xml ——"

bashio::log.info "Launching dotnet GbbConnect2Console.dll…"
dotnet /app/GbbConnect2Console.dll --dont-wait-for-key
bashio::log.info "GbbConnect2Console.dll exited."
bashio::log.info "Exiting add-on."