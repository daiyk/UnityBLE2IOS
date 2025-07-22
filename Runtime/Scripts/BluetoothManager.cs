
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityBLE2IOS
{
    public class BluetoothManager : MonoBehaviour
    {
        /// <summary>
        /// AutoInitialize the Bluetooth manager
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance.isInitialized) return;

#if UNITY_IOS && !UNITY_EDITOR
            _initializeBluetooth();
            _requestPermissions();
#endif
            Instance.isInitialized = true;
            Debug.Log("Bluetooth Manager initialized");
        }
        private static BluetoothManager _instance;

        /// call this method to check if the instance exists before destroying or creating a new instance(e.g. OnDestroy)
        public static bool HasInstance
        {
            get { return _instance != null; }
        }
        public static BluetoothManager Instance
        {
            get
            {
                GameObject go = GameObject.Find("BluetoothManager");
                if (go != null)
                {
                    _instance = go.GetComponent<BluetoothManager>();
                    if (_instance == null)
                    {
                        go.AddComponent<BluetoothManager>();
                        _instance = go.GetComponent<BluetoothManager>();
                        Debug.LogWarning("BluetoothManager component was missing, added automatically.");
                    }
                }
                else
                {
                    go = new GameObject("BluetoothManager");
                    _instance = go.AddComponent<BluetoothManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Events
        public event Action<bool> OnBluetoothStateChanged;
        public event Action<BluetoothDevice> OnDeviceDiscovered;
        public event Action<string> OnDeviceConnected;
        public event Action<string> OnDeviceDisconnected;
        public event Action<string, string> OnConnectionFailed;
        public event Action<bool> OnPermissionResult;
        public event Action<CharacteristicValueMessage> OnCharacteristicValueReceived;
        public event Action<CharacteristicWriteResult> OnCharacteristicWriteSuccess;
        public event Action<CharacteristicWriteResult> OnCharacteristicWriteError;

#if UNITY_IOS && !UNITY_EDITOR
        // Native iOS methods
        [DllImport("__Internal")]
        private static extern void _initializeBluetooth();
        
        [DllImport("__Internal")]
        private static extern void _requestPermissions();
        
        [DllImport("__Internal")]
        private static extern void _startScanning();
        
        [DllImport("__Internal")]
        private static extern void _stopScanning();
        
        [DllImport("__Internal")]
        private static extern void _connectToDevice(string deviceId);
        
        [DllImport("__Internal")]
        private static extern void _disconnectDevice(string deviceId);
        
        [DllImport("__Internal")]
        private static extern bool _isBluetoothEnabled();
        
        [DllImport("__Internal")]
        private static extern bool _isDeviceConnected(string deviceId);
        
        [DllImport("__Internal")]
        private static extern int _getDiscoveredDeviceCount();
        
        [DllImport("__Internal")]
        private static extern string _getDiscoveredDeviceInfo(int index);
        
        [DllImport("__Internal")]
        private static extern void _writeCharacteristic(string deviceId, string characteristicUUID, string hexData);
        
        [DllImport("__Internal")]
        private static extern void _subscribeToCharacteristic(string deviceId, string characteristicUUID);
        
        [DllImport("__Internal")]
        private static extern void _unsubscribeFromCharacteristic(string deviceId, string characteristicUUID);
        
        [DllImport("__Internal")]
        private static extern string _getDeviceCharacteristics(string deviceId);
        
        [DllImport("__Internal")]
        private static extern string _getDeviceServices(string deviceId);
        
        [DllImport("__Internal")]
        private static extern string _getServiceCharacteristics(string deviceId, string serviceUUID);
#endif

        private List<BluetoothDevice> discoveredDevices = new List<BluetoothDevice>();
        private Dictionary<string, BluetoothDevice> connectedDevices = new Dictionary<string, BluetoothDevice>();
        private bool isInitialized = false;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }


        /// <summary>
        /// Request Bluetooth permissions from the user
        /// </summary>
        public void RequestPermissions()
        {
            Debug.Log("Requesting Bluetooth permissions...");
#if UNITY_IOS && !UNITY_EDITOR
            _requestPermissions();
#else
            // Simulate permission granted in editor
            OnPermissionResult?.Invoke(true);
#endif
        }

        /// <summary>
        /// Start scanning for BLE devices
        /// </summary>
        public void StartScanning()
        {
            Debug.Log("Starting Bluetooth scanning...");
            discoveredDevices.Clear();
#if UNITY_IOS && !UNITY_EDITOR
            _startScanning();
#else
            // Simulate device discovery in editor
            SimulateDeviceDiscovery();
#endif
        }

        /// <summary>
        /// Stop scanning for BLE devices
        /// </summary>
        public void StopScanning()
        {
            Debug.Log("Stopping Bluetooth scanning...");
#if UNITY_IOS && !UNITY_EDITOR
            _stopScanning();
#endif
        }

        /// <summary>
        /// Connect to a specific Bluetooth device
        /// </summary>
        /// <param name="deviceId">The device ID to connect to</param>
        public void ConnectToDevice(string deviceId)
        {
            Debug.Log($"Connecting to device: {deviceId}");
#if UNITY_IOS && !UNITY_EDITOR
            _connectToDevice(deviceId);
#else
            // Simulate successful connection in editor
            OnDeviceConnected?.Invoke(deviceId);
#endif
        }

        /// <summary>
        /// Disconnect from a specific Bluetooth device
        /// </summary>
        /// <param name="deviceId">The device ID to disconnect from</param>
        public void DisconnectDevice(string deviceId)
        {
            Debug.Log($"Disconnecting from device: {deviceId}");
#if UNITY_IOS && !UNITY_EDITOR
            _disconnectDevice(deviceId);
#else
            // Simulate disconnection in editor
            OnDeviceDisconnected?.Invoke(deviceId);
#endif
        }

        /// <summary>
        /// Check if Bluetooth is enabled on the device
        /// </summary>
        /// <returns>True if Bluetooth is enabled</returns>
        public bool IsBluetoothEnabled()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _isBluetoothEnabled();
#else
            return true; // Simulate enabled in editor
#endif
        }

        /// <summary>
        /// Check if a specific device is connected
        /// </summary>
        /// <param name="deviceId">The device ID to check</param>
        /// <returns>True if the device is connected</returns>
        public bool IsDeviceConnected(string deviceId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _isDeviceConnected(deviceId);
#else
            return false; // Simulate not connected in editor
#endif
        }

        /// <summary>
        /// Get list of discovered devices
        /// </summary>
        /// <returns>List of discovered Bluetooth devices</returns>
        public List<BluetoothDevice> GetDiscoveredDevices()
        {
            return new List<BluetoothDevice>(discoveredDevices);
        }

        /// <summary>
        /// Clear all discovered devices
        /// </summary>
        public void ClearDiscoveredDevices()
        {
            discoveredDevices.Clear();
            Debug.Log("Cleared discovered devices list");
        }

        /// <summary>
        /// Get a discovered device by its ID
        /// </summary>
        /// <param name="deviceId">The device ID to look for</param>
        /// <returns>BluetoothDevice object or null if not found</returns>
        public BluetoothDevice GetDiscoveredDevice(string deviceId)
        {
            return discoveredDevices.Find(d => d.deviceId == deviceId);
        }

        /// <summary>
        /// Check if a device has been discovered
        /// </summary>
        /// <param name="deviceId">The device ID to check</param>
        /// <returns>True if the device has been discovered</returns>
        public bool IsDeviceDiscovered(string deviceId)
        {
            return discoveredDevices.Any(d => d.deviceId == deviceId);
        }

        /// <summary>
        /// Get list of connected devices
        /// </summary>
        /// <returns>List of connected Bluetooth devices</returns>
        public List<BluetoothDevice> GetConnectedDevices()
        {
            return new List<BluetoothDevice>(connectedDevices.Values);
        }

        /// <summary>
        /// Get a connected device by its ID
        /// </summary>
        /// <param name="deviceId">The device ID to look for</param>
        /// <returns>BluetoothDevice object or null if not found</returns>
        public BluetoothDevice GetConnectedDevice(string deviceId)
        {
            return connectedDevices.TryGetValue(deviceId, out BluetoothDevice device) ? device : null;
        }

        /// <summary>
        /// Get count of connected devices
        /// </summary>
        /// <returns>Number of connected devices</returns>
        public int GetConnectedDeviceCount()
        {
            return connectedDevices.Count;
        }

        /// <summary>
        /// Get count of discovered devices from native layer
        /// </summary>
        /// <returns>Number of discovered devices</returns>
        public int GetDiscoveredDeviceCount()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _getDiscoveredDeviceCount();
#else
            return discoveredDevices.Count;
#endif
        }

        /// <summary>
        /// Get discovered device info by index from native layer
        /// </summary>
        /// <param name="index">Index of the device</param>
        /// <returns>BluetoothDevice object or null if not found</returns>
        public BluetoothDevice GetDiscoveredDeviceByIndex(int index)
        {
#if UNITY_IOS && !UNITY_EDITOR
            string deviceInfo = _getDiscoveredDeviceInfo(index);
            if (!string.IsNullOrEmpty(deviceInfo))
            {
                try
                {
                    return JsonUtility.FromJson<BluetoothDevice>(deviceInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing device info at index {index}: {e.Message}");
                    return null;
                }
            }
            return null;
#else
            if (index >= 0 && index < discoveredDevices.Count)
            {
                return discoveredDevices[index];
            }
            return null;
#endif
        }

        /// <summary>
        /// Get a summary of current connection status
        /// </summary>
        /// <returns>String containing connection status information</returns>
        public string GetConnectionStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine($"Bluetooth Enabled: {IsBluetoothEnabled()}");
            status.AppendLine($"Discovered Devices: {discoveredDevices.Count}");
            status.AppendLine($"Connected Devices: {connectedDevices.Count}");
            
            if (connectedDevices.Count > 0)
            {
                status.AppendLine("Connected devices:");
                foreach (var device in connectedDevices.Values)
                {
                    status.AppendLine($"  - {device.name} ({device.deviceId})");
                }
            }
            
            return status.ToString();
        }

        /// <summary>
        /// Write data to a BLE characteristic
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="characteristicUUID">The characteristic UUID</param>
        /// <param name="data">Data to write as byte array</param>
        public void WriteCharacteristic(string deviceId, string characteristicUUID, byte[] data)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("WriteCharacteristic: Device ID is null or empty");
                return;
            }
            
            if (string.IsNullOrEmpty(characteristicUUID))
            {
                Debug.LogWarning("WriteCharacteristic: Characteristic UUID is null or empty");
                return;
            }
            
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning("WriteCharacteristic: Data is null or empty");
                return;
            }

            // Convert byte array to hex string
            string hexData = System.BitConverter.ToString(data).Replace("-", "").ToLower();
            Debug.Log($"Writing to characteristic {characteristicUUID} on device {deviceId}: {hexData}");

#if UNITY_IOS && !UNITY_EDITOR
            _writeCharacteristic(deviceId, characteristicUUID, hexData);
#else
            // Simulate successful write in editor
            var result = new CharacteristicWriteResult(deviceId, characteristicUUID);
            OnCharacteristicWriteSuccess?.Invoke(result);
#endif
        }

        /// <summary>
        /// Write data to a BLE characteristic using hex string
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="characteristicUUID">The characteristic UUID</param>
        /// <param name="hexData">Data to write as hex string</param>
        public void WriteCharacteristic(string deviceId, string characteristicUUID, string hexData)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("WriteCharacteristic: Device ID is null or empty");
                return;
            }
            
            if (string.IsNullOrEmpty(characteristicUUID))
            {
                Debug.LogWarning("WriteCharacteristic: Characteristic UUID is null or empty");
                return;
            }
            
            if (string.IsNullOrEmpty(hexData))
            {
                Debug.LogWarning("WriteCharacteristic: Hex data is null or empty");
                return;
            }

            Debug.Log($"Writing to characteristic {characteristicUUID} on device {deviceId}: {hexData}");

#if UNITY_IOS && !UNITY_EDITOR
            _writeCharacteristic(deviceId, characteristicUUID, hexData);
#else
            // Simulate successful write in editor
            var result = new CharacteristicWriteResult(deviceId, characteristicUUID);
            OnCharacteristicWriteSuccess?.Invoke(result);
#endif
        }

        /// <summary>
        /// Subscribe to notifications from a BLE characteristic
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="characteristicUUID">The characteristic UUID</param>
        public void SubscribeToCharacteristic(string deviceId, string characteristicUUID)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("SubscribeToCharacteristic: Device ID is null or empty");
                return;
            }
            
            if (string.IsNullOrEmpty(characteristicUUID))
            {
                Debug.LogWarning("SubscribeToCharacteristic: Characteristic UUID is null or empty");
                return;
            }
            
            Debug.Log($"Subscribing to characteristic {characteristicUUID} on device {deviceId}");

#if UNITY_IOS && !UNITY_EDITOR
            _subscribeToCharacteristic(deviceId, characteristicUUID);
#else
            // Simulate subscription in editor
            Debug.Log($"Simulated subscription to {characteristicUUID}");
#endif
        }

        /// <summary>
        /// Unsubscribe from notifications from a BLE characteristic
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="characteristicUUID">The characteristic UUID</param>
        public void UnsubscribeFromCharacteristic(string deviceId, string characteristicUUID)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("UnsubscribeFromCharacteristic: Device ID is null or empty");
                return;
            }
            
            if (string.IsNullOrEmpty(characteristicUUID))
            {
                Debug.LogWarning("UnsubscribeFromCharacteristic: Characteristic UUID is null or empty");
                return;
            }
            
            Debug.Log($"Unsubscribing from characteristic {characteristicUUID} on device {deviceId}");

#if UNITY_IOS && !UNITY_EDITOR
            _unsubscribeFromCharacteristic(deviceId, characteristicUUID);
#else
            // Simulate unsubscription in editor
            Debug.Log($"Simulated unsubscription from {characteristicUUID}");
#endif
        }

        /// <summary>
        /// Get all characteristics for a device
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <returns>Array of BluetoothCharacteristic objects</returns>
        public BluetoothCharacteristic[] GetDeviceCharacteristics(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("GetDeviceCharacteristics: Device ID is null or empty");
                return new BluetoothCharacteristic[] { };
            }
            
#if UNITY_IOS && !UNITY_EDITOR
            string characteristicsJson = _getDeviceCharacteristics(deviceId);
            if (!string.IsNullOrEmpty(characteristicsJson))
            {
                try
                {
                    // Parse JSON array of characteristics
                    var wrapper = JsonUtility.FromJson<CharacteristicArrayWrapper>("{\"items\":" + characteristicsJson + "}");
                    return wrapper.items;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing device characteristics: {e.Message}");
                    return new BluetoothCharacteristic[] { };
                }
            }
            return new BluetoothCharacteristic[] { };
#else
            // Simulate characteristics in editor
            return new BluetoothCharacteristic[]
            {
                new BluetoothCharacteristic("180F", "2A19", new string[] { "read", "notify" }, false), // Battery Level
                new BluetoothCharacteristic("1800", "2A00", new string[] { "read" }, false) // Device Name
            };
#endif
        }

        /// <summary>
        /// Get all services for a device
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <returns>Array of BluetoothService objects</returns>
        public BluetoothService[] GetDeviceServices(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("GetDeviceServices: Device ID is null or empty");
                return new BluetoothService[] { };
            }
            
#if UNITY_IOS && !UNITY_EDITOR
            string servicesJson = _getDeviceServices(deviceId);
            if (!string.IsNullOrEmpty(servicesJson))
            {
                try
                {
                    // Parse JSON array of services
                    var wrapper = JsonUtility.FromJson<ServiceArrayWrapper>("{\"items\":" + servicesJson + "}");
                    return wrapper.items;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing device services: {e.Message}");
                    return new BluetoothService[] { };
                }
            }
            return new BluetoothService[] { };
#else
            // Simulate services in editor
            return new BluetoothService[]
            {
                new BluetoothService("180F", 1), // Battery Service
                new BluetoothService("1800", 2)  // Generic Access Service
            };
#endif
        }

        /// <summary>
        /// Get characteristics for a specific service
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="serviceUUID">The service UUID</param>
        /// <returns>Array of BluetoothCharacteristic objects</returns>
        public BluetoothCharacteristic[] GetServiceCharacteristics(string deviceId, string serviceUUID)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogWarning("GetServiceCharacteristics: Device ID is null or empty");
                return new BluetoothCharacteristic[] { };
            }
            
            if (string.IsNullOrEmpty(serviceUUID))
            {
                Debug.LogWarning("GetServiceCharacteristics: Service UUID is null or empty");
                return new BluetoothCharacteristic[] { };
            }
            
#if UNITY_IOS && !UNITY_EDITOR
            string characteristicsJson = _getServiceCharacteristics(deviceId, serviceUUID);
            if (!string.IsNullOrEmpty(characteristicsJson))
            {
                try
                {
                    // Parse JSON array of characteristics
                    var wrapper = JsonUtility.FromJson<CharacteristicArrayWrapper>("{\"items\":" + characteristicsJson + "}");
                    return wrapper.items;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing service characteristics: {e.Message}");
                    return new BluetoothCharacteristic[] { };
                }
            }
            return new BluetoothCharacteristic[] { };
#else
            // Simulate characteristics for service in editor
            if (serviceUUID == "180F")
            {
                return new BluetoothCharacteristic[]
                {
                    new BluetoothCharacteristic(serviceUUID, "2A19", new string[] { "read", "notify" }, false) // Battery Level
                };
            }
            else if (serviceUUID == "1800")
            {
                return new BluetoothCharacteristic[]
                {
                    new BluetoothCharacteristic(serviceUUID, "2A00", new string[] { "read" }, false), // Device Name
                    new BluetoothCharacteristic(serviceUUID, "2A01", new string[] { "read" }, false)  // Appearance
                };
            }
            return new BluetoothCharacteristic[] { };
#endif
        }

        /// <summary>
        /// Disconnect all connected devices
        /// </summary>
        public void DisconnectAllDevices()
        {
            var deviceIds = new List<string>(connectedDevices.Keys);
            foreach (string deviceId in deviceIds)
            {
                DisconnectDevice(deviceId);
            }
            Debug.Log($"Initiated disconnection for {deviceIds.Count} devices");
        }

        // Called from native iOS code
        public void OnBluetoothStateChangedNative(string enabled)
        {
            bool isEnabled = enabled == "1";
            Debug.Log($"Bluetooth state changed: {isEnabled}");
            OnBluetoothStateChanged?.Invoke(isEnabled);
        }

        // Called from native iOS code
        public void OnDeviceDiscoveredNative(string deviceInfo)
        {
            try
            {
                BluetoothDevice device = JsonUtility.FromJson<BluetoothDevice>(deviceInfo);
                
                // Check if device already exists to avoid duplicates
                BluetoothDevice existingDevice = discoveredDevices.Find(d => d.deviceId == device.deviceId);
                if (existingDevice != null)
                {
                    // Update existing device with new info (RSSI might have changed)
                    existingDevice.rssi = device.rssi;
                    existingDevice.serviceUUIDs = device.serviceUUIDs;
                    existingDevice.manufacturerData = device.manufacturerData;
                    existingDevice.localName = device.localName;
                    existingDevice.txPowerLevel = device.txPowerLevel;
                    Debug.Log($"Updated device: {device.name} ({device.deviceId}) RSSI: {device.rssi}");
                }
                else
                {
                    // Add new device
                    discoveredDevices.Add(device);
                    Debug.Log($"New device discovered: {device.name} ({device.deviceId}) RSSI: {device.rssi}");
                }
                
                OnDeviceDiscovered?.Invoke(device);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing device info: {e.Message}\nDevice info: {deviceInfo}");
            }
        }

        // Called from native iOS code
        public void OnDeviceConnectedNative(string deviceId)
        {
            Debug.Log($"Device connected: {deviceId}");
            
            // Find the device in discovered devices to store its full info
            BluetoothDevice connectedDevice = discoveredDevices.Find(d => d.deviceId == deviceId);
            
            // If not found in discovered devices, try to get it from native layer
            if (connectedDevice == null)
            {
                // Try to find it by searching through discovered devices from native layer
                int deviceCount = GetDiscoveredDeviceCount();
                for (int i = 0; i < deviceCount; i++)
                {
                    BluetoothDevice device = GetDiscoveredDeviceByIndex(i);
                    if (device != null && device.deviceId == deviceId)
                    {
                        connectedDevice = device;
                        break;
                    }
                }
            }
            
            // If still not found, create a minimal device object
            if (connectedDevice == null)
            {
                connectedDevice = new BluetoothDevice
                {
                    deviceId = deviceId,
                    name = "Unknown Device",
                    rssi = 0
                };
            }
            
            // Store the connected device
            connectedDevices[deviceId] = connectedDevice;
            
            OnDeviceConnected?.Invoke(deviceId);
        }

        // Called from native iOS code
        public void OnDeviceDisconnectedNative(string deviceId)
        {
            Debug.Log($"Device disconnected: {deviceId}");
            
            // Remove the device from connected devices
            if (connectedDevices.ContainsKey(deviceId))
            {
                connectedDevices.Remove(deviceId);
                Debug.Log($"Removed device {deviceId} from connected devices list");
            }
            
            OnDeviceDisconnected?.Invoke(deviceId);
        }

        // Called from native iOS code
        public void OnConnectionFailedNative(string errorInfo)
        {
            string[] parts = errorInfo.Split('|');
            string deviceId = parts.Length > 0 ? parts[0] : "";
            string error = parts.Length > 1 ? parts[1] : "Unknown error";
            Debug.LogError($"Connection failed for device {deviceId}: {error}");
            OnConnectionFailed?.Invoke(deviceId, error);
        }

        // Called from native iOS code
        public void OnPermissionResultNative(string granted)
        {
            bool isGranted = granted == "1";
            Debug.Log($"Permission result: {isGranted}");
            OnPermissionResult?.Invoke(isGranted);
        }

        // Called from native iOS code
        public void OnCharacteristicValueReceivedNative(string messageJson)
        {
            try
            {
                CharacteristicValueMessage message = JsonUtility.FromJson<CharacteristicValueMessage>(messageJson);
                Debug.Log($"Received data from characteristic {message.characteristicUUID} on device {message.deviceId}: {message.data}");
                OnCharacteristicValueReceived?.Invoke(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing characteristic value message: {e.Message}\nMessage: {messageJson}");
            }
        }

        // Called from native iOS code
        public void OnCharacteristicWriteSuccessNative(string resultJson)
        {
            try
            {
                CharacteristicWriteResult result = JsonUtility.FromJson<CharacteristicWriteResult>(resultJson);
                Debug.Log($"Successfully wrote to characteristic {result.characteristicUUID} on device {result.deviceId}");
                OnCharacteristicWriteSuccess?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing characteristic write success: {e.Message}\nResult: {resultJson}");
            }
        }

        // Called from native iOS code
        public void OnCharacteristicWriteErrorNative(string resultJson)
        {
            try
            {
                CharacteristicWriteResult result = JsonUtility.FromJson<CharacteristicWriteResult>(resultJson);
                Debug.LogError($"Failed to write to characteristic {result.characteristicUUID} on device {result.deviceId}: {result.error}");
                OnCharacteristicWriteError?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing characteristic write error: {e.Message}\nResult: {resultJson}");
            }
        }

#if UNITY_EDITOR
        // Simulate device discovery for testing in editor
        private void SimulateDeviceDiscovery()
        {
            // Simulate discovering multiple devices with different characteristics
            var simulatedDevices = new List<BluetoothDevice>
            {
                new BluetoothDevice
                {
                    deviceId = "simulated-device-001",
                    name = "Fitness Tracker",
                    rssi = -45,
                    serviceUUIDs = new string[] { "180D", "180F" }, // Heart Rate & Battery Service
                    manufacturerData = "4c001005071c123456",
                    localName = "FitTracker Pro",
                    txPowerLevel = 4
                },
                new BluetoothDevice
                {
                    deviceId = "simulated-device-002", 
                    name = "Smart Watch",
                    rssi = -62,
                    serviceUUIDs = new string[] { "1800", "1801", "180F" },
                    manufacturerData = "4c00100507ab654321",
                    localName = "SmartWatch X1",
                    txPowerLevel = 0
                },
                new BluetoothDevice
                {
                    deviceId = "simulated-device-003",
                    name = "Temperature Sensor",
                    rssi = -38,
                    serviceUUIDs = new string[] { "181A" }, // Environmental Sensing
                    manufacturerData = "",
                    localName = "TempSense v2",
                    txPowerLevel = -4
                },
                new BluetoothDevice
                {
                    deviceId = "vernier-gdx-tmp-001",
                    name = "GDX-TMP 071000ABC",
                    rssi = -52,
                    serviceUUIDs = new string[] { "f4bf14a6-c7d5-4b6d-8aa8-df1a7c83adcb", "b41e6675-a329-40e0-aa01-44d2f444babe", "180F" }, // Vernier Command & Response Services, Battery
                    manufacturerData = "5700010203040506",
                    localName = "GDX-TMP 071000ABC",
                    txPowerLevel = 0
                },
                new BluetoothDevice
                {
                    deviceId = "vernier-gdx-for-002",
                    name = "GDX-FOR 071000DEF",
                    rssi = -48,
                    serviceUUIDs = new string[] { "f4bf14a6-c7d5-4b6d-8aa8-df1a7c83adcb", "b41e6675-a329-40e0-aa01-44d2f444babe", "180A" }, // Vernier Command & Response Services, Device Info
                    manufacturerData = "570001abcdef1234",
                    localName = "GDX-FOR 071000DEF",
                    txPowerLevel = 2
                }
            };

            // Simulate devices being discovered over time
            StartCoroutine(SimulateDiscoveryCoroutine(simulatedDevices));
        }

        private System.Collections.IEnumerator SimulateDiscoveryCoroutine(List<BluetoothDevice> devices)
        {
            foreach (var device in devices)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 2.0f));
                discoveredDevices.Add(device);
                OnDeviceDiscovered?.Invoke(device);
                Debug.Log($"Simulated device discovered: {device.name} ({device.deviceId})");
            }
        }
#endif
    }

    // Helper classes for JSON array parsing
    [System.Serializable]
    internal class CharacteristicArrayWrapper
    {
        public BluetoothCharacteristic[] items;
    }

    [System.Serializable]
    internal class ServiceArrayWrapper
    {
        public BluetoothService[] items;
    }
}
