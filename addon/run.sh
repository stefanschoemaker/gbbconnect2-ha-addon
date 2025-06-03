#!/usr/bin/env bashio
set -e

mkdir -p /root/GbbConnect

bashio::log.info "Generating /root/GbbConnect/Parameters.xml from add-on options…"

{
  echo '<?xml version="1.0" encoding="utf-8"?>'
  printf '<Parameters Version="1"'

  # Top-level switches: only emit if set to true
  if bashio::config.true 'server_autostart'; then
    printf ' Server_AutoStart="1"'
  fi
  if bashio::config.true 'is_verbose_log'; then
    printf ' IsVerboseLog="1"'
  fi
  if bashio::config.true 'is_driver_log'; then
    printf ' IsDriverLog="1"'
  fi
  if bashio::config.true 'is_driver_log2'; then
    printf ' IsDriverLog2="1"'
  fi

  echo '>'

  # Begin <Plant …>
  printf '  <Plant Version="1"'

  if bashio::config.has_value 'plant_number'; then
    pn=$(bashio::config 'plant_number')
    printf ' Number="%s"' "$pn"
  fi

  if bashio::config.has_value 'plant_name'; then
    name=$(bashio::config 'plant_name')
    [[ -n "$name" ]] && printf ' Name="%s"' "$name"
  fi

  if bashio::config.has_value 'plant_driver_no'; then
    dr=$(bashio::config 'plant_driver_no')
    printf ' DriverNo="%s"' "$dr"
  fi

  # This used to be int(0,1); now skip unless the switch is true
  if bashio::config.true 'plant_is_disabled'; then
    printf ' IsDisabled="1"'
  fi

  if bashio::config.has_value 'plant_address_ip'; then
    ip=$(bashio::config 'plant_address_ip')
    [[ -n "$ip" ]] && printf ' AddressIP="%s"' "$ip"
  fi

  if bashio::config.has_value 'plant_port_no'; then
    pport=$(bashio::config 'plant_port_no')
    printf ' PortNo="%s"' "$pport"
  fi

  if bashio::config.has_value 'plant_serial_number'; then
    sn=$(bashio::config 'plant_serial_number')
    [[ -n "$sn" ]] && printf ' SerialNumber="%s"' "$sn"
  fi

  if bashio::config.has_value 'gbboptimizer_plant_id'; then
    pid=$(bashio::config 'gbboptimizer_plant_id')
    [[ -n "$pid" ]] && printf ' GbbOptimizer_PlantId="%s"' "$pid"
  fi

  if bashio::config.has_value 'gbboptimizer_plant_token'; then
    ptoken=$(bashio::config 'gbboptimizer_plant_token')
    [[ -n "$ptoken" ]] && printf ' GbbOptimizer_PlantToken="%s"' "$ptoken"
  fi

  if bashio::config.has_value 'gbboptimizer_mqtt_address'; then
    maddr=$(bashio::config 'gbboptimizer_mqtt_address')
    [[ -n "$maddr" ]] && printf ' GbbOptimizer_Mqtt_Address="%s"' "$maddr"
  fi

  if bashio::config.has_value 'gbboptimizer_mqtt_port'; then
    mport=$(bashio::config 'gbboptimizer_mqtt_port')
    printf ' GbbOptimizer_Mqtt_Port="%s"' "$mport"
  fi

  # Close <Plant/> and </Parameters>
  echo ' />'
  echo '</Parameters>'
} > /root/GbbConnect/Parameters.xml

bashio::log.info "Wrote /root/GbbConnect/Parameters.xml successfully."

bashio::log.info "Launching dotnet GbbConnect2Console.dll…"
exec dotnet /app/GbbConnect2Console.dll