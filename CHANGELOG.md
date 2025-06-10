# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.3] - 2025-06-10

### Added
- **Complete Sample Scene**: Added comprehensive Unity BLE Sample scene (`Samples~/UnityBLESample/`) with full UI implementation
  - **Interactive Sample Scene**: Complete working scene with pre-configured UI for immediate testing
  - **Advanced Controllers**: Enhanced BLEStatusController with real-time device list management and debug console
  - **Device List UI**: Interactive device list with selection, connection status, and visual feedback
  - **Debug Console**: Real-time logging system for BLE operations and status updates
  - **Emoji Font Support**: Included Apple Color Emoji font assets for proper text rendering in device names
  - **Comprehensive Documentation**: Detailed README with setup instructions, troubleshooting, and customization guide

### Changed
- **Sample Structure**: Replaced basic example scripts with complete functional sample scene
- **Package Samples**: Streamlined to single high-quality sample instead of multiple incomplete examples
- **Documentation**: Updated main README to highlight the new comprehensive sample

### Improved
- **User Experience**: Users now get immediate working example instead of having to build UI from scratch
- **Learning Curve**: Complete sample provides better reference implementation for developers
- **Testing**: Ready-to-use scene for immediate BLE functionality testing on iOS devices

## [1.0.2] - 2025-06-05

### Added
- **Instance Check Method**: Added `HasInstance` static property to BluetoothManager for safe instance existence checking, to prevent null reference exceptions during GameObject destruction scenarios.

## [1.0.1] - 2025-06-04

### Fixed
- **Critical Bug Fix**: Resolved "Object BluetoothManager not found" error in UnitySendMessage calls by ensuring GameObject naming consistency
- **Crash Fix**: Fixed NSInvalidArgumentException when inserting nil values into NSDictionary during device info creation
- **Device Name Resolution**: Enhanced BLE device discovery to prioritize advertisement local name over peripheral name, significantly reducing "Unknown Device" entries
- **State Restoration**: Improved CoreBluetooth central manager state restoration with proper peripheral delegate assignment and scanning state recovery

### Changed
- **Auto-Initialization**: Implemented automatic BluetoothManager initialization using `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`
- **Smart GameObject Management**: Added intelligent GameObject discovery that finds existing "BluetoothManager" objects or creates new ones as needed
- **Removed Manual Initialization**: Eliminated the need for manual `Initialize()` calls - the plugin now initializes automatically when first accessed
- **Enhanced Device Name Extraction**: Added sophisticated name resolution from manufacturer data for better device identification
- **Updated Sample Scripts**: Modernized example scripts to use auto-initialization pattern and removed outdated manual setup code

### Improved
- **iOS Plugin Stability**: Enhanced nil value handling throughout the native iOS implementation
- **Device Discovery**: Improved advertisement data parsing with better fallback mechanisms
- **Documentation**: Completely updated README.md to reflect auto-initialization changes and new API patterns
- **Error Handling**: Added more robust error handling for edge cases in device discovery and connection
- **State Management**: Enhanced CoreBluetooth state restoration for better app lifecycle handling

### Technical Enhancements
- Added `CBCentralManagerOptionRestoreIdentifierKey` for proper iOS state restoration
- Implemented smart device filtering to skip devices with no useful identifying information
- Enhanced manufacturer data parsing with ASCII validation
- Improved RSSI-based device filtering for better discovery quality
- Added comprehensive logging for debugging and troubleshooting

### Breaking Changes
- **Manual Initialization Removed**: The `Initialize()` method is no longer required and should be removed from existing code
- **Unity Version Requirement**: Updated minimum Unity version requirement to 2021.3+ for better auto-initialization support

## [1.0.0] - 2025-06-01

### Added
- Initial release of Unity BLE iOS Plugin
- Bluetooth Low Energy device discovery functionality
- Connection management for BLE devices
- Comprehensive device information retrieval (RSSI, services, manufacturer data)
- Event-driven architecture with callbacks for device discovery and connection
- Connected device tracking and management
- Native iOS CoreBluetooth framework integration
- Editor simulation mode for testing without iOS device
- Assembly definition files for proper package structure
- Example scripts demonstrating basic usage
- Automatic iOS build configuration
- Comprehensive error handling and logging

### Features
- Device discovery with advertisement data caching
- Multi-device connection support
- Real-time RSSI monitoring
- Service UUID detection
- Manufacturer data parsing
- Local name and transmission power level support
- Permission handling for iOS Bluetooth access
- Robust error handling with detailed logging
- Unity Package Manager compatibility

### Technical Details
- Unity 2020.3+ compatibility
- iOS 10.0+ support
- CoreBluetooth framework integration
- Thread-safe operation
- Memory-efficient device caching
- Cross-language communication between C# and Objective-C

### Documentation
- Comprehensive README with API reference
- Quick start guide with code examples
- Installation instructions for Unity Package Manager
- Troubleshooting guide
- MIT License
