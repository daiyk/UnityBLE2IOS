using System;
using UnityEngine;

namespace UnityBLE2IOS
{
    [Serializable]
    public class CharacteristicWriteResult
    {
        public string deviceId;
        public string characteristicUUID;
        public string error; // Only present in error messages

        public CharacteristicWriteResult()
        {
            error = "";
        }

        public CharacteristicWriteResult(string deviceId, string characteristicUUID, string error = null)
        {
            this.deviceId = deviceId;
            this.characteristicUUID = characteristicUUID;
            this.error = error ?? "";
        }

        /// <summary>
        /// Indicates whether the write operation was successful
        /// </summary>
        /// <returns>True if successful (no error), false otherwise</returns>
        public bool IsSuccess()
        {
            return string.IsNullOrEmpty(error);
        }

        /// <summary>
        /// Indicates whether the write operation failed
        /// </summary>
        /// <returns>True if failed (has error), false otherwise</returns>
        public bool IsError()
        {
            return !string.IsNullOrEmpty(error);
        }

        public override string ToString()
        {
            if (IsSuccess())
            {
                return $"CharacteristicWriteResult(Device: {deviceId}, Characteristic: {characteristicUUID}, Status: Success)";
            }
            else
            {
                return $"CharacteristicWriteResult(Device: {deviceId}, Characteristic: {characteristicUUID}, Status: Error - {error})";
            }
        }
    }
}
