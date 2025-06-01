# Basic BLE Scanner Sample

This sample demonstrates how to use the Unity BLE iOS Plugin to scan for and connect to Bluetooth Low Energy devices.

## Files Included

- **BluetoothExample.cs**: Main controller script showing basic BLE operations
- **DeviceListItem.cs**: UI component for displaying discovered devices in a list

## Setup Instructions

1. Import this sample through Unity Package Manager
2. Create a new scene or use an existing one
3. Add the BluetoothExample script to a GameObject
4. Configure UI elements (optional) for device list display

## Features Demonstrated

### Device Discovery
- Initialize Bluetooth manager
- Request user permissions
- Start/stop scanning for devices
- Handle discovered device events

### Device Information
- Display device names and RSSI values
- Show service UUIDs
- Access manufacturer data
- Monitor connection status

### Connection Management
- Connect to discovered devices
- Handle connection success/failure
- Disconnect from devices
- Track connected devices

## Code Example

```csharp
// Initialize and start scanning
BluetoothManager.Instance.Initialize();
BluetoothManager.Instance.OnDeviceDiscovered += OnDeviceFound;
BluetoothManager.Instance.RequestPermissions();

private void OnDeviceFound(BluetoothDevice device)
{
    Debug.Log($"Found: {device.name} - RSSI: {device.rssi}");
    
    // Optionally connect to device
    BluetoothManager.Instance.ConnectToDevice(device.deviceId);
}
```

## UI Integration

The DeviceListItem script can be used to create a scrollable list of discovered devices:

1. Create a ScrollView in your Canvas
2. Add DeviceListItem prefabs to display each device
3. Update the list when new devices are discovered
4. Allow users to tap devices to connect

## Testing

- **In Editor**: The plugin provides simulation mode with fake devices
- **On Device**: Deploy to iOS device and test with real BLE peripherals

## Tips

1. Always request permissions before scanning
2. Stop scanning when not needed to save battery
3. Handle connection timeouts gracefully
4. Test with various BLE device types
5. Check Bluetooth is enabled before operations

## Troubleshooting

**No devices found**: Ensure Bluetooth is enabled and permissions granted
**Connection fails**: Check device is in range and connectable
**Crashes on device**: Verify iOS deployment target is 10.0+
