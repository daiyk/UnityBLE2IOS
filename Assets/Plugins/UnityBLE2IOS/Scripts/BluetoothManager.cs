using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace UnityBLE2IOS
{
    public class BluetoothManager : MonoBehaviour
    {
        private static BluetoothManager _instance;
        public static BluetoothManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("BluetoothManager");
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
#endif

        private List<BluetoothDevice> discoveredDevices = new List<BluetoothDevice>();
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
        /// Initialize the Bluetooth manager
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

#if UNITY_IOS && !UNITY_EDITOR
            _initializeBluetooth();
#endif
            isInitialized = true;
            Debug.Log("Bluetooth Manager initialized");
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
                discoveredDevices.Add(device);
                Debug.Log($"Device discovered: {device.name} ({device.deviceId})");
                OnDeviceDiscovered?.Invoke(device);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing device info: {e.Message}");
            }
        }

        // Called from native iOS code
        public void OnDeviceConnectedNative(string deviceId)
        {
            Debug.Log($"Device connected: {deviceId}");
            OnDeviceConnected?.Invoke(deviceId);
        }

        // Called from native iOS code
        public void OnDeviceDisconnectedNative(string deviceId)
        {
            Debug.Log($"Device disconnected: {deviceId}");
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

#if UNITY_EDITOR
        // Simulate device discovery for testing in editor
        private void SimulateDeviceDiscovery()
        {
            BluetoothDevice simulatedDevice = new BluetoothDevice
            {
                deviceId = "simulated-device-001",
                name = "Simulated BLE Device",
                rssi = -45
            };
            discoveredDevices.Add(simulatedDevice);
            OnDeviceDiscovered?.Invoke(simulatedDevice);
        }
#endif
    }
}
