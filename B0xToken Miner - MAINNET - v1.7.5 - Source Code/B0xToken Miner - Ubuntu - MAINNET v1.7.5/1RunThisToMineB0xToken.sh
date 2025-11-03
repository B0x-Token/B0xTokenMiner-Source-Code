#!/usr/bin/env bash
# Function to check if .NET 6.0 runtime is installed
check_dotnet_6() {
    if command -v dotnet &>/dev/null; then
        if dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 6\."; then
            return 0
        fi
    fi
    return 1
}
# Determine Ubuntu version
UBUNTU_VERSION=$(lsb_release -rs)
echo "Detected Ubuntu $UBUNTU_VERSION"
# Check if .NET 6 runtime is already installed
if check_dotnet_6; then
    echo ".NET 6.0 Runtime is already installed:"
    dotnet --list-runtimes
fi

if ! check_dotnet_6; then
    echo ".NET 6.0 Runtime not found. Installing..."
    # Clean up any broken installations
    sudo apt remove dotnet-runtime-6.0 -y 2>/dev/null
    sudo apt autoremove -y
    sudo rm -rf /usr/share/dotnet 2>/dev/null
    sudo rm -rf ~/.dotnet 2>/dev/null
    # For Ubuntu 22.04 and 24.04, use manual installation method
    if [[ "$UBUNTU_VERSION" == "24.04" ]] || [[ "$UBUNTU_VERSION" == "22.04" ]]; then
        echo "Ubuntu $UBUNTU_VERSION detected - using manual installation method..."
        
        # Install dependencies
        sudo apt-get update
        sudo apt-get install -y wget apt-transport-https
        
        # Download and install .NET 6.0 Runtime manually
        wget https://builds.dotnet.microsoft.com/dotnet/Sdk/6.0.428/dotnet-sdk-6.0.428-linux-x64.tar.gz
        
        # Create directory and extract
        sudo mkdir -p /usr/share/dotnet
        sudo tar zxf dotnet-sdk-6.0.428-linux-x64.tar.gz -C /usr/share/dotnet
        rm sdk-6.0.428-linux-x64-binaries
        
        # Create symbolic link
        sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
        
        # Set environment variables
        echo 'export DOTNET_ROOT=/usr/share/dotnet' >> ~/.bashrc
        echo 'export PATH=$PATH:/usr/share/dotnet' >> ~/.bashrc
        export DOTNET_ROOT=/usr/share/dotnet
        export PATH=$PATH:/usr/share/dotnet
    else
        # For Ubuntu 20.04, use package manager
        echo "Using .NET repo for Ubuntu 20.04"
        
        wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        sudo dpkg -i packages-microsoft-prod.deb
        rm packages-microsoft-prod.deb
        
        sudo apt-get update
        sudo apt-get install -y apt-transport-https
        sudo apt-get update
        sudo apt-get install -y dotnet-runtime-6.0
    fi
    # Verify installation
    echo ""
    echo "Verifying installation..."
    if check_dotnet_6; then
        echo "✓ .NET 6.0 Runtime successfully installed:"
        dotnet --list-runtimes
    else
        echo "✗ Failed to install .NET 6.0 Runtime. Please check for errors."
        exit 1
    fi
fi

# Run the application
echo "Starting MAINNET v1.7.3 B0xToken. Press Ctrl+C to stop gracefully."
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
