#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

// Unity messaging function declaration
extern "C" void UnitySendMessage(const char* obj, const char* method, const char* msg);

@interface UnityBluetoothManager : NSObject <CBCentralManagerDelegate, CBPeripheralDelegate>

@property (nonatomic, strong) CBCentralManager *centralManager;
@property (nonatomic, strong) NSMutableDictionary *discoveredPeripherals;
@property (nonatomic, strong) NSMutableDictionary *connectedPeripherals;
@property (nonatomic, strong) NSMutableDictionary *cachedAdvertisementData;
@property (nonatomic, strong) NSMutableDictionary *peripheralCharacteristics; // deviceId -> { serviceUUID -> { characteristicUUID -> CBCharacteristic } }
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
// GATT characteristic operations
- (void)writeValue:(NSData *)data toCharacteristic:(NSString *)characteristicUUID forDevice:(NSString *)deviceId;
- (void)subscribeToCharacteristic:(NSString *)characteristicUUID forDevice:(NSString *)deviceId;
- (void)unsubscribeFromCharacteristic:(NSString *)characteristicUUID forDevice:(NSString *)deviceId;

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
        self.peripheralCharacteristics = [[NSMutableDictionary alloc] init];
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
    
    // Clean up stored characteristics for this device
    [self.peripheralCharacteristics removeObjectForKey:deviceId];
    
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
    
    NSString *deviceId = peripheral.identifier.UUIDString;
    NSString *serviceUUID = service.UUID.UUIDString;
    
    // Initialize nested dictionaries if they don't exist
    if (!self.peripheralCharacteristics[deviceId]) {
        self.peripheralCharacteristics[deviceId] = [[NSMutableDictionary alloc] init];
    }
    if (!self.peripheralCharacteristics[deviceId][serviceUUID]) {
        self.peripheralCharacteristics[deviceId][serviceUUID] = [[NSMutableDictionary alloc] init];
    }
    
    for (CBCharacteristic *characteristic in service.characteristics) {
        NSLog(@"Characteristic UUID: %@, Properties: %lu", characteristic.UUID.UUIDString, (unsigned long)characteristic.properties);
        
        // Store the characteristic
        NSString *characteristicUUID = characteristic.UUID.UUIDString;
        self.peripheralCharacteristics[deviceId][serviceUUID][characteristicUUID] = characteristic;
    }
}

#pragma mark - GATT Characteristic Operations

- (void)writeValue:(NSData *)data toCharacteristic:(NSString *)characteristicUUID forDevice:(NSString *)deviceId {
    NSLog(@"Writing data to characteristic %@ for device %@", characteristicUUID, deviceId);
    
    CBPeripheral *peripheral = self.connectedPeripherals[deviceId];
    if (!peripheral) {
        NSLog(@"Device %@ not connected", deviceId);
        return;
    }
    
    if (peripheral.state != CBPeripheralStateConnected) {
        NSLog(@"Device %@ not in connected state", deviceId);
        return;
    }
    
    // Find the characteristic across all services
    CBCharacteristic *targetCharacteristic = nil;
    NSDictionary *deviceCharacteristics = self.peripheralCharacteristics[deviceId];
    
    for (NSString *serviceUUID in deviceCharacteristics) {
        NSDictionary *serviceCharacteristics = deviceCharacteristics[serviceUUID];
        CBCharacteristic *characteristic = serviceCharacteristics[characteristicUUID];
        if (characteristic) {
            targetCharacteristic = characteristic;
            break;
        }
    }
    
    if (!targetCharacteristic) {
        NSLog(@"Characteristic %@ not found for device %@", characteristicUUID, deviceId);
        return;
    }
    
    // Check if characteristic supports writing
    if (!(targetCharacteristic.properties & CBCharacteristicPropertyWrite) && 
        !(targetCharacteristic.properties & CBCharacteristicPropertyWriteWithoutResponse)) {
        NSLog(@"Characteristic %@ does not support writing", characteristicUUID);
        return;
    }
    
    // Determine write type based on characteristic properties
    CBCharacteristicWriteType writeType = CBCharacteristicWriteWithResponse;
    if (targetCharacteristic.properties & CBCharacteristicPropertyWriteWithoutResponse) {
        writeType = CBCharacteristicWriteWithoutResponse;
    }
    
    // Log the data being written
    NSMutableString *hexString = [NSMutableString string];
    const unsigned char *bytes = (const unsigned char *)[data bytes];
    for (NSUInteger i = 0; i < data.length; i++) {
        [hexString appendFormat:@"%02X ", bytes[i]];
    }
    NSLog(@"Writing data: %@", hexString);
    
    // Write the data
    [peripheral writeValue:data forCharacteristic:targetCharacteristic type:writeType];
}

- (void)subscribeToCharacteristic:(NSString *)characteristicUUID forDevice:(NSString *)deviceId {
    NSLog(@"Subscribing to characteristic %@ for device %@", characteristicUUID, deviceId);
    
    CBPeripheral *peripheral = self.connectedPeripherals[deviceId];
    if (!peripheral) {
        NSLog(@"Device %@ not connected", deviceId);
        return;
    }
    
    if (peripheral.state != CBPeripheralStateConnected) {
        NSLog(@"Device %@ not in connected state", deviceId);
        return;
    }
    
    // Find the characteristic across all services
    CBCharacteristic *targetCharacteristic = nil;
    NSDictionary *deviceCharacteristics = self.peripheralCharacteristics[deviceId];
    
    for (NSString *serviceUUID in deviceCharacteristics) {
        NSDictionary *serviceCharacteristics = deviceCharacteristics[serviceUUID];
        CBCharacteristic *characteristic = serviceCharacteristics[characteristicUUID];
        if (characteristic) {
            targetCharacteristic = characteristic;
            break;
        }
    }
    
    if (!targetCharacteristic) {
        NSLog(@"Characteristic %@ not found for device %@", characteristicUUID, deviceId);
        return;
    }
    
    // Check if characteristic supports notifications or indications
    if (!(targetCharacteristic.properties & CBCharacteristicPropertyNotify) && 
        !(targetCharacteristic.properties & CBCharacteristicPropertyIndicate)) {
        NSLog(@"Characteristic %@ does not support notifications or indications", characteristicUUID);
        return;
    }
    
    // Subscribe to notifications
    [peripheral setNotifyValue:YES forCharacteristic:targetCharacteristic];
    NSLog(@"Subscribed to notifications for characteristic %@", characteristicUUID);
}

- (void)unsubscribeFromCharacteristic:(NSString *)characteristicUUID forDevice:(NSString *)deviceId {
    NSLog(@"Unsubscribing from characteristic %@ for device %@", characteristicUUID, deviceId);
    
    CBPeripheral *peripheral = self.connectedPeripherals[deviceId];
    if (!peripheral) {
        NSLog(@"Device %@ not connected", deviceId);
        return;
    }
    
    if (peripheral.state != CBPeripheralStateConnected) {
        NSLog(@"Device %@ not in connected state", deviceId);
        return;
    }
    
    // Find the characteristic across all services
    CBCharacteristic *targetCharacteristic = nil;
    NSDictionary *deviceCharacteristics = self.peripheralCharacteristics[deviceId];
    
    for (NSString *serviceUUID in deviceCharacteristics) {
        NSDictionary *serviceCharacteristics = deviceCharacteristics[serviceUUID];
        CBCharacteristic *characteristic = serviceCharacteristics[characteristicUUID];
        if (characteristic) {
            targetCharacteristic = characteristic;
            break;
        }
    }
    
    if (!targetCharacteristic) {
        NSLog(@"Characteristic %@ not found for device %@", characteristicUUID, deviceId);
        return;
    }
    
    // Check if characteristic supports notifications or indications
    if (!(targetCharacteristic.properties & CBCharacteristicPropertyNotify) && 
        !(targetCharacteristic.properties & CBCharacteristicPropertyIndicate)) {
        NSLog(@"Characteristic %@ does not support notifications or indications", characteristicUUID);
        return;
    }
    
    // Check if currently subscribed
    if (!targetCharacteristic.isNotifying) {
        NSLog(@"Characteristic %@ is not currently subscribed", characteristicUUID);
        return;
    }
    
    // Unsubscribe from notifications
    [peripheral setNotifyValue:NO forCharacteristic:targetCharacteristic];
    NSLog(@"Unsubscribed from notifications for characteristic %@", characteristicUUID);
}

- (void)peripheral:(CBPeripheral *)peripheral didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error {
    if (error) {
        NSLog(@"Error updating value for characteristic %@: %@", characteristic.UUID.UUIDString, error.localizedDescription);
        return;
    }
    
    NSString *deviceId = peripheral.identifier.UUIDString;
    NSString *characteristicUUID = characteristic.UUID.UUIDString;
    NSData *value = characteristic.value;
    
    if (!value) {
        NSLog(@"Received empty value for characteristic %@", characteristicUUID);
        return;
    }
    
    // Log the received data
    NSMutableString *hexString = [NSMutableString string];
    const unsigned char *bytes = (const unsigned char *)[value bytes];
    for (NSUInteger i = 0; i < value.length; i++) {
        [hexString appendFormat:@"%02X ", bytes[i]];
    }
    NSLog(@"Received data from characteristic %@: %@", characteristicUUID, hexString);
    
    // Convert data to hex string for Unity
    NSMutableString *dataHexString = [NSMutableString stringWithCapacity:value.length * 2];
    for (NSUInteger i = 0; i < value.length; i++) {
        [dataHexString appendFormat:@"%02x", bytes[i]];
    }
    
    // Create JSON message for Unity
    NSDictionary *messageData = @{
        @"deviceId": deviceId,
        @"characteristicUUID": characteristicUUID,
        @"data": dataHexString
    };
    
    NSError *jsonError;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:messageData options:0 error:&jsonError];
    if (jsonData && !jsonError) {
        NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
        UnitySendMessage("BluetoothManager", "OnCharacteristicValueReceivedNative", [jsonString UTF8String]);
    } else {
        NSLog(@"Error creating JSON for characteristic value: %@", jsonError.localizedDescription);
    }
}

- (void)peripheral:(CBPeripheral *)peripheral didWriteValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error {
    if (error) {
        NSLog(@"Error writing value for characteristic %@: %@", characteristic.UUID.UUIDString, error.localizedDescription);
        
        // Notify Unity about the write error
        NSString *deviceId = peripheral.identifier.UUIDString;
        NSDictionary *errorData = @{
            @"deviceId": deviceId,
            @"characteristicUUID": characteristic.UUID.UUIDString,
            @"error": error.localizedDescription
        };
        
        NSError *jsonError;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:errorData options:0 error:&jsonError];
        if (jsonData && !jsonError) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            UnitySendMessage("BluetoothManager", "OnCharacteristicWriteErrorNative", [jsonString UTF8String]);
        }
    } else {
        NSLog(@"Successfully wrote value for characteristic %@", characteristic.UUID.UUIDString);
        
        // Notify Unity about successful write
        NSString *deviceId = peripheral.identifier.UUIDString;
        NSDictionary *successData = @{
            @"deviceId": deviceId,
            @"characteristicUUID": characteristic.UUID.UUIDString
        };
        
        NSError *jsonError;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:successData options:0 error:&jsonError];
        if (jsonData && !jsonError) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            UnitySendMessage("BluetoothManager", "OnCharacteristicWriteSuccessNative", [jsonString UTF8String]);
        }
    }
}

- (void)peripheral:(CBPeripheral *)peripheral didUpdateNotificationStateForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error {
    if (error) {
        NSLog(@"Error updating notification state for characteristic %@: %@", characteristic.UUID.UUIDString, error.localizedDescription);
    } else {
        NSLog(@"Notification state updated for characteristic %@. Notifications enabled: %@", 
              characteristic.UUID.UUIDString, characteristic.isNotifying ? @"YES" : @"NO");
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
        if (!deviceId) {
            NSLog(@"Error: Null deviceId passed to _connectToDevice");
            return;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        [[UnityBluetoothManager sharedInstance] connectToDevice:deviceIdString];
    }
    
    void _disconnectDevice(const char* deviceId) {
        if (!deviceId) {
            NSLog(@"Error: Null deviceId passed to _disconnectDevice");
            return;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        [[UnityBluetoothManager sharedInstance] disconnectDevice:deviceIdString];
    }
    
    bool _isBluetoothEnabled() {
        return [[UnityBluetoothManager sharedInstance] isBluetoothEnabled];
    }
    
    bool _isDeviceConnected(const char* deviceId) {
        if (!deviceId) {
            NSLog(@"Error: Null deviceId passed to _isDeviceConnected");
            return false;
        }
        
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
    
    // Write data to a characteristic
    void _writeCharacteristic(const char* deviceId, const char* characteristicUUID, const char* hexData) {
        if (!deviceId || !characteristicUUID || !hexData) {
            NSLog(@"Error: Null parameters passed to _writeCharacteristic");
            return;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        NSString *characteristicUUIDString = [NSString stringWithUTF8String:characteristicUUID];
        NSString *hexDataString = [NSString stringWithUTF8String:hexData];
        
        if (hexDataString.length == 0 || hexDataString.length % 2 != 0) {
            NSLog(@"Error: Invalid hex data string: %@", hexDataString);
            return;
        }
        
        // Convert hex string to NSData
        NSMutableData *data = [[NSMutableData alloc] init];
        unsigned char byte;
        for (int i = 0; i < hexDataString.length; i += 2) {
            NSString *byteString = [hexDataString substringWithRange:NSMakeRange(i, 2)];
            unsigned int hexValue;
            NSScanner *scanner = [NSScanner scannerWithString:byteString];
            if (![scanner scanHexInt:&hexValue]) {
                NSLog(@"Error: Invalid hex byte in data: %@", byteString);
                return;
            }
            byte = (unsigned char)hexValue;
            [data appendBytes:&byte length:1];
        }
        
        [[UnityBluetoothManager sharedInstance] writeValue:data 
                                        toCharacteristic:characteristicUUIDString 
                                               forDevice:deviceIdString];
    }
    
    // Subscribe to characteristic notifications
    void _subscribeToCharacteristic(const char* deviceId, const char* characteristicUUID) {
        if (!deviceId || !characteristicUUID) {
            NSLog(@"Error: Null parameters passed to _subscribeToCharacteristic");
            return;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        NSString *characteristicUUIDString = [NSString stringWithUTF8String:characteristicUUID];
        
        [[UnityBluetoothManager sharedInstance] subscribeToCharacteristic:characteristicUUIDString 
                                                                forDevice:deviceIdString];
    }
    
    // Unsubscribe from characteristic notifications
    void _unsubscribeFromCharacteristic(const char* deviceId, const char* characteristicUUID) {
        if (!deviceId || !characteristicUUID) {
            NSLog(@"Error: Null parameters passed to _unsubscribeFromCharacteristic");
            return;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        NSString *characteristicUUIDString = [NSString stringWithUTF8String:characteristicUUID];
        
        [[UnityBluetoothManager sharedInstance] unsubscribeFromCharacteristic:characteristicUUIDString 
                                                                    forDevice:deviceIdString];
    }
    
    // Get discovered characteristics for a device as JSON string
    const char* _getDeviceCharacteristics(const char* deviceId) {
        static char buffer[4096]; // Static buffer for safe string return
        buffer[0] = '\0'; // Initialize as empty string
        
        if (!deviceId) {
            NSLog(@"Error: Null deviceId passed to _getDeviceCharacteristics");
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        UnityBluetoothManager *manager = [UnityBluetoothManager sharedInstance];
        
        NSDictionary *deviceCharacteristics = manager.peripheralCharacteristics[deviceIdString];
        if (!deviceCharacteristics) {
            // Return empty JSON array if no characteristics found
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSMutableArray *characteristicsArray = [[NSMutableArray alloc] init];
        
        // Iterate through services and their characteristics
        for (NSString *serviceUUID in deviceCharacteristics) {
            NSDictionary *serviceCharacteristics = deviceCharacteristics[serviceUUID];
            
            for (NSString *characteristicUUID in serviceCharacteristics) {
                CBCharacteristic *characteristic = serviceCharacteristics[characteristicUUID];
                
                // Build properties array
                NSMutableArray *properties = [[NSMutableArray alloc] init];
                if (characteristic.properties & CBCharacteristicPropertyRead) {
                    [properties addObject:@"read"];
                }
                if (characteristic.properties & CBCharacteristicPropertyWrite) {
                    [properties addObject:@"write"];
                }
                if (characteristic.properties & CBCharacteristicPropertyWriteWithoutResponse) {
                    [properties addObject:@"writeWithoutResponse"];
                }
                if (characteristic.properties & CBCharacteristicPropertyNotify) {
                    [properties addObject:@"notify"];
                }
                if (characteristic.properties & CBCharacteristicPropertyIndicate) {
                    [properties addObject:@"indicate"];
                }
                
                NSDictionary *characteristicInfo = @{
                    @"serviceUUID": serviceUUID,
                    @"characteristicUUID": characteristicUUID,
                    @"properties": properties,
                    @"isNotifying": @(characteristic.isNotifying)
                };
                
                [characteristicsArray addObject:characteristicInfo];
            }
        }
        
        NSError *error;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:characteristicsArray options:NSJSONWritingPrettyPrinted error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            if (jsonString.length < sizeof(buffer)) {
                strcpy(buffer, [jsonString UTF8String]);
            } else {
                NSLog(@"Characteristics JSON too large for buffer");
                strcpy(buffer, "[]");
            }
        } else {
            NSLog(@"Error creating characteristics JSON: %@", error.localizedDescription);
            strcpy(buffer, "[]");
        }
        
        return buffer;
    }
    
    // Get all services for a device as JSON string
    const char* _getDeviceServices(const char* deviceId) {
        static char buffer[2048]; // Static buffer for safe string return
        buffer[0] = '\0'; // Initialize as empty string
        
        if (!deviceId) {
            NSLog(@"Error: Null deviceId passed to _getDeviceServices");
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        UnityBluetoothManager *manager = [UnityBluetoothManager sharedInstance];
        
        NSDictionary *deviceCharacteristics = manager.peripheralCharacteristics[deviceIdString];
        if (!deviceCharacteristics) {
            // Return empty JSON array if no services found
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSMutableArray *servicesArray = [[NSMutableArray alloc] init];
        
        // Iterate through services
        for (NSString *serviceUUID in deviceCharacteristics) {
            NSDictionary *serviceCharacteristics = deviceCharacteristics[serviceUUID];
            
            NSDictionary *serviceInfo = @{
                @"serviceUUID": serviceUUID,
                @"characteristicCount": @(serviceCharacteristics.count)
            };
            
            [servicesArray addObject:serviceInfo];
        }
        
        NSError *error;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:servicesArray options:NSJSONWritingPrettyPrinted error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            if (jsonString.length < sizeof(buffer)) {
                strcpy(buffer, [jsonString UTF8String]);
            } else {
                NSLog(@"Services JSON too large for buffer");
                strcpy(buffer, "[]");
            }
        } else {
            NSLog(@"Error creating services JSON: %@", error.localizedDescription);
            strcpy(buffer, "[]");
        }
        
        return buffer;
    }
    
    // Get all characteristics for a specific service as JSON string
    const char* _getServiceCharacteristics(const char* deviceId, const char* serviceUUID) {
        static char buffer[3072]; // Static buffer for safe string return
        buffer[0] = '\0'; // Initialize as empty string
        
        if (!deviceId || !serviceUUID) {
            NSLog(@"Error: Null parameters passed to _getServiceCharacteristics");
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSString *deviceIdString = [NSString stringWithUTF8String:deviceId];
        NSString *serviceUUIDString = [NSString stringWithUTF8String:serviceUUID];
        UnityBluetoothManager *manager = [UnityBluetoothManager sharedInstance];
        
        NSDictionary *deviceCharacteristics = manager.peripheralCharacteristics[deviceIdString];
        if (!deviceCharacteristics) {
            // Return empty JSON array if no device found
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSDictionary *serviceCharacteristics = deviceCharacteristics[serviceUUIDString];
        if (!serviceCharacteristics) {
            // Return empty JSON array if service not found
            strcpy(buffer, "[]");
            return buffer;
        }
        
        NSMutableArray *characteristicsArray = [[NSMutableArray alloc] init];
        
        // Iterate through characteristics in this service
        for (NSString *characteristicUUID in serviceCharacteristics) {
            CBCharacteristic *characteristic = serviceCharacteristics[characteristicUUID];
            
            // Build properties array
            NSMutableArray *properties = [[NSMutableArray alloc] init];
            if (characteristic.properties & CBCharacteristicPropertyRead) {
                [properties addObject:@"read"];
            }
            if (characteristic.properties & CBCharacteristicPropertyWrite) {
                [properties addObject:@"write"];
            }
            if (characteristic.properties & CBCharacteristicPropertyWriteWithoutResponse) {
                [properties addObject:@"writeWithoutResponse"];
            }
            if (characteristic.properties & CBCharacteristicPropertyNotify) {
                [properties addObject:@"notify"];
            }
            if (characteristic.properties & CBCharacteristicPropertyIndicate) {
                [properties addObject:@"indicate"];
            }
            
            NSDictionary *characteristicInfo = @{
                @"characteristicUUID": characteristicUUID,
                @"properties": properties,
                @"isNotifying": @(characteristic.isNotifying)
            };
            
            [characteristicsArray addObject:characteristicInfo];
        }
        
        NSError *error;
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:characteristicsArray options:NSJSONWritingPrettyPrinted error:&error];
        if (jsonData && !error) {
            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            if (jsonString.length < sizeof(buffer)) {
                strcpy(buffer, [jsonString UTF8String]);
            } else {
                NSLog(@"Service characteristics JSON too large for buffer");
                strcpy(buffer, "[]");
            }
        } else {
            NSLog(@"Error creating service characteristics JSON: %@", error.localizedDescription);
            strcpy(buffer, "[]");
        }
        
        return buffer;
    }
}
