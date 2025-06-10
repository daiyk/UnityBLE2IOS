# Unity BLE Sample Scene

This sample demonstrates a complete BLE (Bluetooth Low Energy) implementation using the UnityBLE2IOS package. It provides a comprehensive UI for scanning, connecting, and managing BLE devices on iOS.

## Features Demonstrated

- **Device Scanning**: Discover nearby BLE devices with real-time updates
- **Device Information Display**: Shows device name, RSSI, and connection status
- **Connection Management**: Connect and disconnect from BLE devices
- **Real-time Status Updates**: Live status monitoring and debug console
- **Interactive Device List**: Clickable device items with visual feedback
- **Debug Console**: Real-time logging of BLE operations

## Files Included

### Scripts
- **BLEStatusController.cs** - Main controller managing the BLE UI and interactions
- **BLEDeviceItem.cs** - UI component for individual device list items

### Scenes
- **UnityBLESample.unity** - Complete sample scene with pre-configured UI

### Prefabs
- **BLE_Device.prefab** - Prefab for device list items

### Fonts
- **Apple Color Emoji.ttc** - Apple Color Emoji font file
- **Apple Color Emoji Color.asset** - TextMesh Pro font asset for emoji support

## How to Use

1. **Import the Sample**
   - Open Unity Package Manager
   - Find "Unity BLE iOS Plugin" in your packages
   - Expand "Samples" and click "Import" next to "Unity BLE Sample"

2. **Open the Scene**
   - Navigate to `Samples/Unity BLE iOS Plugin/UnityBLESample/Scenes/`
   - Open `UnityBLESample.unity`

3. **Build and Test**
   - Build for iOS (iOS 10.0+)
   - Deploy to an iOS device
   - The sample will automatically request Bluetooth permissions

4. **Configure TextMesh Pro Emoji Support (Optional)**
   - If you want emoji support in the UI text:
   - Go to Window → TextMeshPro → Settings
   - In the "Fallback Font Assets" section, click the "+" button
   - Drag the "Apple Color Emoji Color" asset from the sample's Fonts folder into the list
   - This enables emoji rendering in device names and debug console output

## UI Controls

- **Scan Button**: Start/stop scanning for BLE devices
- **Connect Button**: Connect to the selected device
- **Disconnect Button**: Disconnect from the currently connected device
- **Device List**: Tap any discovered device to select it
- **Debug Console**: View real-time BLE operation logs

## Code Structure

### BLEStatusController
The main controller that:
- Manages UI state and button interactions
- Handles BLE device discovery and connection events
- Updates the device list dynamically
- Provides debug logging functionality

Key methods:
- `OnScanButtonPressed()` - Toggle scanning state
- `OnConnectButtonPressed()` - Connect to selected device
- `OnDisconnectButtonPressed()` - Disconnect from current device
- `OnDeviceDiscovered()` - Handle new device discovery
- `UpdateDeviceList()` - Refresh the UI device list

### BLEDeviceItem
UI component for each device in the list:
- Displays device information (name, RSSI)
- Handles selection state and visual feedback
- Manages click interactions for device selection

## Customization

You can customize the sample by:

1. **Modifying UI Colors**: Adjust button colors in the inspector
2. **Adding Device Filtering**: Filter devices by name, RSSI, or services
3. **Extending Device Info**: Display additional device properties
4. **Custom Interactions**: Add service discovery or characteristic reading

## Requirements

- Unity 2021.3 or later
- iOS 10.0 or later
- Physical iOS device (BLE not available in simulator)
- UnityBLE2IOS package installed

## Troubleshooting

- **No devices found**: Ensure Bluetooth is enabled and permissions are granted
- **Connection fails**: Check device is in range and connectable
- **UI not updating**: Verify the scene has the proper UI references set

For more information, see the main package documentation.
