#!/usr/bin/env bash

# Function to check if .NET 6.0 is installed
check_dotnet_6() {
    if command -v dotnet &> /dev/null; then
        # Check if .NET 6.0 runtime/SDK is available
        if dotnet --list-sdks | grep -q "^6\."; then
            return 0  # .NET 6.0 found
        fi
    fi
    return 1  # .NET 6.0 not found
}

# Check for dotnet 6.0
if ! check_dotnet_6; then
    echo ".NET 6.0 is not found or not installed."
    echo "Installing .NET 6.0..."
    
    # Add Microsoft package signing key and repository
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    
    # Update package list and install .NET 6.0
    sudo apt-get update
    sudo apt-get install -y apt-transport-https
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-6.0
    
    # Verify the installation
    echo "Installed .NET version:"
    dotnet --version
    
    # Check if installation was successful
    if check_dotnet_6; then
        echo ".NET 6.0 is successfully installed."
    else
        echo "Failed to install .NET 6.0. Please check for errors above."
        exit 1
    fi
else
    echo ".NET 6.0 is already installed."
    dotnet --version
fi

# Run the application in a loop
echo "Starting B0xToken.dll..."
while : ; do
    dotnet B0xToken.dll
    echo "Application stopped. Restarting in 15 seconds..."
    sleep 15
done
