# Unity BLE iOS Plugin - Installation & Setup Guide

## Overview

This guide will walk you through installing and setting up the Unity BLE iOS Plugin in your Unity project.

## Installation Methods

### Method 1: Unity Package Manager (Git URL) - Recommended

This is the easiest method for installing the plugin directly from GitHub.

1. **Open Unity Package Manager**
   - In Unity, go to `Window → Package Manager`

2. **Add Package from Git URL**
   - Click the "+" button in the top-left corner
   - Select "Add package from git URL..."

3. **Enter Repository URL**
   ```
   https://github.com/yourusername/UnityBLE2IOS.git
   ```

4. **Install Package**
   - Click "Add" and wait for Unity to download and install the package
   - The package will appear in your Package Manager under "In Project"

### Method 2: Unity Package Manager (Local)

If you have downloaded or cloned the repository locally:

1. **Get the Package**
   - Clone: `git clone https://github.com/yourusername/UnityBLE2IOS.git`
   - Or download the repository as a ZIP file

2. **Add Local Package**
   - Open Unity Package Manager (`Window → Package Manager`)
   - Click "+" → "Add package from disk..."
   - Navigate to the downloaded/cloned folder
   - Select the `package.json` file
   - Click "Open"

### Method 3: Git Dependency (package.json)

Add the dependency directly to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.yourcompany.unityble2ios": "https://github.com/yourusername/UnityBLE2IOS.git",
    // ... other dependencies
  }
}
```

## Post-Installation Setup

### 1. Verify Installation

After installation, verify the package is properly installed:

1. Check Package Manager shows "Unity BLE iOS Plugin" under "In Project"
2. Verify scripts are available in your project:
   ```csharp
   using UnityBLE2IOS; // Should work without errors
   ```

### 2. Import Samples (Optional)

1. In Package Manager, select "Unity BLE iOS Plugin"
2. Expand "Samples" section
3. Click "Import" next to "Basic BLE Scanner"
4. Sample files will be added to `Assets/Samples/Unity BLE iOS Plugin/1.0.0/Basic BLE Scanner/`

### 3. iOS Build Settings

The plugin automatically configures most iOS settings, but verify:

#### Player Settings
1. Go to `File → Build Settings → iOS → Player Settings`
2. Set **Minimum iOS Version** to `10.0` or later
3. Ensure **Target Device Family** includes iPhone/iPad as needed

#### Info.plist Permissions
The plugin's build processor automatically adds required permissions, but you can verify in Xcode after building:

```xml
<key>NSBluetoothAlwaysUsageDescription</key>
<string>This app uses Bluetooth to connect to nearby devices</string>
<key>NSBluetoothPeripheralUsageDescription</key>
<string>This app uses Bluetooth to connect to nearby devices</string>
```

## First Steps

### 1. Basic Implementation

Create a simple script to test the plugin:

```csharp
using UnityEngine;
using UnityBLE2IOS;

public class BLETest : MonoBehaviour
{
    void Start()
    {
        // Initialize Bluetooth
        BluetoothManager.Instance.Initialize();
        
        // Subscribe to events
        BluetoothManager.Instance.OnPermissionResult += OnPermissionResult;
        BluetoothManager.Instance.OnDeviceDiscovered += OnDeviceDiscovered;
        
        // Request permissions
        BluetoothManager.Instance.RequestPermissions();
    }
    
    private void OnPermissionResult(bool granted)
    {
        if (granted)
        {
            Debug.Log("Bluetooth permission granted!");
            BluetoothManager.Instance.StartScanning();
        }
        else
        {
            Debug.LogError("Bluetooth permission denied!");
        }
    }
    
    private void OnDeviceDiscovered(BluetoothDevice device)
    {
        Debug.Log($"Found device: {device.name} (RSSI: {device.rssi})");
    }
}
```

### 2. Build and Test

1. **Build for iOS**
   - Go to `File → Build Settings`
   - Select iOS platform
   - Click "Build" or "Build and Run"

2. **Test on Device**
   - Deploy to an iOS device (simulator won't work for Bluetooth)
   - Check console logs for Bluetooth activity
   - Ensure permissions are requested and granted

## Troubleshooting Installation

### Common Issues

**Package not found in Package Manager**
- Verify the Git URL is correct
- Check your internet connection
- Try refreshing Package Manager

**Compilation errors after installation**
- Ensure Unity version is 2020.3 or later
- Check that iOS build support is installed
- Restart Unity if needed

**Build errors on iOS**
- Verify iOS deployment target is 10.0+
- Check Xcode version is 12 or later
- Ensure CoreBluetooth framework is available

**Runtime errors on device**
- Verify Bluetooth permissions in device settings
- Check that Bluetooth is enabled
- Test with known BLE devices nearby

### Getting Help

If you encounter issues:

1. Check the [GitHub Issues](https://github.com/yourusername/UnityBLE2IOS/issues)
2. Review the API documentation
3. Test with the included samples
4. Enable verbose logging for debugging

## Next Steps

After successful installation:

1. **Read the API Documentation** - Understand available methods and events
2. **Import and Study Samples** - Learn from working examples
3. **Test in Editor** - Use simulation mode for rapid development
4. **Build and Test** - Verify functionality on real iOS devices
5. **Implement Your Features** - Use the plugin in your specific use case

## Version Management

### Updating the Package

To update to a newer version:

1. **Via Package Manager**: Select the package and click "Update"
2. **Via Git**: Pull latest changes if using local installation
3. **Via manifest.json**: Update the version or branch reference

### Version Pinning

To pin to a specific version, use:
```
https://github.com/yourusername/UnityBLE2IOS.git#v1.0.0
```

This ensures your project uses a specific stable version.
