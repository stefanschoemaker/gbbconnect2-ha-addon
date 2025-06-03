
# GbbConnect2

Program to connect inverters (eg: Deye) and program [GbbOptimizer.gbbsoft.pl.](https://GbbOptimizer.gbbsoft.pl/)

To connect with inverters program uses SolarmanV5 protocol. (Loggers serial numers: 17xxxxxxx, 21xxxxxxx or 40xxxxxxx) (maybe also: 27xxxxxxx, 23xxxxxxx)

To connect with GbbOptimizer program uses Mqtt server and own protocol.

GbbConnect remarks:
- Data on disk are grouped using number in column "No". So if you want start new inverter with new data then put new No.

## Download

Last Windows version download: [GbbConnect2.msi](http://www.gbbsoft.pl/!download/GbbConnect2/GbbConnect2Setup.msi) [setup.exe](http://www.gbbsoft.pl/!download/GbbConnect2/setup.exe)

## Connection to inverter

if first connection failed than program tries to connect every 5 minutes.

## Connection to mqtt

If first connection to mqtt failed then program tries to connect every 5 minutes.

Program every minute sends keepalive messave to mqtt. If connected has been lost then every minute program tries to reconect.

## Setup in [GbbOptimizer](https://GbbOptimizer.gbbsoft.pl/)

Manual how setup GbbConnect2 with Deye and GbbVictronWeb: [Manual](https://gbboptimizer.gbbsoft.pl/Manual?Filters.PageNo=31)

## History

[https://github.com/gbbsoft/GbbConnect2/activity?ref=master](https://github.com/gbbsoft/GbbConnect2/activity?ref=master)

# GbbConnect2Console

Program on console.

## Download

Last version download: [GbbConnect2Console.zip](http://www.gbbsoft.pl/!download/GbbConnect2/GbbConnect2Console.zip)

## Parameters

-? - list of parameters

--dont-wait-for-key -  don't wait for key, but just wait forever

# GbbConnectConsole on Docker

Program GbbConnect2Console can be run in docker. File Dockerfile is present in root directory.

## Configuration file

You can use GbbConnect program to create (and test) configuration file (My Documents\GbbConnect2\Parameters.xml). Then file can be move as /root/GbbConnect2/Parameters.xml on docker container.

## How to run on Docker

- Install docker (or DockerDesktop)
- Clone GbbConnect2: git clone https://github.com/gbbsoft/GbbConnect2
- enter to GbbConnect2 directory: cd GbbConnect2
- run from GbbConnect2 directory: docker build . -t gbbconnect2image
- create container: docker container create -i -t --name gbbconnect2console gbbconnect2image
- copy to current directory file Parameters.xml from Windows version (c:\Users\<user>\Dokuments\GbbConnect2\Parameters.xml) or create based on exmaple below
- copy Parameters.xml to container: docker cp ./Parameters.xml gbbconnect2console:/app/Parameters.xml
- start container: docker start gbbconnect2console
- make container always running: docker update --restart unless-stopped gbbconnect2console

## Sample Parameters.xml file

```
<?xml version="1.0" encoding="utf-8"?>
<Parameters Version="1"
            Server_AutoStart="0"
            IsVerboseLog="1"
            IsDriverLog="1"
            IsDriverLog2="1">
	<Plant Version="1"
	       Number="1"
	       Name="Any text"
	       DriverNo="0"
	       IsDisabled="0"
	       AddressIP="1.2.3.4"
	       PortNo="8899"
	       SerialNumber="123456"
	       GbbOptimizer_PlantId="<PlantId>"
	       GbbOptimizer_PlantToken="<PlantToken>"
	       GbbOptimizer_Mqtt_Address="gbboptimizerX-mqtt.gbbsoft.pl"
	       GbbOptimizer_Mqtt_Port="8883"/>
</Parameters>
```

DriverNo="0" - SolarmanV5 (wifi-dongle)
DriverNo="1" - ModBucTCP (wired-dongle)

Remark: SerialNumber: it is dongle SN, not inverter SN!

# Script for  GbbConnect2Console on Linux (Debian 12 Example) (Author: https://github.com/SpengeSec)

Script for compete setup of GbbConnect2Console: [https://github.com/Sp3nge/GbbConnect2Console-Installer](https://github.com/Sp3nge/GbbConnect2Console-Installer)

Instructional video: https://www.youtube.com/watch?v=1x1-wjyAsrs

... or do it manually with following two steps:

# Compiling GbbConnect2Console on Linux (Debian 12 Example) (Author: https://github.com/SpengeSec)

This guide explains how to compile the `GbbConnect2Console` application for Linux. The output will be a self-contained, single-file executable that can run on compatible Linux systems without requiring a separate .NET runtime installation.

These instructions assume you are on a Debian 12 based system. Adapt package installation commands for other distributions if necessary.

### Prerequisites

1.  **Operating System:** A Linux distribution (e.g., Debian 12, Ubuntu 22.04+).
2.  **Git:** To clone the repository.
    ```bash
    sudo apt update
    sudo apt install git -y
    ```
3.  **.NET 9.0 SDK (or newer compatible version):** The project targets .NET 9.0.
    ```bash
    # Download Microsoft package signing key and repository configuration
    wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb

    # Update package lists and install the .NET 9.0 SDK
    sudo apt update
    sudo apt install -y apt-transport-https
    sudo apt update
    sudo apt install -y dotnet-sdk-9.0
    ```
    Verify the installation:
    ```bash
    dotnet --version
    # Expected output: 9.0.xxx or higher
    ```

### Compilation Steps

1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/gbbsoft/GbbConnect2.git
    cd GbbConnect2
    ```

2.  **Navigate to the Console Project Directory:**
    ```bash
    cd GbbConnect2Console
    ```

3.  **Clean Previous Build Artifacts (Optional but Recommended for a Fresh Build):**
    ```bash
    rm -rf ./bin ./obj ./publish_output_self_contained
    ```

4.  **Publish the Application:**
    This command compiles the application in `Release` mode, targets `linux-x64`, creates a self-contained deployment (includes the .NET runtime), and packages it into a single executable file.
    ```bash
    dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish_output_self_contained /p:PublishSingleFile=true
    ```
    *   `-c Release`: Compiles in Release configuration (optimized).
    *   `-r linux-x64`: Specifies the target runtime identifier as Linux 64-bit.
    *   `--self-contained true`: Creates a self-contained deployment.
    *   `-o ./publish_output_self_contained`: Specifies the output directory for the published files.
    *   `/p:PublishSingleFile=true`: Packages the application and its dependencies into a single executable.


### Running the Compiled Application

1.  **Navigate to the Output Directory:**
    ```bash
    cd ./publish_output_self_contained
    ```
    You should find the executable `GbbConnect2Console` here.

2.  **Make the File Executable:**
    ```bash
    chmod +x GbbConnect2Console
    ```

3. **Parameters.xml** file must be in publish_output_self_contained directory

4.  **Run the Application:**
    You can now run the application. To see available command-line options:
    ```bash
    ./GbbConnect2Console --dont-wait-for-key
    ```
	

# Auto-Starting GbbConnect2Console on Linux with systemd (Author: https://github.com/SpengeSec)

This guide will show you how to configure your compiled `GbbConnect2.Console` application to run as a background service on Linux. This service will automatically start when the system boots and restart if it crashes.

**Prerequisites:**
*   Your `GbbConnect2.Console` application has been compiled for Linux (e.g., into a self-contained executable).
*   The application supports a command-line argument like `--dont-wait-for-key` to run without waiting for user input.
*   You have `sudo` (administrator) privileges on your Linux system.
*   The compiled application files (including the main executable and `Parameters.xml`) are ready.

**Assumptions:**
*   Your compiled application files are currently in a directory like `/root/GbbConnect2/GbbConnect2Console/publish_output_self_contained/`. (You will move them to `/opt/gbbconnect2console/`).
*   The main executable is named `GbbConnect2Console`.

### Steps:

**1. Create a Dedicated User for the Service (Recommended for Security)**

It's best practice to run services under a dedicated, non-privileged user account.
```bash
sudo useradd --system --no-create-home --shell /usr/sbin/nologin gbbconsoleuser
```

**2. Deploy Application Files to a Standard Location**

Move your application files to a common directory for services, like `/opt/`.
```bash
# Create the application directory
sudo mkdir -p /opt/gbbconnect2console

# Copy your application files (replace source path if different)
# Example source path: /root/GbbConnect2/GbbConnect2Console/publish_output_self_contained/
sudo cp -r /path/to/your/publish_output_self_contained/* /opt/gbbconnect2console/

# Set correct ownership and permissions
sudo chown -R gbbconsoleuser:gbbconsoleuser /opt/gbbconnect2console
sudo chmod +x /opt/gbbconnect2console/GbbConnect2Console
```
*Ensure your `Parameters.xml` file is copied to `/opt/gbbconnect2console/` along with the executable.*

**3. Create the systemd Service File**

This file tells `systemd` how to manage your application. Create it with:
```bash
sudo nano /etc/systemd/system/gbbconnect2console.service
```
Paste the following content into the file. **Read the comments and adjust if needed.**
```ini
[Unit]
Description=GbbConnect2 Console Application Service
After=network-online.target # Ensures network is up before starting

[Service]
Type=simple
# User and Group to run the service as (created in Step 1)
User=gbbconsoleuser
Group=gbbconsoleuser

# Working directory for the application
# This is important if your app reads files (like Parameters.xml) from its current directory
WorkingDirectory=/opt/gbbconnect2console

# Command to start the application
# Make sure the path to your executable is correct and include the --dont-wait-for-key flag
ExecStart=/opt/gbbconnect2console/GbbConnect2Console --dont-wait-for-key

# Restart policy
Restart=always    # Or "on-failure"
RestartSec=10     # Seconds to wait before restarting

# Logging configuration
StandardOutput=journal
StandardError=journal
SyslogIdentifier=gbbconnect2console

# Optional: Disable .NET telemetry for deployed apps
Environment="DOTNET_PRINT_TELEMETRY_MESSAGE=false"

[Install]
WantedBy=multi-user.target
```
Save the file and exit (for `nano`: `Ctrl+X`, then `Y`, then `Enter`).

**4. Enable and Start the Service**

```bash
# Reload systemd to recognize the new service file
sudo systemctl daemon-reload

# Enable the service to start automatically on boot
sudo systemctl enable gbbconnect2console.service

# Start the service immediately
sudo systemctl start gbbconnect2console.service
```

**5. Verify the Service**

```bash
# Check the status of the service
sudo systemctl status gbbconnect2console.service
```
You should see `Active: active (running)`.

```bash
# View the latest logs from your service (e.g., last 50 lines)
sudo journalctl -u gbbconnect2console.service -n 50 --no-pager

# To follow logs in real-time (press Ctrl+C to stop)
sudo journalctl -f -u gbbconnect2console.service
```

### Managing the Service

*   **To stop the service:**
    ```bash
    sudo systemctl stop gbbconnect2console.service
    ```
*   **To restart the service (e.g., after updating `Parameters.xml` or the application):**
    ```bash
    sudo systemctl restart gbbconnect2console.service
    ```
*   **To disable the service from starting on boot:**
    ```bash
    sudo systemctl disable gbbconnect2console.service
    ```

Your `GbbConnect2.Console` application is now configured to run persistently as a service!
