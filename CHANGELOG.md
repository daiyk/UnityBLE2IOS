# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
