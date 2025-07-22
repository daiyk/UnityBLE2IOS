using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class BluetoothCharacteristic
    {
        public string serviceUUID;
        public string characteristicUUID;
        public string[] properties;
        public bool isNotifying;

        public BluetoothCharacteristic()
        {
            properties = new string[0];
            isNotifying = false;
        }

        public BluetoothCharacteristic(string serviceUUID, string characteristicUUID, string[] properties, bool isNotifying = false)
        {
            this.serviceUUID = serviceUUID;
            this.characteristicUUID = characteristicUUID;
            this.properties = properties ?? new string[0];
            this.isNotifying = isNotifying;
        }

        public bool CanRead()
        {
            return Array.IndexOf(properties, "read") >= 0;
        }

        public bool CanWrite()
        {
            return Array.IndexOf(properties, "write") >= 0 || Array.IndexOf(properties, "writeWithoutResponse") >= 0;
        }

        public bool CanNotify()
        {
            return Array.IndexOf(properties, "notify") >= 0 || Array.IndexOf(properties, "indicate") >= 0;
        }

        public bool HasWriteWithoutResponse()
        {
            return Array.IndexOf(properties, "writeWithoutResponse") >= 0;
        }

        public override string ToString()
        {
            return $"BluetoothCharacteristic(UUID: {characteristicUUID}, Service: {serviceUUID}, Properties: [{string.Join(", ", properties)}], Notifying: {isNotifying})";
        }
    }
}
