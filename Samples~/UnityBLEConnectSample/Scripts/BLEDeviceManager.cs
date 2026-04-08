using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityBLE2IOS;

namespace UnityBLE2IOS.Samples.Connect
{
public class BLEDeviceManager : MonoBehaviour
{
    [SerializeField]
    private Button m_ScanButton;
    [SerializeField]
    private Button m_ConnectButton;
    [SerializeField]
    private Button m_DisconnectButton;
    [SerializeField]
    private Button m_InspectButton;
    [SerializeField]
    private Button m_SubscribeButton;
    [SerializeField]
    private TMP_Dropdown m_ServicesDropdown;
    [SerializeField]
    private TMP_Dropdown m_CharacteristicsDropdown;
    [SerializeField]
    private GameObject m_ItemContainer;
    [SerializeField]
    private GameObject m_DeviceItemPrefab;

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
    private TextMeshProUGUI m_DebugConsole;
    [SerializeField]
    private ScrollRect m_DebugScrollRect;
    [SerializeField]
    private int m_MaxDebugLines = 30;

    private readonly List<GameObject> m_DeviceItems = new List<GameObject>();
    private readonly List<string> m_DebugMessages = new List<string>();

    private BluetoothManager m_CachedManager;
    private BluetoothDevice m_SelectedDevice;
    private BluetoothService[] m_CurrentServices = Array.Empty<BluetoothService>();
    private BluetoothCharacteristic[] m_CurrentCharacteristics = Array.Empty<BluetoothCharacteristic>();
    private BluetoothService m_SelectedService;
    private BluetoothCharacteristic m_SelectedCharacteristic;

    private bool m_IsScanning;
    private bool m_IsConnected;
    private bool m_HasPermission;
    private string m_ConnectedDeviceId = string.Empty;
    private string m_SubscribedCharacteristicId = string.Empty;

    private void Start()
    {
        try
        {
            var manager = BluetoothManager.Instance;
            if (manager == null)
            {
                LogToDebugConsole("Failed to get BluetoothManager instance");
                return;
            }

            m_CachedManager = manager;
            SubscribeToManagerEvents(manager);
            BindUiEvents();

            SetStaticButtonLabels();
            ClearServicesDropdown();
            ClearCharacteristicsDropdown();
            UpdateButtonStates();

            manager.RequestPermissions();

            LogToDebugConsole("Generic BLE manager initialized");
            LogToDebugConsole($"Platform: {Application.platform}");
        }
        catch (Exception exception)
        {
            LogToDebugConsole($"Initialization error: {exception.Message}");
            Debug.LogError($"BLEDeviceManager initialization error: {exception}");
        }
    }

    private void OnDestroy()
    {
        UnbindUiEvents();

        if (m_CachedManager == null)
        {
            return;
        }

        try
        {
            UnsubscribeFromManagerEvents(m_CachedManager);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Error unsubscribing BLE manager events: {exception.Message}");
        }
        finally
        {
            m_CachedManager = null;
        }
    }

    private void SubscribeToManagerEvents(BluetoothManager manager)
    {
        manager.OnBluetoothStateChanged += OnBluetoothStateChanged;
        manager.OnDeviceDiscovered += OnDeviceDiscovered;
        manager.OnDeviceConnected += OnDeviceConnected;
        manager.OnServicesDiscovered += OnServicesDiscovered;
        manager.OnDeviceDisconnected += OnDeviceDisconnected;
        manager.OnConnectionFailed += OnConnectionFailed;
        manager.OnPermissionResult += OnPermissionResult;
        manager.OnCharacteristicValueReceived += OnCharacteristicValueReceived;
        manager.OnCharacteristicReadSuccess += OnCharacteristicReadSuccess;
        manager.OnCharacteristicReadError += OnCharacteristicReadError;
        manager.OnCharacteristicNotificationStateChanged += OnCharacteristicNotificationStateChanged;
    }

    private void UnsubscribeFromManagerEvents(BluetoothManager manager)
    {
        manager.OnBluetoothStateChanged -= OnBluetoothStateChanged;
        manager.OnDeviceDiscovered -= OnDeviceDiscovered;
        manager.OnDeviceConnected -= OnDeviceConnected;
        manager.OnServicesDiscovered -= OnServicesDiscovered;
        manager.OnDeviceDisconnected -= OnDeviceDisconnected;
        manager.OnConnectionFailed -= OnConnectionFailed;
        manager.OnPermissionResult -= OnPermissionResult;
        manager.OnCharacteristicValueReceived -= OnCharacteristicValueReceived;
        manager.OnCharacteristicReadSuccess -= OnCharacteristicReadSuccess;
        manager.OnCharacteristicReadError -= OnCharacteristicReadError;
        manager.OnCharacteristicNotificationStateChanged -= OnCharacteristicNotificationStateChanged;
    }

    private void BindUiEvents()
    {
        if (m_ScanButton != null)
        {
            m_ScanButton.onClick.AddListener(OnScanButtonClicked);
        }

        if (m_ConnectButton != null)
        {
            m_ConnectButton.onClick.AddListener(OnConnectButtonClicked);
        }

        if (m_DisconnectButton != null)
        {
            m_DisconnectButton.onClick.AddListener(OnDisconnectButtonClicked);
        }

        if (m_InspectButton != null)
        {
            m_InspectButton.onClick.AddListener(OnInspectButtonClicked);
        }

        if (m_SubscribeButton != null)
        {
            m_SubscribeButton.onClick.AddListener(OnSubscribeButtonClicked);
        }

        if (m_ServicesDropdown != null)
        {
            m_ServicesDropdown.onValueChanged.AddListener(OnServicesDropdownChanged);
        }

        if (m_CharacteristicsDropdown != null)
        {
            m_CharacteristicsDropdown.onValueChanged.AddListener(OnCharacteristicsDropdownChanged);
        }
    }

    private void UnbindUiEvents()
    {
        if (m_ScanButton != null)
        {
            m_ScanButton.onClick.RemoveListener(OnScanButtonClicked);
        }

        if (m_ConnectButton != null)
        {
            m_ConnectButton.onClick.RemoveListener(OnConnectButtonClicked);
        }

        if (m_DisconnectButton != null)
        {
            m_DisconnectButton.onClick.RemoveListener(OnDisconnectButtonClicked);
        }

        if (m_InspectButton != null)
        {
            m_InspectButton.onClick.RemoveListener(OnInspectButtonClicked);
        }

        if (m_SubscribeButton != null)
        {
            m_SubscribeButton.onClick.RemoveListener(OnSubscribeButtonClicked);
        }

        if (m_ServicesDropdown != null)
        {
            m_ServicesDropdown.onValueChanged.RemoveListener(OnServicesDropdownChanged);
        }

        if (m_CharacteristicsDropdown != null)
        {
            m_CharacteristicsDropdown.onValueChanged.RemoveListener(OnCharacteristicsDropdownChanged);
        }
    }

    private void OnScanButtonClicked()
    {
        if (m_IsScanning)
        {
            StopScanning();
            return;
        }

        StartScanning();
    }

    private void OnConnectButtonClicked()
    {
        if (m_SelectedDevice == null)
        {
            LogToDebugConsole("Select a BLE device before connecting");
            return;
        }

        if (!m_SelectedDevice.isConnectable)
        {
            LogToDebugConsole("Selected device reports itself as not connectable");
            return;
        }

        m_CachedManager.ConnectToDevice(m_SelectedDevice.deviceId);
        LogToDebugConsole($"Connecting to {GetBestDeviceName(m_SelectedDevice)}");
    }

    private void OnDisconnectButtonClicked()
    {
        if (!m_IsConnected || string.IsNullOrEmpty(m_ConnectedDeviceId))
        {
            return;
        }

        m_CachedManager.DisconnectDevice(m_ConnectedDeviceId);
    }

    private void OnInspectButtonClicked()
    {
        if (!m_IsConnected || string.IsNullOrEmpty(m_ConnectedDeviceId))
        {
            LogToDebugConsole("Connect to a device before inspecting it");
            return;
        }

        LogConnectedDeviceSummary();

        if (m_SelectedCharacteristic == null)
        {
            LogToDebugConsole("Select a characteristic to inspect");
            return;
        }

        LogCharacteristicDetails(m_SelectedCharacteristic);

        if (!m_SelectedCharacteristic.CanRead())
        {
            LogToDebugConsole("Selected characteristic does not support read. Use Subscribe for notify/indicate characteristics.");
            return;
        }

        m_CachedManager.ReadCharacteristic(m_ConnectedDeviceId, m_SelectedCharacteristic.characteristicUUID);
        LogToDebugConsole($"Read requested for characteristic {m_SelectedCharacteristic.characteristicUUID}.");
    }

    private void OnSubscribeButtonClicked()
    {
        if (!m_IsConnected || string.IsNullOrEmpty(m_ConnectedDeviceId))
        {
            LogToDebugConsole("Connect to a device before subscribing");
            return;
        }

        if (m_SelectedCharacteristic == null)
        {
            LogToDebugConsole("Select a characteristic before subscribing");
            return;
        }

        if (!m_SelectedCharacteristic.CanNotify())
        {
            LogToDebugConsole("Selected characteristic does not support notify or indicate");
            return;
        }

        if (m_SubscribedCharacteristicId == m_SelectedCharacteristic.characteristicUUID)
        {
            m_CachedManager.UnsubscribeFromCharacteristic(m_ConnectedDeviceId, m_SelectedCharacteristic.characteristicUUID);
            return;
        }

        if (!string.IsNullOrEmpty(m_SubscribedCharacteristicId))
        {
            m_CachedManager.UnsubscribeFromCharacteristic(m_ConnectedDeviceId, m_SubscribedCharacteristicId);
        }

        m_CachedManager.SubscribeToCharacteristic(m_ConnectedDeviceId, m_SelectedCharacteristic.characteristicUUID);
    }

    private void OnServicesDropdownChanged(int index)
    {
        if (index < 0 || index >= m_CurrentServices.Length)
        {
            m_SelectedService = null;
            ClearCharacteristicsDropdown();
            UpdateButtonStates();
            return;
        }

        m_SelectedService = m_CurrentServices[index];
        PopulateCharacteristicsDropdown(m_SelectedService.serviceUUID);
        LogToDebugConsole($"Selected service: {m_SelectedService.serviceUUID}");
    }

    private void OnCharacteristicsDropdownChanged(int index)
    {
        if (index < 0 || index >= m_CurrentCharacteristics.Length)
        {
            m_SelectedCharacteristic = null;
            UpdateButtonStates();
            return;
        }

        m_SelectedCharacteristic = m_CurrentCharacteristics[index];
        LogCharacteristicDetails(m_SelectedCharacteristic);
        UpdateButtonStates();
    }

    private void StartScanning()
    {
        ClearDeviceList();
        ClearConnectionData();
        m_CachedManager.ClearDiscoveredDevices();
        m_CachedManager.StartScanning();
        m_IsScanning = true;
        UpdateButtonStates();
        LogToDebugConsole("Started scanning for BLE devices");
    }

    private void StopScanning()
    {
        m_CachedManager.StopScanning();
        m_IsScanning = false;
        UpdateButtonStates();
        LogToDebugConsole($"Stopped scanning. Devices listed: {m_DeviceItems.Count}");
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

    private void ClearConnectionData()
    {
        m_IsConnected = false;
        m_ConnectedDeviceId = string.Empty;
        m_SubscribedCharacteristicId = string.Empty;
        m_SelectedService = null;
        m_SelectedCharacteristic = null;
        m_CurrentServices = Array.Empty<BluetoothService>();
        m_CurrentCharacteristics = Array.Empty<BluetoothCharacteristic>();
        ClearServicesDropdown();
        ClearCharacteristicsDropdown();
    }

    private void OnBluetoothStateChanged(bool enabled)
    {
        LogToDebugConsole($"Bluetooth state changed: {(enabled ? "Enabled" : "Disabled")}");
        UpdateButtonStates();
    }

    private void OnPermissionResult(bool granted)
    {
        m_HasPermission = granted;
        LogToDebugConsole(granted ? "Bluetooth permissions granted" : "Bluetooth permissions denied");
        UpdateButtonStates();
    }

    private void OnDeviceDiscovered(BluetoothDevice device)
    {
        for (int index = 0; index < m_DeviceItems.Count; index++)
        {
            BLEDeviceItem existingItem = m_DeviceItems[index].GetComponent<BLEDeviceItem>();
            if (existingItem != null && existingItem.DeviceId == device.deviceId)
            {
                existingItem.UpdateDeviceInfo(device);
                return;
            }
        }

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
            else
            {
                LogToDebugConsole("Device item prefab is missing BLEDeviceItem");
                Destroy(newItem);
            }
        }

        LogToDebugConsole($"Found device: {GetBestDeviceName(device)} ({device.deviceId}) RSSI {device.rssi} dBm");
        UpdateButtonStates();
    }

    private void OnDeviceSelected(BluetoothDevice device)
    {
        m_SelectedDevice = device;
        LogToDebugConsole($"Selected device: {GetBestDeviceName(device)}");
        LogDiscoveredDeviceSummary(device);
        UpdateButtonStates();
    }

    private void OnDeviceConnected(string deviceId)
    {
        m_IsConnected = true;
        m_ConnectedDeviceId = deviceId;
        m_SelectedDevice = m_CachedManager.GetConnectedDevice(deviceId) ?? m_CachedManager.GetDiscoveredDevice(deviceId) ?? m_SelectedDevice;

        if (m_IsScanning)
        {
            StopScanning();
        }

        foreach (GameObject item in m_DeviceItems)
        {
            BLEDeviceItem deviceItem = item.GetComponent<BLEDeviceItem>();
            if (deviceItem == null)
            {
                continue;
            }

            deviceItem.SetConnectionStatus(deviceItem.DeviceId == deviceId);
        }

        ClearServicesDropdown();
        ClearCharacteristicsDropdown();
        LogToDebugConsole($"Connected to device: {deviceId}");
        LogConnectedDeviceSummary();
        UpdateButtonStates();
    }

    private void OnServicesDiscovered(string deviceId)
    {
        if (deviceId != m_ConnectedDeviceId)
        {
            return;
        }

        m_CurrentServices = m_CachedManager.GetDeviceServices(deviceId) ?? Array.Empty<BluetoothService>();
        m_SelectedService = null;
        m_SelectedCharacteristic = null;
        m_CurrentCharacteristics = Array.Empty<BluetoothCharacteristic>();

        PopulateServicesDropdown();
        LogToDebugConsole($"GATT services discovered: {m_CurrentServices.Length}");

        for (int index = 0; index < m_CurrentServices.Length; index++)
        {
            BluetoothService service = m_CurrentServices[index];
            LogToDebugConsole($"Service {index + 1}: {service.serviceUUID} ({service.characteristicCount} characteristics)");
        }

        if (m_CurrentServices.Length > 0)
        {
            m_ServicesDropdown.value = 0;
            OnServicesDropdownChanged(0);
        }

        UpdateButtonStates();
    }

    private void OnDeviceDisconnected(string deviceId)
    {
        if (deviceId == m_ConnectedDeviceId)
        {
            ClearConnectionData();
        }

        foreach (GameObject item in m_DeviceItems)
        {
            BLEDeviceItem deviceItem = item.GetComponent<BLEDeviceItem>();
            if (deviceItem == null)
            {
                continue;
            }

            if (deviceItem.DeviceId == deviceId)
            {
                deviceItem.SetConnectionStatus(false);
            }
            else
            {
                deviceItem.RefreshDeviceData();
            }
        }

        LogToDebugConsole($"Disconnected from device: {deviceId}");
        UpdateButtonStates();
    }

    private void OnConnectionFailed(string deviceId, string error)
    {
        LogToDebugConsole($"Connection failed for {deviceId}: {error}");
        UpdateButtonStates();
    }

    private void OnCharacteristicValueReceived(CharacteristicValueMessage message)
    {
        if (message == null || message.deviceId != m_ConnectedDeviceId)
        {
            return;
        }

        byte[] bytes = message.GetDataAsBytes();
        string utf8 = SanitizeTextPayload(message.GetDataAsString());

        LogToDebugConsole($"Characteristic value from {message.characteristicUUID}: {message.data} ({bytes.Length} bytes)");
        if (!string.IsNullOrEmpty(utf8))
        {
            LogToDebugConsole($"Text payload: {utf8}");
        }
    }

    private void OnCharacteristicReadSuccess(CharacteristicReadResult result)
    {
        if (result == null || result.deviceId != m_ConnectedDeviceId)
        {
            return;
        }

        byte[] bytes = result.GetDataAsBytes();
        string utf8 = SanitizeTextPayload(result.GetDataAsString());

        LogToDebugConsole($"Characteristic read from {result.characteristicUUID}: {result.data} ({bytes.Length} bytes)");
        if (!string.IsNullOrEmpty(utf8))
        {
            LogToDebugConsole($"Text payload: {utf8}");
        }
    }

    private void OnCharacteristicReadError(CharacteristicReadResult result)
    {
        if (result == null || result.deviceId != m_ConnectedDeviceId)
        {
            return;
        }

        LogToDebugConsole($"Characteristic read failed for {result.characteristicUUID}: {result.error}");
    }

    private void OnCharacteristicNotificationStateChanged(CharacteristicNotificationStateResult result)
    {
        if (result == null || result.deviceId != m_ConnectedDeviceId)
        {
            return;
        }

        if (result.IsError())
        {
            LogToDebugConsole($"Notification state change failed for {result.characteristicUUID}: {result.error}");
            UpdateButtonStates();
            return;
        }

        if (result.isNotifying)
        {
            m_SubscribedCharacteristicId = result.characteristicUUID;
            LogToDebugConsole($"Subscribed to characteristic: {result.characteristicUUID}");
        }
        else
        {
            if (m_SubscribedCharacteristicId == result.characteristicUUID)
            {
                m_SubscribedCharacteristicId = string.Empty;
                LogToDebugConsole($"Unsubscribed from characteristic: {result.characteristicUUID}");
            }
        }

        UpdateButtonStates();
    }

    private void PopulateServicesDropdown()
    {
        if (m_ServicesDropdown == null)
        {
            return;
        }

        m_ServicesDropdown.ClearOptions();

        if (m_CurrentServices.Length == 0)
        {
            m_ServicesDropdown.options.Add(new TMP_Dropdown.OptionData("No services discovered"));
        }
        else
        {
            foreach (BluetoothService service in m_CurrentServices)
            {
                string displayName = $"{service.serviceUUID} ({service.characteristicCount} chars)";
                m_ServicesDropdown.options.Add(new TMP_Dropdown.OptionData(displayName));
            }
        }

        m_ServicesDropdown.RefreshShownValue();
        m_ServicesDropdown.value = 0;
        ConfigureDropdownFontSize(m_ServicesDropdown);
    }

    private void PopulateCharacteristicsDropdown(string serviceUuid)
    {
        if (m_CharacteristicsDropdown == null || string.IsNullOrEmpty(serviceUuid) || string.IsNullOrEmpty(m_ConnectedDeviceId))
        {
            return;
        }

        m_CurrentCharacteristics = m_CachedManager.GetServiceCharacteristics(m_ConnectedDeviceId, serviceUuid) ?? Array.Empty<BluetoothCharacteristic>();
        m_SelectedCharacteristic = null;
        m_CharacteristicsDropdown.ClearOptions();

        if (m_CurrentCharacteristics.Length == 0)
        {
            m_CharacteristicsDropdown.options.Add(new TMP_Dropdown.OptionData("No characteristics discovered"));
        }
        else
        {
            foreach (BluetoothCharacteristic characteristic in m_CurrentCharacteristics)
            {
                string properties = GetPropertySummary(characteristic);
                string displayName = string.IsNullOrEmpty(properties)
                    ? characteristic.characteristicUUID
                    : $"{characteristic.characteristicUUID} [{properties}]";
                m_CharacteristicsDropdown.options.Add(new TMP_Dropdown.OptionData(displayName));
            }
        }

        m_CharacteristicsDropdown.RefreshShownValue();
        m_CharacteristicsDropdown.value = 0;
        ConfigureDropdownFontSize(m_CharacteristicsDropdown);

        if (m_CurrentCharacteristics.Length > 0)
        {
            m_SelectedCharacteristic = m_CurrentCharacteristics[0];
            LogCharacteristicDetails(m_SelectedCharacteristic);
        }

        UpdateButtonStates();
    }

    private void ClearServicesDropdown()
    {
        m_CurrentServices = Array.Empty<BluetoothService>();
        m_SelectedService = null;

        if (m_ServicesDropdown == null)
        {
            return;
        }

        m_ServicesDropdown.ClearOptions();
        m_ServicesDropdown.options.Add(new TMP_Dropdown.OptionData("No services discovered"));
        m_ServicesDropdown.value = 0;
        m_ServicesDropdown.RefreshShownValue();
        ConfigureDropdownFontSize(m_ServicesDropdown);
    }

    private void ClearCharacteristicsDropdown()
    {
        m_CurrentCharacteristics = Array.Empty<BluetoothCharacteristic>();
        m_SelectedCharacteristic = null;

        if (m_CharacteristicsDropdown == null)
        {
            return;
        }

        m_CharacteristicsDropdown.ClearOptions();
        m_CharacteristicsDropdown.options.Add(new TMP_Dropdown.OptionData("No characteristics discovered"));
        m_CharacteristicsDropdown.value = 0;
        m_CharacteristicsDropdown.RefreshShownValue();
        ConfigureDropdownFontSize(m_CharacteristicsDropdown);
    }

    private void ConfigureDropdownFontSize(TMP_Dropdown dropdown)
    {
        if (dropdown == null)
        {
            return;
        }

        if (dropdown.captionText != null)
        {
            dropdown.captionText.fontSize = 32f;
        }

        if (dropdown.itemText != null)
        {
            dropdown.itemText.fontSize = 28f;
        }
    }

    private void LogDiscoveredDeviceSummary(BluetoothDevice device)
    {
        if (device == null)
        {
            return;
        }

        LogToDebugConsole($"Device ID: {device.deviceId}");
        LogToDebugConsole($"Name: {GetBestDeviceName(device)}");
        LogToDebugConsole($"RSSI: {device.rssi} dBm");
        LogToDebugConsole($"Connectable: {(device.isConnectable ? "Yes" : "No")}");

        if (!string.IsNullOrWhiteSpace(device.localName))
        {
            LogToDebugConsole($"Local Name: {device.localName}");
        }

        if (!string.IsNullOrWhiteSpace(device.manufacturerData))
        {
            LogToDebugConsole($"Manufacturer Data: {device.manufacturerData}");
        }

        if (device.txPowerLevel != 0)
        {
            LogToDebugConsole($"TX Power: {device.txPowerLevel}");
        }

        if (device.serviceUUIDs != null && device.serviceUUIDs.Length > 0)
        {
            LogToDebugConsole($"Advertised Services: {string.Join(", ", device.serviceUUIDs)}");
        }
    }

    private void LogConnectedDeviceSummary()
    {
        if (string.IsNullOrEmpty(m_ConnectedDeviceId))
        {
            return;
        }

        BluetoothDevice connectedDevice = m_CachedManager.GetConnectedDevice(m_ConnectedDeviceId)
            ?? m_CachedManager.GetDiscoveredDevice(m_ConnectedDeviceId)
            ?? m_SelectedDevice;

        LogToDebugConsole("Connected device summary:");
        LogDiscoveredDeviceSummary(connectedDevice);
        LogToDebugConsole($"GATT Ready: {(m_CachedManager.IsGattReady(m_ConnectedDeviceId) ? "Yes" : "No")}");
        LogToDebugConsole($"Service Count: {m_CurrentServices.Length}");

        if (!string.IsNullOrEmpty(m_SubscribedCharacteristicId))
        {
            LogToDebugConsole($"Subscribed Characteristic: {m_SubscribedCharacteristicId}");
        }
    }

    private void LogCharacteristicDetails(BluetoothCharacteristic characteristic)
    {
        if (characteristic == null)
        {
            return;
        }

        LogToDebugConsole($"Characteristic: {characteristic.characteristicUUID}");
        LogToDebugConsole($"Service: {characteristic.serviceUUID}");
        LogToDebugConsole($"Properties: {GetPropertySummary(characteristic)}");
        LogToDebugConsole($"Can Read: {(characteristic.CanRead() ? "Yes" : "No")}");
        LogToDebugConsole($"Can Notify: {(characteristic.CanNotify() ? "Yes" : "No")}");
        LogToDebugConsole($"Can Write: {(characteristic.CanWrite() ? "Yes" : "No")}");
        LogToDebugConsole($"Is Notifying: {(m_SubscribedCharacteristicId == characteristic.characteristicUUID ? "Yes" : "No")}");
    }

    private void LogToDebugConsole(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";

        m_DebugMessages.Add(logMessage);
        while (m_DebugMessages.Count > m_MaxDebugLines)
        {
            m_DebugMessages.RemoveAt(0);
        }

        if (m_DebugConsole != null)
        {
            m_DebugConsole.text = string.Join("\n", m_DebugMessages);
        }

        if (m_DebugScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            m_DebugScrollRect.verticalNormalizedPosition = 0f;
        }

        Debug.Log(logMessage);
    }

    private void SetStaticButtonLabels()
    {
        SetButtonText(m_InspectButton, "Inspect");
        SetButtonText(m_SubscribeButton, "Subscribe");
    }

    private void UpdateButtonStates()
    {
        bool bluetoothEnabled = m_CachedManager != null && m_CachedManager.IsBluetoothEnabled();
        bool canScan = bluetoothEnabled && m_HasPermission && !m_IsConnected;
        bool hasSelection = m_SelectedDevice != null;
        bool hasCharacteristicSelection = m_SelectedCharacteristic != null;
        bool canInspect = m_IsConnected && hasCharacteristicSelection && m_SelectedCharacteristic.CanRead();
        bool canSubscribe = m_IsConnected && hasCharacteristicSelection && m_SelectedCharacteristic.CanNotify();
        bool isSelectedCharacteristicSubscribed =
            hasCharacteristicSelection &&
            m_SelectedCharacteristic.characteristicUUID == m_SubscribedCharacteristicId;

        UpdateButtonVisualState(m_ScanButton, canScan, m_IsScanning ? m_ScanningColor : m_EnabledColor);
        SetButtonText(m_ScanButton, m_IsScanning ? "Stop Scan" : "Scan");

        bool canConnect = bluetoothEnabled && m_HasPermission && hasSelection && !m_IsConnected && m_SelectedDevice.isConnectable;
        UpdateButtonVisualState(m_ConnectButton, canConnect, m_EnabledColor);

        UpdateButtonVisualState(m_DisconnectButton, m_IsConnected, m_ConnectedColor);
        UpdateButtonVisualState(m_InspectButton, canInspect, m_EnabledColor);
        UpdateButtonVisualState(m_SubscribeButton, canSubscribe, m_ConnectedColor);
        SetButtonText(m_SubscribeButton, isSelectedCharacteristicSubscribed ? "Unsubscribe" : "Subscribe");

        if (m_ServicesDropdown != null)
        {
            m_ServicesDropdown.interactable = m_IsConnected && m_CurrentServices.Length > 0;
        }

        if (m_CharacteristicsDropdown != null)
        {
            m_CharacteristicsDropdown.interactable = m_IsConnected && m_CurrentCharacteristics.Length > 0;
        }
    }

    private void UpdateButtonVisualState(Button button, bool interactable, Color enabledColor)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = interactable;
        if (button.TryGetComponent<Image>(out Image image))
        {
            image.color = interactable ? enabledColor : m_DisabledColor;
        }
    }

    private void SetButtonText(Button button, string text)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI buttonText = button.GetComponent<TextMeshProUGUI>();
        if (buttonText == null)
        {
            buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    private static string GetBestDeviceName(BluetoothDevice device)
    {
        if (device == null)
        {
            return "Unknown Device";
        }

        if (!string.IsNullOrWhiteSpace(device.name))
        {
            return device.name;
        }

        if (!string.IsNullOrWhiteSpace(device.localName))
        {
            return device.localName;
        }

        return "Unknown Device";
    }

    private static string GetPropertySummary(BluetoothCharacteristic characteristic)
    {
        if (characteristic?.properties == null || characteristic.properties.Length == 0)
        {
            return "None";
        }

        return string.Join(", ", characteristic.properties);
    }

    private static string SanitizeTextPayload(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder(value.Length);
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (!char.IsControl(character) || character == '\r' || character == '\n' || character == '\t')
            {
                builder.Append(character);
            }
        }

        string sanitized = builder.ToString().Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? string.Empty : sanitized;
    }
}
}
