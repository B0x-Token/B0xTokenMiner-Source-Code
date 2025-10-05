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


# Run the application
echo "Starting MAINNET B0xToken. Press Ctrl+C to stop gracefully."
echo "========================================="

dotnet B0xToken.dll
DOTNET_PID=$!

# Wait for the process to complete or be interrupted
wait "$DOTNET_PID"

echo "Press any two keys to exit the terminal.  Miner is stopped."
# Wait for any keypress
read -n 1 -s

echo ""
echo "Key pressed once. Press again and exit Terminal for B0xToken..."
read -n 1 -s

echo ""
echo "Second Key pressed. Exiting Terminal for B0xToken... in 3 seconds"

# Optional: Add a small delay so user can see the final message
sleep 3
