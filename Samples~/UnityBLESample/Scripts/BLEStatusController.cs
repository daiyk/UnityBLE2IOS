using UnityEngine;
using UnityEngine.UI;
using UnityBLE2IOS;
using System.Collections.Generic;
using TMPro;

public class BLEStatusController : MonoBehaviour
{
    [SerializeField]
    private Button m_ConnectButton;
    [SerializeField]
    private Button m_DisconnectButton;
    [SerializeField]
    private Button m_ScanButton;
    [SerializeField]
    private GameObject m_ItemContainer;
    [SerializeField]
    private GameObject m_DeviceItemPrefab; // Prefab for BLE device items
    
    [Header("Button Colors")]
    [SerializeField]
    private Color m_EnabledColor = Color.white;
    [SerializeField]
    private Color m_DisabledColor = Color.gray;
    [SerializeField]
    private Color m_ScanningColor = Color.yellow;
    [SerializeField]
    private Color m_ConnectedColor = Color.green;
    [Header("Debug Console")]
    [SerializeField]
    private TextMeshProUGUI m_DebugConsole; // Reference to the debug console text
    [SerializeField]
    private ScrollRect m_DebugScrollRect; // Optional: for auto-scrolling
    [SerializeField]
    private int m_MaxDebugLines = 20; // Maximum lines to keep in debug console
    
    private List<GameObject> m_DeviceItems = new List<GameObject>();
    private BluetoothDevice m_SelectedDevice = null;
    private bool m_IsScanning = false;
    private bool m_IsConnected = false;
    private string m_ConnectedDeviceId = "";
    
    private List<string> m_DebugMessages = new List<string>();
    private float m_LastStatusUpdate = 0f;
    private const float STATUS_UPDATE_INTERVAL = 2f; // Update status every 2 seconds
    private string m_LastStatus = ""; // Moved from method to class level
    
    void Start()
    {
        // Subscribe to Bluetooth events
        BluetoothManager.Instance.OnDeviceDiscovered += OnDeviceDiscovered;
        BluetoothManager.Instance.OnDeviceConnected += OnDeviceConnected;
        BluetoothManager.Instance.OnDeviceDisconnected += OnDeviceDisconnected;
        BluetoothManager.Instance.OnConnectionFailed += OnConnectionFailed;
        BluetoothManager.Instance.OnPermissionResult += OnPermissionResult;
        
        // Setup button listeners
        if (m_ScanButton != null)
            m_ScanButton.onClick.AddListener(OnScanButtonClicked);
        if (m_ConnectButton != null)
            m_ConnectButton.onClick.AddListener(OnConnectButtonClicked);
        if (m_DisconnectButton != null)
            m_DisconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        
        // Initial button state
        UpdateButtonStates();
        
        // Initialize debug console
        LogToDebugConsole("Bluetooth Manager initialized");
        LogToDebugConsole($"Platform: {Application.platform}");
        
        // Request permissions
        BluetoothManager.Instance.RequestPermissions();
    }
    
    void Update()
    {
        // Periodic status updates
        if (Time.time - m_LastStatusUpdate > STATUS_UPDATE_INTERVAL)
        {
            UpdateConnectionStatusDisplay();
            m_LastStatusUpdate = Time.time;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (BluetoothManager.HasInstance)
        {
            BluetoothManager.Instance.OnDeviceDiscovered -= OnDeviceDiscovered;
            BluetoothManager.Instance.OnDeviceConnected -= OnDeviceConnected;
            BluetoothManager.Instance.OnDeviceDisconnected -= OnDeviceDisconnected;
            BluetoothManager.Instance.OnConnectionFailed -= OnConnectionFailed;
            BluetoothManager.Instance.OnPermissionResult -= OnPermissionResult;
        }
    }
    
    private void OnScanButtonClicked()
    {
        if (m_IsScanning)
        {
            StopScanning();
        }
        else
        {
            StartScanning();
        }
    }
    
    private void OnConnectButtonClicked()
    {
        if (m_SelectedDevice != null && !m_IsConnected)
        {
            BluetoothManager.Instance.ConnectToDevice(m_SelectedDevice.deviceId);
        }
    }
    
    private void OnDisconnectButtonClicked()
    {
        if (m_IsConnected && !string.IsNullOrEmpty(m_ConnectedDeviceId))
        {
            BluetoothManager.Instance.DisconnectDevice(m_ConnectedDeviceId);
        }
    }
    
    private void StartScanning()
    {
        ClearDeviceList();
        m_IsScanning = true;
        BluetoothManager.Instance.StartScanning();
        UpdateButtonStates();
        LogToDebugConsole("🔍 Started scanning for BLE devices...");
    }
    
    private void StopScanning()
    {
        m_IsScanning = false;
        BluetoothManager.Instance.StopScanning();
        UpdateButtonStates();
        LogToDebugConsole($"⏹️ Stopped scanning. Found {m_DeviceItems.Count} devices");
    }
    
    private void ClearDeviceList()
    {
        foreach (GameObject item in m_DeviceItems)
        {
            Destroy(item);
        }
        m_DeviceItems.Clear();
        m_SelectedDevice = null;
        UpdateButtonStates();
    }
    
    private void OnDeviceDiscovered(BluetoothDevice device)
    {
        // Check if device already exists in the list
        foreach (GameObject item in m_DeviceItems)
        {
            BLEDeviceItem deviceItem = item.GetComponent<BLEDeviceItem>();
            if (deviceItem != null && deviceItem.DeviceId == device.deviceId)
            {
                // Update existing device info
                deviceItem.UpdateDeviceInfo(device);
                return;
            }
        }
        
        // Create new device item
        if (m_DeviceItemPrefab != null && m_ItemContainer != null)
        {
            GameObject newItem = Instantiate(m_DeviceItemPrefab, m_ItemContainer.transform);
            BLEDeviceItem deviceItem = newItem.GetComponent<BLEDeviceItem>();
            
            if (deviceItem != null)
            {
                deviceItem.SetDeviceInfo(device);
                deviceItem.OnDeviceSelected += OnDeviceSelected;
                m_DeviceItems.Add(newItem);
            }
        }
        
        LogToDebugConsole($"📱 Device found: {device.name} (RSSI: {device.rssi} dBm)");
        LogToDebugConsole($"   ID: {device.deviceId}");
        
        if (device.serviceUUIDs != null && device.serviceUUIDs.Length > 0)
        {
            LogToDebugConsole($"   Services: {string.Join(", ", device.serviceUUIDs)}");
        }
    }
    
    private void OnDeviceSelected(BluetoothDevice device)
    {
        m_SelectedDevice = device;
        UpdateButtonStates();
        LogToDebugConsole($"✅ Selected: {device.name}");
        LogToDebugConsole($"   Ready to connect to {device.deviceId}");
    }
    
    private void OnDeviceConnected(string deviceId)
    {
        m_IsConnected = true;
        m_ConnectedDeviceId = deviceId;
        
        // Stop scanning when connected
        if (m_IsScanning)
        {
            StopScanning();
        }
        
        UpdateButtonStates();
        
        // Get device info for detailed logging
        BluetoothDevice connectedDevice = BluetoothManager.Instance.GetConnectedDevice(deviceId);
        if (connectedDevice != null)
        {
            LogToDebugConsole($"🔗 CONNECTED to: {connectedDevice.name}");
            LogToDebugConsole($"   Device ID: {deviceId}");
            LogToDebugConsole($"   Signal: {connectedDevice.rssi} dBm");
        }
        else
        {
            LogToDebugConsole($"🔗 CONNECTED to device: {deviceId}");
        }
        
        // Display connection summary
        DisplayConnectionSummary();
    }
    
    private void OnDeviceDisconnected(string deviceId)
    {
        m_IsConnected = false;
        m_ConnectedDeviceId = "";
        UpdateButtonStates();
        LogToDebugConsole($"❌ DISCONNECTED from: {deviceId}");
        DisplayConnectionSummary();
    }
    
    private void OnConnectionFailed(string deviceId, string error)
    {
        UpdateButtonStates();
        LogToDebugConsole($"⚠️ CONNECTION FAILED: {deviceId}");
        LogToDebugConsole($"   Error: {error}");
    }
    
    private void OnPermissionResult(bool granted)
    {
        if (granted)
        {
            LogToDebugConsole("✅ Bluetooth permissions GRANTED");
        }
        else
        {
            LogToDebugConsole("❌ Bluetooth permissions DENIED");
        }
        UpdateButtonStates();
    }
    
    private void LogToDebugConsole(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";
        
        m_DebugMessages.Add(logMessage);
        
        // Keep only the last N messages
        while (m_DebugMessages.Count > m_MaxDebugLines)
        {
            m_DebugMessages.RemoveAt(0);
        }
        
        // Update the debug console display
        if (m_DebugConsole != null)
        {
            m_DebugConsole.text = string.Join("\n", m_DebugMessages);
            
            // Auto-scroll to bottom if ScrollRect is available
            if (m_DebugScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                m_DebugScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        // Also log to Unity console
        Debug.Log(logMessage);
    }
    
    private void UpdateConnectionStatusDisplay()
    {
        if (!m_IsConnected && !m_IsScanning && m_DeviceItems.Count == 0)
            return; // Don't spam when idle
            
        string status = GetDetailedConnectionStatus();
        
        // If connected update the status every 2 seconds
 
        LogToDebugConsole("📊 Status Update:");
        foreach (string line in status.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
                LogToDebugConsole($"   {line}");
        }
        m_LastStatus = status;
        
    }
    
    private void DisplayConnectionSummary()
    {
        string summary = BluetoothManager.Instance.GetConnectionStatus();
        LogToDebugConsole("📋 Connection Summary:");
        foreach (string line in summary.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
                LogToDebugConsole($"   {line}");
        }
    }
    
    private string GetDetailedConnectionStatus()
    {
        var status = new System.Text.StringBuilder();
        
        status.AppendLine($"Bluetooth: {(BluetoothManager.Instance.IsBluetoothEnabled() ? "ON" : "OFF")}");
        status.AppendLine($"Scanning: {(m_IsScanning ? "YES" : "NO")}");
        status.AppendLine($"Discovered: {m_DeviceItems.Count} devices");
        status.AppendLine($"Selected: {(m_SelectedDevice != null ? m_SelectedDevice.name : "None")}");
        status.AppendLine($"Connected: {(m_IsConnected ? "YES" : "NO")}");
        
        if (m_IsConnected)
        {
            BluetoothDevice connectedDevice = BluetoothManager.Instance.GetConnectedDevice(m_ConnectedDeviceId);
            if (connectedDevice != null)
            {
                status.AppendLine($"Device: {connectedDevice.name}");
                status.AppendLine($"Signal: {connectedDevice.rssi} dBm");
            }
        }
        
        return status.ToString();
    }
    
    // Public method to manually trigger status display (can be called from UI button)
    public void ShowDetailedStatus()
    {
        LogToDebugConsole("🔍 DETAILED STATUS:");
        DisplayConnectionSummary();
        
        LogToDebugConsole($"📱 Discovered Devices ({m_DeviceItems.Count}):");
        foreach (GameObject item in m_DeviceItems)
        {
            BLEDeviceItem deviceItem = item.GetComponent<BLEDeviceItem>();
            if (deviceItem != null)
            {
                BluetoothDevice device = BluetoothManager.Instance.GetDiscoveredDevice(deviceItem.DeviceId);
                if (device != null)
                {
                    LogToDebugConsole($"   • {device.name} (RSSI: {device.rssi} dBm)");
                    LogToDebugConsole($"     ID: {device.deviceId}");
                }
            }
        }
    }
    
    private void UpdateButtonStates()
    {
        bool bluetoothEnabled = BluetoothManager.Instance.IsBluetoothEnabled();
        
        // Scan Button
        if (m_ScanButton != null)
        {
            m_ScanButton.interactable = bluetoothEnabled && !m_IsConnected;
            
            if (m_ScanButton.TryGetComponent<Image>(out Image scanImage))
            {
                if (!bluetoothEnabled || m_IsConnected)
                    scanImage.color = m_DisabledColor;
                else if (m_IsScanning)
                    scanImage.color = m_ScanningColor;
                else
                    scanImage.color = m_EnabledColor;
            }
            
            // Update button text
            if (m_ScanButton.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI scanText))
            {
                scanText.text = m_IsScanning ? "Stop Scan" : "Scan";
            }
            else if (m_ScanButton.GetComponentInChildren<TextMeshProUGUI>() != null)
            {
                m_ScanButton.GetComponentInChildren<TextMeshProUGUI>().text = m_IsScanning ? "Stop Scan" : "Scan";
            }
        }
        
        // Connect Button
        if (m_ConnectButton != null)
        {
            m_ConnectButton.interactable = bluetoothEnabled && m_SelectedDevice != null && !m_IsConnected;
            
            if (m_ConnectButton.TryGetComponent<Image>(out Image connectImage))
            {
                connectImage.color = m_ConnectButton.interactable ? m_EnabledColor : m_DisabledColor;
            }
        }
        
        // Disconnect Button
        if (m_DisconnectButton != null)
        {
            m_DisconnectButton.interactable = m_IsConnected;
            
            if (m_DisconnectButton.TryGetComponent<Image>(out Image disconnectImage))
            {
                disconnectImage.color = m_IsConnected ? m_ConnectedColor : m_DisabledColor;
            }
        }
    }
}
