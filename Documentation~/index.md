# Unity BLE iOS Plugin Documentation

## Overview

This documentation provides detailed information about the Unity BLE iOS Plugin, a comprehensive solution for Bluetooth Low Energy communication on iOS devices.

## Architecture

The plugin consists of several key components:

### C# Layer (`Runtime/Scripts/`)
- **BluetoothManager.cs**: Main singleton class that manages all BLE operations
- **BluetoothDevice.cs**: Data structure representing a BLE device

### Native iOS Layer (`Runtime/Plugins/iOS/`)
- **UnityBluetoothManager.mm**: Objective-C++ implementation using CoreBluetooth

### Editor Support (`Editor/`)
- **UnityBLE2IOSBuildProcessor.cs**: Automatically configures iOS build settings

### Examples (`Samples~/`)
- **BluetoothExample.cs**: Complete example implementation
- **DeviceListItem.cs**: UI component for device display

## Data Flow

1. **Initialization**: BluetoothManager initializes CoreBluetooth on iOS
2. **Permission Request**: App requests Bluetooth permissions from user
3. **Device Discovery**: CoreBluetooth scans and reports discovered devices
4. **Data Parsing**: Advertisement data is parsed and cached
5. **Connection Management**: Devices can be connected/disconnected
6. **Event Notification**: C# callbacks notify about state changes

## Memory Management

The plugin uses efficient memory management:
- Device data is cached in native dictionaries
- Strings are safely passed between Objective-C and C#
- Automatic cleanup on disconnection
- No memory leaks in continuous scanning

## Threading

- CoreBluetooth operations run on the main thread
- Unity callbacks are invoked on the Unity main thread
- Thread-safe data structures for device storage

## Error Handling

Comprehensive error handling includes:
- Bluetooth state monitoring
- Connection failure detection
- Invalid device handling
- JSON parsing error recovery
- Graceful degradation when Bluetooth is unavailable

## Platform Requirements

### iOS
- iOS 10.0 or later
- CoreBluetooth framework
- Bluetooth hardware capability

### Unity
- Unity 2020.3 or later
- iOS build support
- Assembly definition support

## Best Practices

1. **Always check permissions** before starting operations
2. **Handle connection failures** gracefully
3. **Stop scanning** when not needed to save battery
4. **Use events** rather than polling for state changes
5. **Test thoroughly** on actual iOS devices

## Performance Considerations

- Scanning consumes battery power
- Connection attempts have timeouts
- Multiple simultaneous connections impact performance
- RSSI updates are rate-limited by iOS

## Security

- User permission required for Bluetooth access
- No sensitive data stored in plain text
- Secure communication through CoreBluetooth APIs
- Privacy-compliant device identification
