using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class BluetoothDevice
    {
        public string deviceId;
        public string name;
        public int rssi;
        public bool isConnectable;
        public string[] serviceUUIDs;
        public string manufacturerData;
        public string localName;
        public int txPowerLevel;

        public BluetoothDevice()
        {
            serviceUUIDs = new string[0];
            manufacturerData = "";
            localName = "";
            txPowerLevel = 0;
        }

        public BluetoothDevice(string deviceId, string name, int rssi = 0, bool isConnectable = true)
        {
            this.deviceId = deviceId;
            this.name = name;
            this.rssi = rssi;
            this.isConnectable = isConnectable;
            this.serviceUUIDs = new string[0];
            this.manufacturerData = "";
            this.localName = "";
            this.txPowerLevel = 0;
        }

        public override string ToString()
        {
            return $"BluetoothDevice(ID: {deviceId}, Name: {name}, RSSI: {rssi}, LocalName: {localName})";
        }
    }
}
