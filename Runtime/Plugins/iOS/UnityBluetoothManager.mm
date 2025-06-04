#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

// Unity messaging function declaration
extern "C" void UnitySendMessage(const char* obj, const char* method, const char* msg);

@interface UnityBluetoothManager : NSObject <CBCentralManagerDelegate, CBPeripheralDelegate>

@property (nonatomic, strong) CBCentralManager *centralManager;
@property (nonatomic, strong) NSMutableDictionary *discoveredPeripherals;
@property (nonatomic, strong) NSMutableDictionary *connectedPeripherals;
@property (nonatomic, strong) NSMutableDictionary *cachedAdvertisementData;
@property (nonatomic, assign) BOOL isScanning;

+ (instancetype)sharedInstance;
- (void)initializeBluetooth;
- (void)requestPermissions;
- (void)startScanning;
- (void)stopScanning;
- (void)connectToDevice:(NSString *)deviceId;
- (void)disconnectDevice:(NSString *)deviceId;
- (BOOL)isBluetoothEnabled;
- (BOOL)isDeviceConnected:(NSString *)deviceId;

@end

@implementation UnityBluetoothManager

+ (instancetype)sharedInstance {
    static UnityBluetoothManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[UnityBluetoothManager alloc] init];
    });
    return sharedInstance;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        self.discoveredPeripherals = [[NSMutableDictionary alloc] init];
        self.connectedPeripherals = [[NSMutableDictionary alloc] init];
        self.cachedAdvertisementData = [[NSMutableDictionary alloc] init];
        self.isScanning = NO;
    }
    return self;
}

- (void)initializeBluetooth {
    NSLog(@"Initializing Bluetooth...");
    if (self.centralManager == nil) {
        NSDictionary *options = @{
            CBCentralManagerOptionShowPowerAlertKey: @YES,
            CBCentralManagerOptionRestoreIdentifierKey: @"UnityBLE2IOSRestoreIdentifier"
        };
        self.centralManager = [[CBCentralManager alloc] initWithDelegate:self queue:nil options:options];
    }
}

- (void)requestPermissions {
    NSLog(@"Requesting Bluetooth permissions...");
    // iOS handles permissions automatically when we try to use Bluetooth
    // Just initialize the central manager which will trigger permission request
    [self initializeBluetooth];
    
    // Simulate permission granted for now - in real app, this would be handled by the delegate
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(1.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
        UnitySendMessage("BluetoothManager", "OnPermissionResultNative", "1");
    });
}

- (void)startScanning {
    NSLog(@"Starting BLE scan...");
    if (self.centralManager.state != CBManagerStatePoweredOn) {
        NSLog(@"Bluetooth is not powered on. Current state: %ld", (long)self.centralManager.state);
        return;
    }
    
    if (self.isScanning) {
        NSLog(@"Already scanning, stopping current scan first");
        [self stopScanning];
    }
    
    [self.discoveredPeripherals removeAllObjects];
    [self.cachedAdvertisementData removeAllObjects];
    
    // Scan for all peripherals
    NSDictionary *scanOptions = @{
        CBCentralManagerScanOptionAllowDuplicatesKey: @NO
    };
    
    [self.centralManager scanForPeripheralsWithServices:nil options:scanOptions];
    self.isScanning = YES;
    NSLog(@"BLE scan started successfully");
}

- (void)stopScanning {
    NSLog(@"Stopping BLE scan...");
    if (self.centralManager && self.isScanning) {
        [self.centralManager stopScan];
        self.isScanning = NO;
        NSLog(@"BLE scan stopped");
    }
}

- (void)connectToDevice:(NSString *)deviceId {
    NSLog(@"Connecting to device: %@", deviceId);
    CBPeripheral *peripheral = self.discoveredPeripherals[deviceId];
    if (peripheral) {
        peripheral.delegate = self;
        [self.centralManager connectPeripheral:peripheral options:nil];
    } else {
        NSLog(@"Device not found: %@", deviceId);
        NSString *errorInfo = [NSString stringWithFormat:@"%@|Device not found", deviceId];
        UnitySendMessage("BluetoothManager", "OnConnectionFailedNative", [errorInfo UTF8String]);
    }
}

- (void)disconnectDevice:(NSString *)deviceId {
    NSLog(@"Disconnecting device: %@", deviceId);
    CBPeripheral *peripheral = self.connectedPeripherals[deviceId];
    if (peripheral) {
        [self.centralManager cancelPeripheralConnection:peripheral];
    }
}

- (BOOL)isBluetoothEnabled {
    return self.centralManager.state == CBManagerStatePoweredOn;
}

- (BOOL)isDeviceConnected:(NSString *)deviceId {
    CBPeripheral *peripheral = self.connectedPeripherals[deviceId];
    return peripheral && peripheral.state == CBPeripheralStateConnected;
}

#pragma mark - CBCentralManagerDelegate

- (void)centralManagerDidUpdateState:(CBCentralManager *)central {
    NSLog(@"Central Manager state updated: %ld", (long)central.state);
    
    BOOL isEnabled = (central.state == CBManagerStatePoweredOn);
    NSString *stateString = isEnabled ? @"1" : @"0";
    UnitySendMessage("BluetoothManager", "OnBluetoothStateChangedNative", [stateString UTF8String]);
    
    if (!isEnabled && self.isScanning) {
        self.isScanning = NO;
    }
}

- (void)centralManager:(CBCentralManager *)central didDiscoverPeripheral:(CBPeripheral *)peripheral advertisementData:(NSDictionary<NSString *,id> *)advertisementData RSSI:(NSNumber *)RSSI {
    
    NSString *deviceId = peripheral.identifier.UUIDString;
        // Try to get the best available name in order of preference:
    // 1. Local name from advertisement data (most common for BLE devices)
    // 2. Peripheral name (if set by device)
    // 3. Fallback to "Unknown Device"
    NSString *deviceName = @"Unknown Device";
    
    // First priority: Local name from advertisement data
    NSString *localName = advertisementData[CBAdvertisementDataLocalNameKey];
    if (localName && localName.length > 0) {
        deviceName = localName;
    }
    // Second priority: Peripheral name
    else if (peripheral.name && peripheral.name.length > 0) {
        deviceName = peripheral.name;
    }
    // Third priority: Try to extract name from manufacturer data (device-specific)
    else {
        NSData *manufacturerData = advertisementData[CBAdvertisementDataManufacturerDataKey];
        if (manufacturerData && manufacturerData.length > 2) {
            // Some devices encode name in manufacturer data - this is device-specific
            // For example, some devices put readable text after the first 2 bytes
            NSString *possibleName = [self extractNameFromManufacturerData:manufacturerData];
            if (possibleName && possibleName.length > 0) {
                deviceName = possibleName;
            }
        }
    }
    
    // Skip devices with very weak signal (optional filtering)
    if ([RSSI intValue] < -90) {
        NSLog(@"Skipping device with weak signal: %@ (RSSI: %@)", deviceName, RSSI);
        return;
    }
    
    // Store the discovered peripheral
    self.discoveredPeripherals[deviceId] = peripheral;
    
    // Extract additional identifying information from advertisement
    NSArray *serviceUUIDs = advertisementData[CBAdvertisementDataServiceUUIDsKey] ?: @[];
    NSData *manufacturerData = advertisementData[CBAdvertisementDataManufacturerDataKey];
    NSNumber *txPowerLevel = advertisementData[CBAdvertisementDataTxPowerLevelKey];

    // convert service UUIDs to string array safely
    NSMutableArray *serviceUUIDStrings = [NSMutableArray array];
    if (serviceUUIDs.count == 0 && [deviceName isEqualToString:@"Unknown Device"]) {
        // Skip devices with no name and no services (likely not useful)
        return;
    }
    else
    {
        for (CBUUID *uuid in serviceUUIDs) {
            [serviceUUIDStrings addObject:uuid.UUIDString];
        }
    }

    // convert manufacturer data to Hex string if available
    NSString *manufacturerDataString = @"";
    if(manufacturerData && manufacturerData.length > 0) {
        const unsigned char *dataBytes = (const unsigned char *)[manufacturerData bytes];
        NSMutableString *hexString = [NSMutableString stringWithCapacity:manufacturerData.length * 2];
        for (NSInteger i = 0; i < manufacturerData.length; i++) {
            [hexString appendFormat:@"%02x", dataBytes[i]];
        }
        manufacturerDataString = [hexString copy];
    }

    // Create device info JSON
    NSDictionary *deviceInfo = @{
        @"deviceId": deviceId ?: @"Unknown Device ID",
        @"name": deviceName ?: @"Unknown Device",
        @"rssi": RSSI,
        @"isConnectable": @YES,
        @"serviceUUIDs": serviceUUIDStrings,
        @"manufacturerData": manufacturerDataString,
        @"localName": localName ?: @"Unknown",
        @"txPowerLevel": txPowerLevel ?: @0
    };
    
    // Cache the complete device info for later retrieval
    self.cachedAdvertisementData[deviceId] = deviceInfo;
    
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:deviceInfo options:0 error:&error];
    if (jsonData && !error) {
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        UnitySendMessage("BluetoothManager", "OnDeviceDiscoveredNative", [jsonString UTF8String]);
        NSLog(@"Discovered device: %@ (%@) RSSI: %@", deviceName, deviceId, RSSI);
    } else {
        NSLog(@"Error creating JSON for device: %@", error.localizedDescription);
    }
}

- (void)centralManager:(CBCentralManager *)central didConnectPeripheral:(CBPeripheral *)peripheral {
    NSLog(@"Connected to peripheral: %@", peripheral.name);
    
    NSString *deviceId = peripheral.identifier.UUIDString;
    self.connectedPeripherals[deviceId] = peripheral;
    
    // Discover services
    [peripheral discoverServices:nil];
    
    UnitySendMessage("BluetoothManager", "OnDeviceConnectedNative", [deviceId UTF8String]);
}

- (void)centralManager:(CBCentralManager *)central didFailToConnectPeripheral:(CBPeripheral *)peripheral error:(NSError *)error {
    NSLog(@"Failed to connect to peripheral: %@, error: %@", peripheral.name, error.localizedDescription);
    
    NSString *deviceId = peripheral.identifier.UUIDString;
    NSString *errorInfo = [NSString stringWithFormat:@"%@|%@", deviceId, error.localizedDescription];
    UnitySendMessage("BluetoothManager", "OnConnectionFailedNative", [errorInfo UTF8String]);
}

- (void)centralManager:(CBCentralManager *)central didDisconnectPeripheral:(CBPeripheral *)peripheral error:(NSError *)error {
    NSLog(@"Disconnected from peripheral: %@", peripheral.name);
    
    NSString *deviceId = peripheral.identifier.UUIDString;
    [self.connectedPeripherals removeObjectForKey:deviceId];
    
    UnitySendMessage("BluetoothManager", "OnDeviceDisconnectedNative", [deviceId UTF8String]);
}

- (void)centralManager:(CBCentralManager *)central willRestoreState:(NSDictionary<NSString *, id> *)dict {
    NSLog(@"Central Manager will restore state: %@", dict);
    
    // Restore discovered peripherals if any
    NSArray *peripherals = dict[CBCentralManagerRestoredStatePeripheralsKey];
    if (peripherals) {
        for (CBPeripheral *peripheral in peripherals) {
            NSString *deviceId = peripheral.identifier.UUIDString;
            self.discoveredPeripherals[deviceId] = peripheral;
            peripheral.delegate = self;
            NSLog(@"Restored peripheral: %@ (%@)", peripheral.name ?: @"Unknown", deviceId);
        }
    }
    
    // Check if scanning was active
    NSArray *scanServices = dict[CBCentralManagerRestoredStateScanServicesKey];
    NSDictionary *scanOptions = dict[CBCentralManagerRestoredStateScanOptionsKey];
    if (scanServices || scanOptions) {
        self.isScanning = YES;
        NSLog(@"Restored scanning state");
    }
}

#pragma mark - CBPeripheralDelegate

- (void)peripheral:(CBPeripheral *)peripheral didDiscoverServices:(NSError *)error {
    if (error) {
        NSLog(@"Error discovering services: %@", error.localizedDescription);
        return;
    }
    
    NSLog(@"Discovered %lu services for peripheral: %@", (unsigned long)peripheral.services.count, peripheral.name);
    
    for (CBService *service in peripheral.services) {
        NSLog(@"Service UUID: %@", service.UUID.UUIDString);
        // Discover characteristics for each service
        [peripheral discoverCharacteristics:nil forService:service];
    }
}

- (void)peripheral:(CBPeripheral *)peripheral didDiscoverCharacteristicsForService:(CBService *)service error:(NSError *)error {
    if (error) {
        NSLog(@"Error discovering characteristics: %@", error.localizedDescription);
        return;
    }
    
    NSLog(@"Discovered %lu characteristics for service: %@", (unsigned long)service.characteristics.count, service.UUID.UUIDString);
    
    for (CBCharacteristic *characteristic in service.characteristics) {
        NSLog(@"Characteristic UUID: %@, Properties: %lu", characteristic.UUID.UUIDString, (unsigned long)characteristic.properties);
    }
}

#pragma mark - Helper Methods
// Helper method to try extracting readable name from manufacturer data
- (NSString *)extractNameFromManufacturerData:(NSData *)manufacturerData {
    if (manufacturerData.length <= 2) return nil;
    
    // Skip first 2 bytes (usually company identifier) and try to read as string
    NSData *nameData = [manufacturerData subdataWithRange:NSMakeRange(2, manufacturerData.length - 2)];
    NSString *possibleName = [[NSString alloc] initWithData:nameData encoding:NSUTF8StringEncoding];
    
    // Validate that it's a reasonable name (printable ASCII characters)
    if (possibleName && possibleName.length > 0 && possibleName.length < 50) {
        NSCharacterSet *printableSet = [NSCharacterSet characterSetWithCharactersInString:@"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -_()[]"];
        NSString *filtered = [[possibleName componentsSeparatedByCharactersInSet:[printableSet invertedSet]] componentsJoinedByString:@""];
        if (filtered.length > 2) {  // At least 3 valid characters
            return filtered;
        }
    }
    
    return nil;
}

@end

#pragma mark - Unity Interface Functions

extern "C" {
    void _initializeBluetooth() {
        [[UnityBluetoothManager sharedInstance] initializeBluetooth];
    }
    
    void _requestPermissions() {
        [[UnityBluetoothManager sharedInstance] requestPermissions];
    }
    
    void _startScanning() {
        [[UnityBluetoothManager sharedInstance] startScanning];
    }
    
    void _stopScanning() {
        [[UnityBluetoothManager sharedInstance] stopScanning];
    }
    
    void _connectToDevice(const char* deviceId) {
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        [[UnityBluetoothManager sharedInstance] connectToDevice:deviceIdString];
    }
    
    void _disconnectDevice(const char* deviceId) {
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        [[UnityBluetoothManager sharedInstance] disconnectDevice:deviceIdString];
    }
    
    bool _isBluetoothEnabled() {
        return [[UnityBluetoothManager sharedInstance] isBluetoothEnabled];
    }
    
    bool _isDeviceConnected(const char* deviceId) {
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        return [[UnityBluetoothManager sharedInstance] isDeviceConnected:deviceIdString];
    }
    
    // Get number of discovered devices
    int _getDiscoveredDeviceCount() {
        return (int)[[UnityBluetoothManager sharedInstance] discoveredPeripherals].count;
    }
    
    // Get discovered device info by index as JSON string
    const char* _getDiscoveredDeviceInfo(int index) {
        static char buffer[2048]; // Static buffer for safe string return
        buffer[0] = '\0'; // Initialize as empty string
        
        UnityBluetoothManager *manager = [UnityBluetoothManager sharedInstance];
        NSArray *deviceIds = [manager.discoveredPeripherals allKeys];
        
        if (index < 0 || index >= deviceIds.count) {
            return buffer; // Return empty string for invalid index
        }
        
        NSString *deviceId = deviceIds[index];
        
        // Try to get cached advertisement data first
        NSDictionary *cachedDeviceInfo = manager.cachedAdvertisementData[deviceId];
        if (cachedDeviceInfo) {
            NSError *error;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:cachedDeviceInfo options:0 error:&error];
            if (jsonData && !error) {
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                if (jsonString.length < sizeof(buffer)) {
                    strcpy(buffer, [jsonString UTF8String]);
                }
                return buffer;
            }
        }
        
        // Fallback to basic peripheral info if cached data not available
        CBPeripheral *peripheral = manager.discoveredPeripherals[deviceId];
        if (peripheral) {
            NSDictionary *deviceInfo = @{
                @"deviceId": peripheral.identifier.UUIDString,
                @"name": peripheral.name ?: @"Unknown Device",
                @"rssi": @0,
                @"isConnectable": @YES,
                @"serviceUUIDs": @[],
                @"manufacturerData": @"",
                @"localName": @"",
                @"txPowerLevel": @0
            };
            
            NSError *error;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:deviceInfo options:0 error:&error];
            if (jsonData && !error) {
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                if (jsonString.length < sizeof(buffer)) {
                    strcpy(buffer, [jsonString UTF8String]);
                }
            }
        }
        
        return buffer;
    }
}
