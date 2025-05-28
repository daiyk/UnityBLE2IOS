# Unity BLE2iOS Plugin Implementation Guide

## Overview
This Unity plugin provides Bluetooth Low Energy (BLE) functionality for iOS devices. It allows Unity applications to:
- Request Bluetooth permissions
- Scan for and discover BLE devices
- Connect to and disconnect from BLE devices
- Handle connection events and status changes

## File Structure
```
Assets/Plugins/UnityBLE2IOS/
├── Scripts/
│   ├── BluetoothManager.cs          # Main Unity C# interface
│   └── BluetoothDevice.cs           # Device data structure
├── Plugins/iOS/
│   ├── UnityBluetoothManager.mm     # Native iOS implementation
│   └── Info.plist                   # iOS permissions
├── Editor/
│   └── UnityBLE2IOSBuildProcessor.cs # Build-time configuration
├── Examples/
│   ├── BluetoothExample.cs          # Example usage script
│   └── DeviceListItem.cs            # UI component for device list
└── package.json                     # Package metadata
```

## Integration Steps

### 1. Copy Plugin to Unity Project
Copy the entire `UnityBLE2IOS` folder to your Unity project's `Assets/Plugins/` directory.

### 2. Setup Scene
1. Create a new GameObject and attach the `BluetoothExample.cs` script
2. Create UI elements and assign them to the script:
   - Scan button
   - Stop scan button
   - Request permissions button
   - Status text
   - Device list parent (ScrollView content)
   - Device item prefab

### 3. Build Configuration
The plugin automatically configures the iOS build through the `UnityBLE2IOSBuildProcessor.cs` script:
- Adds CoreBluetooth framework
- Sets minimum iOS deployment target to 10.0
- Adds required permission descriptions to Info.plist

## Usage Examples

### Basic Usage
```csharp
using UnityBLE2IOS;

// Initialize
BluetoothManager.Instance.Initialize();

// Request permissions
BluetoothManager.Instance.RequestPermissions();

// Start scanning
BluetoothManager.Instance.StartScanning();

// Subscribe to events
BluetoothManager.Instance.OnDeviceDiscovered += (device) => {
    Debug.Log($"Found device: {device.name}");
};

BluetoothManager.Instance.OnDeviceConnected += (deviceId) => {
    Debug.Log($"Connected to: {deviceId}");
};
```

### Event Handling
```csharp
void Start()
{
    var btManager = BluetoothManager.Instance;
    
    // Bluetooth state changes
    btManager.OnBluetoothStateChanged += (enabled) => {
        if (enabled) {
            Debug.Log("Bluetooth is enabled");
        } else {
            Debug.Log("Bluetooth is disabled");
        }
    };
    
    // Device discovery
    btManager.OnDeviceDiscovered += (device) => {
        Debug.Log($"Discovered: {device.name} ({device.deviceId}) RSSI: {device.rssi}");
    };
    
    // Connection events
    btManager.OnDeviceConnected += (deviceId) => {
        Debug.Log($"Connected to: {deviceId}");
    };
    
    btManager.OnDeviceDisconnected += (deviceId) => {
        Debug.Log($"Disconnected from: {deviceId}");
    };
    
    btManager.OnConnectionFailed += (deviceId, error) => {
        Debug.LogError($"Connection failed: {error}");
    };
    
    // Permission result
    btManager.OnPermissionResult += (granted) => {
        if (granted) {
            Debug.Log("Bluetooth permissions granted");
        } else {
            Debug.LogError("Bluetooth permissions denied");
        }
    };
}
```

## API Reference

### BluetoothManager Class

#### Methods
- `Initialize()` - Initialize the Bluetooth manager
- `RequestPermissions()` - Request Bluetooth permissions from user
- `StartScanning()` - Start scanning for BLE devices
- `StopScanning()` - Stop scanning for BLE devices
- `ConnectToDevice(string deviceId)` - Connect to a specific device
- `DisconnectDevice(string deviceId)` - Disconnect from a specific device
- `IsBluetoothEnabled()` - Check if Bluetooth is enabled
- `IsDeviceConnected(string deviceId)` - Check if device is connected
- `GetDiscoveredDevices()` - Get list of discovered devices

#### Events
- `OnBluetoothStateChanged(bool enabled)` - Bluetooth state changed
- `OnDeviceDiscovered(BluetoothDevice device)` - New device discovered
- `OnDeviceConnected(string deviceId)` - Device connected
- `OnDeviceDisconnected(string deviceId)` - Device disconnected
- `OnConnectionFailed(string deviceId, string error)` - Connection failed
- `OnPermissionResult(bool granted)` - Permission request result

### BluetoothDevice Class

#### Properties
- `string deviceId` - Unique device identifier
- `string name` - Device name
- `int rssi` - Signal strength
- `bool isConnectable` - Whether device can be connected to
- `string[] serviceUUIDs` - Available service UUIDs

## iOS Build Requirements

### Minimum Requirements
- iOS 10.0 or later
- Xcode 12 or later
- CoreBluetooth framework

### Permissions
The plugin automatically adds these permissions to Info.plist:
- `NSBluetoothAlwaysUsageDescription`
- `NSBluetoothPeripheralUsageDescription`

## Testing

### In Unity Editor
The plugin includes simulation code for testing in the Unity Editor:
- Simulates permission granted
- Creates mock devices for testing UI
- Provides debug output

### On iOS Device
1. Build and deploy to iOS device
2. Grant Bluetooth permissions when prompted
3. Test scanning and connection functionality

## Troubleshooting

### Common Issues

1. **Permissions Not Granted**
   - Ensure Info.plist contains proper permission descriptions
   - Check iOS Settings > Privacy > Bluetooth

2. **Devices Not Found**
   - Ensure target devices are in advertising mode
   - Check Bluetooth is enabled on iOS device
   - Verify scanning is active

3. **Connection Failures**
   - Check device is still in range
   - Verify device supports BLE connections
   - Check for interference or multiple connection attempts

### Debug Logging
Enable debug logging by checking Unity Console and Xcode console for detailed information about:
- Bluetooth state changes
- Device discovery events
- Connection attempts and results
- Permission requests

## Extension Points

The plugin can be extended to support:
- Service and characteristic discovery
- Reading/writing characteristic values
- Notification subscriptions
- Custom service filtering
- RSSI monitoring
- Connection parameter updates

## Support
For issues and feature requests, please refer to the project documentation or create an issue in the project repository.
