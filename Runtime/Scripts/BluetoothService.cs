using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class BluetoothService
    {
        public string serviceUUID;
        public int characteristicCount;

        public BluetoothService()
        {
            characteristicCount = 0;
        }

        public BluetoothService(string serviceUUID, int characteristicCount = 0)
        {
            this.serviceUUID = serviceUUID;
            this.characteristicCount = characteristicCount;
        }

        public override string ToString()
        {
            return $"BluetoothService(UUID: {serviceUUID}, Characteristics: {characteristicCount})";
        }
    }
}
