# Unity BLE iOS Plugin

A comprehensive Unity plugin for Bluetooth Low Energy (BLE) communication on iOS devices using the CoreBluetooth framework.

## Features

- ✅ **Auto-Initialization**: Automatically initializes before scene load - no manual setup required
- ✅ **Smart GameObject Management**: Automatically finds or creates "BluetoothManager" GameObject
- ✅ **Device Discovery**: Scan for BLE devices with comprehensive advertisement data
- ✅ **Connection Management**: Connect and disconnect from BLE devices  
- ✅ **Device Information**: Access RSSI, service UUIDs, manufacturer data, and more
- ✅ **Event-Driven Architecture**: Subscribe to discovery, connection, and state change events
- ✅ **Connected Device Tracking**: Manage multiple connected devices simultaneously
- ✅ **iOS CoreBluetooth Integration**: Native iOS implementation for optimal performance
- ✅ **Editor Simulation**: Test your BLE logic in the Unity Editor
- ✅ **Comprehensive Error Handling**: Robust error handling and logging

## Installation

### Via Unity Package (.unitypackage) - Recommended

1. Go to the [GitHub repository releases page](https://github.com/daiyk/UnityBLE2IOS/releases)
2. Download the latest `UnityBLE2IOS.unitypackage` file
3. In Unity, go to **Assets** → **Import Package** → **Custom Package...**
4. Select the downloaded `.unitypackage` file
5. Click **Import** to add the plugin to your project

### Via Unity Package Manager (Git URL)

1. Open Unity Package Manager (Window → Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/daiyk/UnityBLE2IOS.git`
5. Click "Add"

## Requirements

- **Unity**: 2022.3 or later
- **iOS**: 10.0 or later  
- **Xcode**: 12 or later
- **Platform**: iOS only (uses CoreBluetooth framework)

## Unity Editor Simulation

⚠️ **Important**: When running in the Unity Editor, this plugin uses **simulated devices** for debugging purposes. It does **NOT** perform actual Bluetooth scanning or connections.

The plugin provides mock BLE devices and simulated connection behavior in the Unity Editor to allow you to develop and test your UI and application logic without needing an iOS device. Real Bluetooth operations only work when deployed to an actual iOS device.

## How It Works

### Auto-Initialization
The BluetoothManager automatically initializes when your app starts using Unity's `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)`. This means:

- ✅ **No manual initialization required** - Just access `BluetoothManager.Instance`
- ✅ **Smart GameObject management** - Automatically looks for existing "BluetoothManager" GameObject or creates one
- ✅ **Persistent across scenes** - The manager persists throughout your app lifecycle
- ✅ **Component auto-attachment** - Automatically adds BluetoothManager component if missing

### GameObject Management
The plugin follows this logic:
1. **Looks for existing GameObject** named "BluetoothManager" in the scene
2. **Adds component if missing** - Attaches BluetoothManager component if GameObject exists but component is missing
3. **Creates new GameObject** - If no "BluetoothManager" GameObject found, creates one automatically
4. **Persists across scenes** - Uses `DontDestroyOnLoad()` to maintain the GameObject

## Usage

```csharp

// Start scanning for devices
BluetoothManager.Instance.StartScanning();

// Request permissions if needed
BluetoothManager.Instance.RequestPermissions();

// Connect to a device
BluetoothManager.Instance.ConnectToDevice(deviceId);
```

## Quick Start

```csharp
using UnityBLE2IOS;
using UnityEngine;

public class BLEController : MonoBehaviour
{
    void Start()
    {
        var bluetoothManager = BluetoothManager.Instance;
        
        // Subscribe to events
        bluetoothManager.OnDeviceDiscovered += OnDeviceFound;
        bluetoothManager.OnDeviceConnected += OnDeviceConnected;
        bluetoothManager.OnDeviceDisconnected += OnDeviceDisconnected;
        bluetoothManager.OnPermissionResult += OnPermissionResult;
        
        // Request permissions (optional - auto-requested on first use)
        bluetoothManager.RequestPermissions();
    }
    
    private void OnPermissionResult(bool granted)
    {
        if (granted)
        {
            Debug.Log("Bluetooth permission granted - starting scan");
            BluetoothManager.Instance.StartScanning();
        }
        else
        {
            Debug.LogError("Bluetooth permission denied");
        }
    }
    
    private void OnDeviceFound(BluetoothDevice device)
    {
        Debug.Log($"Found device: {device.name} (RSSI: {device.rssi})");
        Debug.Log($"Services: {string.Join(", ", device.serviceUUIDs)}");
        
        // Connect to the first device found (example)
        BluetoothManager.Instance.ConnectToDevice(device.deviceId);
    }
    
    private void OnDeviceConnected(string deviceId)
    {
        Debug.Log($"Connected to device: {deviceId}");
        
        // Get connected device info
        var device = BluetoothManager.Instance.GetConnectedDevice(deviceId);
        if (device != null)
        {
            Debug.Log($"Connected device name: {device.name}");
        }
    }
    
    private void OnDeviceDisconnected(string deviceId)
    {
        Debug.Log($"Disconnected from device: {deviceId}");
    }
}
```

## API Reference

### BluetoothManager (Singleton)

#### Core Methods
- `RequestPermissions()` - Request Bluetooth permissions from user
- `StartScanning()` - Start scanning for BLE devices
- `StopScanning()` - Stop scanning for BLE devices
- `ConnectToDevice(string deviceId)` - Connect to a specific device
- `DisconnectDevice(string deviceId)` - Disconnect from a device
- `DisconnectAllDevices()` - Disconnect from all connected devices

#### Device Information
- `GetDiscoveredDevices()` - Get list of all discovered devices
- `GetConnectedDevices()` - Get list of all connected devices
- `GetConnectedDevice(string deviceId)` - Get specific connected device
- `GetDiscoveredDevice(string deviceId)` - Get specific discovered device
- `IsDeviceConnected(string deviceId)` - Check if device is connected
- `IsDeviceDiscovered(string deviceId)` - Check if device was discovered
- `ClearDiscoveredDevices()` - Clear the discovered devices list
- `GetDiscoveredDeviceByIndex(int index)` - Get discovered device by index from native layer

#### Status Methods
- `IsBluetoothEnabled()` - Check if Bluetooth is enabled
- `GetConnectionStatus()` - Get comprehensive status summary
- `GetConnectedDeviceCount()` - Get number of connected devices
- `GetDiscoveredDeviceCount()` - Get number of discovered devices

#### Events
- `OnBluetoothStateChanged` - Bluetooth enabled/disabled
- `OnDeviceDiscovered` - New device discovered
- `OnDeviceConnected` - Device connected successfully
- `OnDeviceDisconnected` - Device disconnected
- `OnConnectionFailed` - Connection attempt failed
- `OnPermissionResult` - Bluetooth permission result

### BluetoothDevice

Properties available for each discovered/connected device:

```csharp
public class BluetoothDevice
{
    public string deviceId;           // Unique device identifier
    public string name;               // Device name
    public int rssi;                  // Signal strength
    public bool isConnectable;        // Whether device accepts connections
    public string[] serviceUUIDs;     // Advertised service UUIDs
    public string manufacturerData;   // Manufacturer-specific data (hex string)
    public string localName;          // Local name from advertisement
    public int txPowerLevel;          // Transmission power level
}
```

## Examples

The package includes a comprehensive sample:

### Unity BLE Sample (`Samples~/UnityBLESample/`)
A complete BLE sample scene with full UI implementation:
- **BLEStatusController.cs** - Main controller with comprehensive BLE management
- **BLEDeviceItem.cs** - Interactive device list item component
- **UnityBLESample.unity** - Complete sample scene
- **BLE_Device.prefab** - Device list item prefab

Features:
- Interactive device scanning and connection
- Real-time device list with RSSI updates
- Debug console with operation logging
- Connection status management
- Visual feedback for device selection

Import the sample through Package Manager to see a complete usage example.

## iOS Build Settings

The plugin automatically configures required iOS settings, but ensure:

1. **Info.plist** includes Bluetooth usage descriptions:
   ```xml
   <key>NSBluetoothAlwaysUsageDescription</key>
   <string>This app uses Bluetooth to connect to nearby devices</string>
   <key>NSBluetoothPeripheralUsageDescription</key>
   <string>This app uses Bluetooth to connect to nearby devices</string>
   ```

2. **Minimum iOS Version**: Set to iOS 10.0 or later

## Troubleshooting

### Common Issues

1. **No devices found**: Ensure Bluetooth is enabled and app has permission
2. **Connection fails**: Check device is in range and connectable
3. **Build errors**: Verify iOS deployment target is 10.0+
4. **"Object BluetoothManager not found" error**: The GameObject is auto-created, but ensure you're accessing `BluetoothManager.Instance` before native callbacks occur

### GameObject Setup
If you want to manually place a "BluetoothManager" GameObject in your scene:

1. Create an empty GameObject in your scene
2. Name it exactly "BluetoothManager"
3. The plugin will automatically attach the BluetoothManager component
4. The GameObject will persist across scene changes automatically

### Auto-Initialization Troubleshooting

```csharp
void Start() 
{
    // Force GameObject creation and verify setup
    var manager = BluetoothManager.Instance;
    
    // Check if properly initialized
    Debug.Log($"Bluetooth enabled: {manager.IsBluetoothEnabled()}");
    Debug.Log($"Manager GameObject: {manager.gameObject.name}");
}
```

### Debug Logging

Enable verbose logging to troubleshoot issues:

```csharp
// Check connection status
Debug.Log(BluetoothManager.Instance.GetConnectionStatus());

// Monitor discovered devices
foreach(var device in BluetoothManager.Instance.GetDiscoveredDevices())
{
    Debug.Log($"Device: {device.name}, RSSI: {device.rssi}");
}
```

## License

MIT License - see LICENSE file for details.

## Support

For issues, feature requests, or contributions, please visit the [GitHub repository](https://github.com/daiyk/UnityBLE2IOS).
