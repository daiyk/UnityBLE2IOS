using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class CharacteristicReadResult
    {
        public string deviceId;
        public string characteristicUUID;
        public string data;
        public string error;

        public CharacteristicReadResult()
        {
            data = string.Empty;
            error = string.Empty;
        }

        public CharacteristicReadResult(string deviceId, string characteristicUUID, string data = null, string error = null)
        {
            this.deviceId = deviceId;
            this.characteristicUUID = characteristicUUID;
            this.data = data ?? string.Empty;
            this.error = error ?? string.Empty;
        }

        public bool IsSuccess()
        {
            return string.IsNullOrEmpty(error);
        }

        public bool IsError()
        {
            return !string.IsNullOrEmpty(error);
        }

        public byte[] GetDataAsBytes()
        {
            if (string.IsNullOrEmpty(data) || data.Length % 2 != 0)
            {
                return Array.Empty<byte>();
            }

            byte[] bytes = new byte[data.Length / 2];
            for (int index = 0; index < data.Length; index += 2)
            {
                bytes[index / 2] = Convert.ToByte(data.Substring(index, 2), 16);
            }

            return bytes;
        }

        public string GetDataAsString()
        {
            try
            {
                byte[] bytes = GetDataAsBytes();
                return bytes.Length == 0 ? string.Empty : System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public override string ToString()
        {
            return IsSuccess()
                ? $"CharacteristicReadResult(Device: {deviceId}, Characteristic: {characteristicUUID}, Data: {data})"
                : $"CharacteristicReadResult(Device: {deviceId}, Characteristic: {characteristicUUID}, Error: {error})";
        }
    }
}