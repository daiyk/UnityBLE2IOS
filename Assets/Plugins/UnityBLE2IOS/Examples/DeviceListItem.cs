using System;
using UnityEngine;
using UnityEngine.UI;
using UnityBLE2IOS;

public class DeviceListItem : MonoBehaviour
{
    [Header("UI References")]
    public Text deviceNameText;
    public Text deviceIdText;
    public Text rssiText;
    public Button connectButton;
    public Image connectionStatusImage;

    [Header("Connection Status Colors")]
    public Color disconnectedColor = Color.red;
    public Color connectedColor = Color.green;

    private BluetoothDevice device;
    private Action<BluetoothDevice> onClickCallback;
    private bool isConnected = false;

    public void Setup(BluetoothDevice device, Action<BluetoothDevice> onClickCallback)
    {
        this.device = device;
        this.onClickCallback = onClickCallback;

        UpdateUI();
        
        if (connectButton != null)
        {
            connectButton.onClick.RemoveAllListeners();
            connectButton.onClick.AddListener(OnConnectButtonClicked);
        }
    }

    private void UpdateUI()
    {
        if (device == null) return;

        if (deviceNameText != null)
        {
            deviceNameText.text = string.IsNullOrEmpty(device.name) ? "Unknown Device" : device.name;
        }

        if (deviceIdText != null)
        {
            deviceIdText.text = device.deviceId;
        }

        if (rssiText != null)
        {
            rssiText.text = $"RSSI: {device.rssi} dBm";
        }

        UpdateConnectionStatusUI();
    }

    public void UpdateConnectionStatus(bool connected)
    {
        isConnected = connected;
        UpdateConnectionStatusUI();
    }

    private void UpdateConnectionStatusUI()
    {
        if (connectionStatusImage != null)
        {
            connectionStatusImage.color = isConnected ? connectedColor : disconnectedColor;
        }

        if (connectButton != null)
        {
            Text buttonText = connectButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isConnected ? "Disconnect" : "Connect";
            }
        }
    }

    private void OnConnectButtonClicked()
    {
        onClickCallback?.Invoke(device);
    }
}
