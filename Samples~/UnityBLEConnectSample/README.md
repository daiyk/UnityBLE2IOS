# Unity BLE Connect Sample

This sample demonstrates a focused BLE connection workflow using the UnityBLE2IOS package. It includes scanning, device selection, connection management, GATT service and characteristic inspection, characteristic notification subscription, and runtime debug logging.

## Features Demonstrated

- Device scanning with start and stop controls
- Device list updates with name, RSSI, connectable state, advertised services, and optional local name or TX power details
- Device selection and connection state feedback
- Connect and disconnect workflow for a selected BLE peripheral
- Service discovery and service dropdown population after connection
- Characteristic dropdown population for the selected service
- Notification subscription for the selected characteristic
- Debug console output for permissions, Bluetooth state, scans, connections, GATT inspection, reads, and notifications

## Files Included

### Scenes
- `Scenes/UnityBLEConnectSample.unity` - Complete connect sample scene with pre-configured UI

### Scripts
- `Scripts/BLEDeviceManager.cs` - Main controller for scanning, connecting, service inspection, characteristic selection, notification subscription, and debug output
- `Scripts/BLEDeviceItem.cs` - UI component for discovered BLE device rows

### Prefabs
- `Prefabs/BLE_Device.prefab` - Prefab for discovered BLE device rows

### Fonts
- `Fonts/Apple Color Emoji.ttc` - Apple Color Emoji font file
- `Fonts/Apple Color Emoji Color.asset` - TextMesh Pro font asset for emoji fallback support
- `Fonts/BLE_Sample_LiberationSans SDF.asset` - TextMesh Pro font asset used by sample UI text

## How to Use

1. Open Unity Package Manager.
2. Select "Unity BLE iOS Plugin".
3. Expand "Samples" and import "Unity BLE Connect Sample".
4. Open the imported scene at `Assets/Samples/Unity BLE iOS Plugin/<version>/Unity BLE Connect Sample/Scenes/UnityBLEConnectSample.unity`.
5. Build for iOS and deploy to a physical iOS device.
6. Grant Bluetooth permission when prompted.

BLE is not available in the iOS simulator. Use a physical iOS device for scanning, connecting, and notification testing.

## UI Controls

- **Scan**: Start or stop scanning for nearby BLE peripherals.
- **Connect**: Connect to the selected device.
- **Disconnect**: Disconnect from the connected device.
- **Inspect**: Refresh discovered services and characteristics for the connected device.
- **Services**: Select a discovered GATT service.
- **Characteristics**: Select a characteristic from the selected service.
- **Subscribe**: Subscribe to notifications for the selected characteristic.
- **Device List**: Select a discovered device.
- **Debug Console**: Review runtime BLE status and operation logs.

## Optional TextMesh Pro Emoji Support

If you want emoji fallback support in the sample UI:

1. Open `Window > TextMeshPro > Settings`.
2. In "Fallback Font Assets", click the "+" button.
3. Add `Apple Color Emoji Color` from this sample's `Fonts` folder.

## Troubleshooting

- **No devices found**: Confirm Bluetooth is enabled, permission is granted, and the peripheral is advertising.
- **Connection fails**: Confirm the device is in range and connectable.
- **No services or characteristics**: Connect first, then use **Inspect** after the connection is complete.
- **Subscribe does not receive data**: Confirm the selected characteristic supports notifications and the peripheral is sending updates.
- **Missing UI references after import**: Reimport this sample from Package Manager and confirm the `.meta` files were preserved in the package.

For package API details, see the main UnityBLE2IOS README and package documentation.
