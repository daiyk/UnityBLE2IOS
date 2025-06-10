using UnityEngine;
using UnityEngine.UI;
using UnityBLE2IOS;
using TMPro;
using System;

public class BLEDeviceItem : MonoBehaviour
{
    [SerializeField]
    private Button m_ClickableText; // Reference to the Button component for component containing TextMeshProUGUI
    [SerializeField]
    private Color m_NormalColor = Color.white; // Normal state color
    [SerializeField]
    private Color m_SelectedColor = Color.green; // Selected state color
    
    private Image m_Image; // Reference to the Image component
    private static BLEDeviceItem s_CurrentlySelected; // Track currently selected item
    private BluetoothDevice m_DeviceInfo;
    
    public event Action<BluetoothDevice> OnDeviceSelected;
    public string DeviceId => m_DeviceInfo?.deviceId ?? "";
    
    void Start()
    {
        m_Image = GetComponent<Image>();
        m_Image.color = m_NormalColor;
        
        // Add click listener
        if (m_ClickableText != null)
        {
            m_ClickableText.onClick.AddListener(OnItemClicked);
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
    
    private void UpdateDisplayText()
    {
        if (m_DeviceInfo == null) return;
        
        string displayText = $"{m_DeviceInfo.name}\nRSSI: {m_DeviceInfo.rssi} dBm";
        
        // Find TextMeshProUGUI component in button or its children
        TextMeshProUGUI textComponent = null;
        if (m_ClickableText != null)
        {
            textComponent = m_ClickableText.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
                textComponent = m_ClickableText.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (textComponent != null)
        {
            textComponent.text = displayText;
        }
    }

    private void OnItemClicked()
    {
        SelectItem();
        OnDeviceSelected?.Invoke(m_DeviceInfo);
    }
    
    public void SelectItem()
    {
        // Deselect previously selected item
        if (s_CurrentlySelected != null && s_CurrentlySelected != this)
        {
            s_CurrentlySelected.DeselectItem();
        }
        
        // Select this item
        s_CurrentlySelected = this;
        m_Image.color = m_SelectedColor;
    }
    
    public void DeselectItem()
    {
        m_Image.color = m_NormalColor;
        if (s_CurrentlySelected == this)
        {
            s_CurrentlySelected = null;
        }
    }
}
