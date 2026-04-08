using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityBLE2IOS;

namespace UnityBLE2IOS.Samples.Connect
{
public class BLEDeviceItem : MonoBehaviour
{
    [SerializeField]
    private Button m_ClickableButton;
    [SerializeField]
    private Color m_NormalColor = Color.white;
    [SerializeField]
    private Color m_SelectedColor = Color.blue;
    [SerializeField]
    private Color m_ConnectedColor = Color.green;

    private static BLEDeviceItem s_CurrentlySelected;

    private Image m_BackgroundImage;
    private BluetoothDevice m_DeviceInfo;
    private bool m_IsConnected;

    public event Action<BluetoothDevice> OnDeviceSelected;

    public string DeviceId => m_DeviceInfo?.deviceId ?? string.Empty;
    public BluetoothDevice DeviceInfo => m_DeviceInfo;
    public bool IsConnected => m_IsConnected;

    private void Start()
    {
        m_BackgroundImage = GetComponent<Image>();
        if (m_BackgroundImage != null)
        {
            m_BackgroundImage.color = m_NormalColor;
        }

        if (m_ClickableButton != null)
        {
            m_ClickableButton.onClick.AddListener(OnItemClicked);
        }
    }

    private void OnDestroy()
    {
        if (m_ClickableButton != null)
        {
            m_ClickableButton.onClick.RemoveListener(OnItemClicked);
        }

        if (s_CurrentlySelected == this)
        {
            s_CurrentlySelected = null;
        }
    }

    public void SetDeviceInfo(BluetoothDevice deviceInfo)
    {
        m_DeviceInfo = deviceInfo;
        UpdateDisplayText();
    }

    public void UpdateDeviceInfo(BluetoothDevice deviceInfo)
    {
        m_DeviceInfo = deviceInfo;
        UpdateDisplayText();
    }

    public void SetConnectionStatus(bool isConnected)
    {
        m_IsConnected = isConnected;
        UpdateVisualState();
        UpdateDisplayText();
    }

    public void SelectItem()
    {
        if (s_CurrentlySelected != null && s_CurrentlySelected != this)
        {
            s_CurrentlySelected.DeselectItem();
        }

        s_CurrentlySelected = this;
        UpdateVisualState();
        OnDeviceSelected?.Invoke(m_DeviceInfo);
    }

    public void DeselectItem()
    {
        if (s_CurrentlySelected == this)
        {
            s_CurrentlySelected = null;
        }

        UpdateVisualState();
    }

    public void ForceDeselect()
    {
        if (s_CurrentlySelected == this)
        {
            s_CurrentlySelected = null;
        }

        m_IsConnected = false;
        UpdateVisualState();
    }

    public bool IsSelected()
    {
        return s_CurrentlySelected == this;
    }

    public void RefreshDeviceData()
    {
        if (m_DeviceInfo != null)
        {
            UpdateDisplayText();
        }
    }

    private void OnItemClicked()
    {
        SelectItem();
    }

    private void UpdateDisplayText()
    {
        if (m_DeviceInfo == null)
        {
            return;
        }

        string deviceName = GetBestDeviceName(m_DeviceInfo);
        string connectionStatus = m_IsConnected ? " (Connected)" : string.Empty;
        int advertisedServiceCount = m_DeviceInfo.serviceUUIDs?.Length ?? 0;

        string displayText =
            $"{deviceName}{connectionStatus}\n" +
            $"RSSI: {m_DeviceInfo.rssi} dBm\n" +
            $"Connectable: {(m_DeviceInfo.isConnectable ? "Yes" : "No")}\n" +
            $"Advertised Services: {advertisedServiceCount}";

        if (!string.IsNullOrWhiteSpace(m_DeviceInfo.localName) && m_DeviceInfo.localName != deviceName)
        {
            displayText += $"\nLocal Name: {m_DeviceInfo.localName}";
        }

        if (m_DeviceInfo.txPowerLevel != 0)
        {
            displayText += $"\nTX Power: {m_DeviceInfo.txPowerLevel}";
        }

        TextMeshProUGUI textComponent = null;
        if (m_ClickableButton != null)
        {
            textComponent = m_ClickableButton.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                textComponent = m_ClickableButton.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        if (textComponent != null)
        {
            textComponent.text = displayText;
        }
    }

    private void UpdateVisualState()
    {
        if (m_BackgroundImage == null)
        {
            return;
        }

        if (m_IsConnected)
        {
            m_BackgroundImage.color = m_ConnectedColor;
            return;
        }

        m_BackgroundImage.color = s_CurrentlySelected == this ? m_SelectedColor : m_NormalColor;
    }

    private static string GetBestDeviceName(BluetoothDevice device)
    {
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
}
}
