//
// Basic BLE Scanner Example
// 
// This example demonstrates how to use the UnityBLE2IOS plugin to:
// - Request Bluetooth permissions
// - Scan for BLE devices
// - Connect/disconnect from devices
// - Handle Bluetooth state changes
//
// Note: The BluetoothManager now auto-initializes when first accessed.
// No manual Initialize() call is required.
//
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityBLE2IOS;

public class BluetoothExample : MonoBehaviour
{
    [Header("UI References")]
    public Button scanButton;
    public Button stopScanButton;
    public Button requestPermissionsButton;
    public Text statusText;
    public Transform deviceListParent;
    public GameObject deviceItemPrefab;

    private Dictionary<string, GameObject> deviceItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        // Subscribe to events
        BluetoothManager.Instance.OnBluetoothStateChanged += OnBluetoothStateChanged;
        BluetoothManager.Instance.OnDeviceDiscovered += OnDeviceDiscovered;
        BluetoothManager.Instance.OnDeviceConnected += OnDeviceConnected;
        BluetoothManager.Instance.OnDeviceDisconnected += OnDeviceDisconnected;
        BluetoothManager.Instance.OnConnectionFailed += OnConnectionFailed;
        BluetoothManager.Instance.OnPermissionResult += OnPermissionResult;

        // Setup UI
        SetupUI();
        
        // Check initial Bluetooth state
        UpdateBluetoothStatus();
    }

    private void SetupUI()
    {
        if (scanButton != null)
            scanButton.onClick.AddListener(StartScanning);
            
        if (stopScanButton != null)
            stopScanButton.onClick.AddListener(StopScanning);
            
        if (requestPermissionsButton != null)
            requestPermissionsButton.onClick.AddListener(RequestPermissions);

        UpdateStatusText("Bluetooth Manager ready. Request permissions to begin scanning.");
    }

    private void RequestPermissions()
    {
        UpdateStatusText("Requesting Bluetooth permissions...");
        BluetoothManager.Instance.RequestPermissions();
    }

    private void StartScanning()
    {
        if (!BluetoothManager.Instance.IsBluetoothEnabled())
        {
            UpdateStatusText("Bluetooth is not enabled!");
            return;
        }

        UpdateStatusText("Scanning for devices...");
        ClearDeviceList();
        BluetoothManager.Instance.StartScanning();
    }

    private void StopScanning()
    {
        UpdateStatusText("Stopped scanning");
        BluetoothManager.Instance.StopScanning();
    }

    private void ClearDeviceList()
    {
        foreach (var item in deviceItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        deviceItems.Clear();
    }

    #region Event Handlers

    private void OnBluetoothStateChanged(bool isEnabled)
    {
        UpdateBluetoothStatus();
        if (isEnabled)
        {
            UpdateStatusText("Bluetooth enabled");
        }
        else
        {
            UpdateStatusText("Bluetooth disabled");
            ClearDeviceList();
        }
    }

    private void OnDeviceDiscovered(BluetoothDevice device)
    {
        UpdateStatusText($"Found device: {device.name}");
        AddDeviceToList(device);
    }

    private void OnDeviceConnected(string deviceId)
    {
        UpdateStatusText($"Connected to device: {deviceId}");
        UpdateDeviceConnectionStatus(deviceId, true);
    }

    private void OnDeviceDisconnected(string deviceId)
    {
        UpdateStatusText($"Disconnected from device: {deviceId}");
        UpdateDeviceConnectionStatus(deviceId, false);
    }

    private void OnConnectionFailed(string deviceId, string error)
    {
        UpdateStatusText($"Connection failed: {error}");
    }

    private void OnPermissionResult(bool granted)
    {
        if (granted)
        {
            UpdateStatusText("Bluetooth permissions granted! You can now scan for devices.");
        }
        else
        {
            UpdateStatusText("Bluetooth permissions denied. Please enable in Settings.");
        }
    }

    #endregion

    #region UI Updates

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Bluetooth Status: {message}");
    }

    private void UpdateBluetoothStatus()
    {
        bool isEnabled = BluetoothManager.Instance.IsBluetoothEnabled();
        if (scanButton != null)
            scanButton.interactable = isEnabled;
    }

    private void AddDeviceToList(BluetoothDevice device)
    {
        if (deviceItems.ContainsKey(device.deviceId))
            return; // Already added

        if (deviceListParent == null || deviceItemPrefab == null)
            return;

        GameObject deviceItem = Instantiate(deviceItemPrefab, deviceListParent);
        deviceItems[device.deviceId] = deviceItem;

        // Setup device item UI
        DeviceListItem deviceComponent = deviceItem.GetComponent<DeviceListItem>();
        if (deviceComponent != null)
        {
            deviceComponent.Setup(device, OnDeviceItemClicked);
        }
    }

    private void UpdateDeviceConnectionStatus(string deviceId, bool isConnected)
    {
        if (deviceItems.TryGetValue(deviceId, out GameObject deviceItem))
        {
            DeviceListItem deviceComponent = deviceItem.GetComponent<DeviceListItem>();
            if (deviceComponent != null)
            {
                deviceComponent.UpdateConnectionStatus(isConnected);
            }
        }
    }

    private void OnDeviceItemClicked(BluetoothDevice device)
    {
        if (BluetoothManager.Instance.IsDeviceConnected(device.deviceId))
        {
            UpdateStatusText($"Disconnecting from {device.name}...");
            BluetoothManager.Instance.DisconnectDevice(device.deviceId);
        }
        else
        {
            UpdateStatusText($"Connecting to {device.name}...");
            BluetoothManager.Instance.ConnectToDevice(device.deviceId);
        }
    }

    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (BluetoothManager.Instance != null)
        {
            BluetoothManager.Instance.OnBluetoothStateChanged -= OnBluetoothStateChanged;
            BluetoothManager.Instance.OnDeviceDiscovered -= OnDeviceDiscovered;
            BluetoothManager.Instance.OnDeviceConnected -= OnDeviceConnected;
            BluetoothManager.Instance.OnDeviceDisconnected -= OnDeviceDisconnected;
            BluetoothManager.Instance.OnConnectionFailed -= OnConnectionFailed;
            BluetoothManager.Instance.OnPermissionResult -= OnPermissionResult;
        }
    }
}
