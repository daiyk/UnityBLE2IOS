using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class CharacteristicValueMessage
    {
        public string deviceId;
        public string characteristicUUID;
        public string data; // Hex string

        public CharacteristicValueMessage()
        {
            data = "";
        }

        public CharacteristicValueMessage(string deviceId, string characteristicUUID, string data)
        {
            this.deviceId = deviceId;
            this.characteristicUUID = characteristicUUID;
            this.data = data ?? "";
        }

        /// <summary>
        /// Converts the hex string data to byte array
        /// </summary>
        /// <returns>Byte array representation of the hex data</returns>
        public byte[] GetDataAsBytes()
        {
            if (string.IsNullOrEmpty(data) || data.Length % 2 != 0)
                return new byte[0];

            byte[] bytes = new byte[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Gets the data length in bytes
        /// </summary>
        /// <returns>Number of bytes in the data</returns>
        public int GetDataLength()
        {
            return string.IsNullOrEmpty(data) ? 0 : data.Length / 2;
        }

        public override string ToString()
        {
            return $"CharacteristicValueMessage(Device: {deviceId}, Characteristic: {characteristicUUID}, Data: {data})";
        }
    }
}
