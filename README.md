# Unity Bluetooth iOS Plugin

A Unity plugin that provides Bluetooth Low Energy (BLE) functionality for iOS devices.

## Features

- Request Bluetooth permissions
- Scan for BLE devices
- Connect to BLE devices
- Read/Write characteristics
- Handle connection events

## Installation

1. Copy the `UnityBLE2IOS` folder to your Unity project's `Assets/Plugins/` directory
2. Ensure your iOS build target is set correctly
3. The plugin will automatically configure iOS permissions

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

## Requirements

- Unity 2019.4 or later
- iOS 10.0 or later
- Xcode 12 or later

## iOS Permissions

The plugin automatically adds the following permissions to your iOS app:
- NSBluetoothAlwaysUsageDescription
- NSBluetoothPeripheralUsageDescription
