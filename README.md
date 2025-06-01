# Unity BLE iOS Plugin

A comprehensive Unity plugin for Bluetooth Low Energy (BLE) communication on iOS devices using the CoreBluetooth framework.

## Features

- ✅ **Device Discovery**: Scan for BLE devices with comprehensive advertisement data
- ✅ **Connection Management**: Connect and disconnect from BLE devices  
- ✅ **Device Information**: Access RSSI, service UUIDs, manufacturer data, and more
- ✅ **Event-Driven Architecture**: Subscribe to discovery, connection, and state change events
- ✅ **Connected Device Tracking**: Manage multiple connected devices simultaneously
- ✅ **iOS CoreBluetooth Integration**: Native iOS implementation for optimal performance
- ✅ **Editor Simulation**: Test your BLE logic in the Unity Editor
- ✅ **Comprehensive Error Handling**: Robust error handling and logging

## Installation

### Via Unity Package Manager (Git URL) - Recommended

1. Open Unity Package Manager (Window → Package Manager)
2. Click the "+" button in the top-left corner
3. Select "Add package from git URL..."
4. Enter: `https://github.com/yourusername/UnityBLE2IOS.git`
5. Click "Add"

### Via Unity Package Manager (Local)

1. Clone or download this repository
2. Open Unity Package Manager (Window → Package Manager)
3. Click "+" → "Add package from disk..."
4. Select the `package.json` file from the cloned repository

## Requirements

- **Unity**: 2020.3 or later
- **iOS**: 10.0 or later  
- **Xcode**: 12 or later
- **Platform**: iOS only (uses CoreBluetooth framework)

## Usage

```csharp
// Initialize Bluetooth
BluetoothManager.Instance.Initialize();

// Request permissions
BluetoothManager.Instance.RequestPermissions();

// Start scanning
BluetoothManager.Instance.StartScanning();

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
        // Initialize the Bluetooth manager
        BluetoothManager.Instance.Initialize();
        
        // Subscribe to events
        BluetoothManager.Instance.OnDeviceDiscovered += OnDeviceFound;
        BluetoothManager.Instance.OnDeviceConnected += OnDeviceConnected;
        BluetoothManager.Instance.OnDeviceDisconnected += OnDeviceDisconnected;
        BluetoothManager.Instance.OnPermissionResult += OnPermissionResult;
        
        // Request Bluetooth permissions
        BluetoothManager.Instance.RequestPermissions();
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
- `Initialize()` - Initialize the Bluetooth manager
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

The package includes example scripts in `Samples~/BasicBLEScanner/`:

- **BluetoothExample.cs** - Complete example showing device discovery and connection
- **DeviceListItem.cs** - UI component for displaying device information

Import the samples through Package Manager to see complete usage examples.

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

## Editor Testing

The plugin includes simulation mode for testing in the Unity Editor:

- Simulates realistic BLE device discovery
- Provides sample devices with different characteristics
- Allows testing of UI and logic without iOS device

## Troubleshooting

### Common Issues

1. **No devices found**: Ensure Bluetooth is enabled and app has permission
2. **Connection fails**: Check device is in range and connectable
3. **Build errors**: Verify iOS deployment target is 10.0+

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

For issues, feature requests, or contributions, please visit the [GitHub repository](https://github.com/yourusername/UnityBLE2IOS).
